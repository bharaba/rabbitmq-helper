﻿using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using Thycotic.Logging;
using Thycotic.MessageQueue.Client.QueueClient;
using Thycotic.Messages.Common;
using Thycotic.Utility.Serialization;

namespace Thycotic.MessageQueue.Client.Wrappers
{
    /// <summary>
    /// RPC consumer wrapper
    /// </summary>
    /// <typeparam name="TConsumable">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TConsumer">The type of the handler.</typeparam>
    /// <seealso cref="Thycotic.MessageQueue.Client.Wrappers.ConsumerWrapperBase{TConsumable,TConsumer}" />
    public class BlockingConsumerWrapper<TConsumable, TResponse, TConsumer> : ConsumerWrapperBase<TConsumable, TConsumer>
        where TConsumable : class, IBlockingConsumable
        where TResponse : class
        where TConsumer : IBlockingConsumer<TConsumable, TResponse>
    {
        private readonly IObjectSerializer _objectSerializer;
        private readonly IMessageEncryptor _messageEncryptor;
        private readonly Func<Owned<TConsumer>> _consumerFactory;
        private readonly ICommonConnection _connection;

        private readonly ILogWriter _log = Log.Get(typeof(TConsumer));

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockingConsumerWrapper{TConsumable, TResponse, TConsumer}" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="exchangeNameProvider">The exchange name provider.</param>
        /// <param name="objectSerializer">The object serializer.</param>
        /// <param name="messageEncryptor">The message encryptor.</param>
        /// <param name="prioritySchedulerProvider">The priority scheduler provider.</param>
        /// <param name="consumerFactory">The handler factory.</param>
        public BlockingConsumerWrapper(ICommonConnection connection, IExchangeNameProvider exchangeNameProvider, IObjectSerializer objectSerializer,
            IMessageEncryptor messageEncryptor, IPrioritySchedulerProvider prioritySchedulerProvider, Func<Owned<TConsumer>> consumerFactory)
            : base(connection, exchangeNameProvider)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(exchangeNameProvider != null);
            Contract.Requires<ArgumentNullException>(objectSerializer != null);
            Contract.Requires<ArgumentNullException>(messageEncryptor != null);
            Contract.Requires<ArgumentNullException>(prioritySchedulerProvider != null);
            Contract.Requires<ArgumentNullException>(consumerFactory != null);

            _objectSerializer = objectSerializer;
            _messageEncryptor = messageEncryptor;
            _consumerFactory = consumerFactory;
            _connection = connection;
            PriorityScheduler = prioritySchedulerProvider.AboveNormal;
        }

        /// <summary>
        /// Starts the handle task.
        /// </summary>
        /// <param name="consumerTag">The consumer tag.</param>
        /// <param name="deliveryTag">The delivery tag.</param>
        /// <param name="redelivered">if set to <c>true</c> [redelivered].</param>
        /// <param name="exchange">The exchange.</param>
        /// <param name="routingKey">The routing key.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        protected override Task StartHandleTask(string consumerTag, DeliveryTagWrapper deliveryTag, bool redelivered, string exchange, string routingKey,
            ICommonModelProperties properties, byte[] body)
        {
            if (redelivered)
            {
                _log.Warn(string.Format("Blocking requests cannot be redelivered. Will not process message for {0}", routingKey));
                CommonModel.BasicNack(deliveryTag, exchange, routingKey, false, requeue: false);
                return Task.FromResult(false);
            }

            var task = Task.Factory.StartNew(() => ExecuteMessage(deliveryTag, exchange, routingKey, properties, body),
                ActiveTasks.Token,
                TaskCreationOptions.None,
                PriorityScheduler);

            ActiveTasks.AddTask(task);

            return task;

        }

        /// <summary>
        /// Executes the message.
        /// </summary>
        /// <param name="deliveryTag">The delivery tag.</param>
        /// <param name="exchangeName">The exchange.</param>
        /// <param name="routingKey">The routing key.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="body">The body.</param>
        private void ExecuteMessage(DeliveryTagWrapper deliveryTag, string exchangeName, string routingKey, ICommonModelProperties properties, byte[] body)
        {
            using (LogCorrelation.Create())
            {
                var responseType = BlockingConsumerResponseTypes.Success;
                object response;

                try
                {

                    TConsumable message;

                    try
                    {
                        _log.Debug("Decrypting and deserializing");
                        message = _objectSerializer.ToObject<TConsumable>(_messageEncryptor.Decrypt(exchangeName, body));

                    }
                    catch (Exception ex)
                    {
                        _log.Error("Failed to decrypt or deserialize message", ex);

                        throw;
                    }

                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(ActiveTasks.Token).Token;

                    try
                    {
                        PreConsume(linkedToken, message);

                        using (var consumer = _consumerFactory())
                        {
                            response = consumer.Value.Consume(linkedToken,  message);

                            _log.Debug(string.Format("Successfully processed {0}", this.GetRoutingKey(typeof(TConsumable))));
                        }
                    }
                    finally
                    {
                        PostConsume(linkedToken, message);
                    }

                    CommonModel.BasicAck(deliveryTag, exchangeName, routingKey, false);
                }
                catch (ObjectDisposedException)
                {
                    CommonModel.BasicNack(deliveryTag, exchangeName, routingKey, false, requeue: false);

                    response = new BlockingConsumerError { Message = "Engine shutting down" };
                    responseType = BlockingConsumerResponseTypes.Error;
                }
                catch (Exception ex)
                {
                    _log.Error(
                        string.Format("Failed to process {0} because {1}", this.GetRoutingKey(typeof(TConsumable)),
                            ex.Message), ex);

                    CommonModel.BasicNack(deliveryTag, exchangeName, routingKey, false, requeue: false);

                    response = new BlockingConsumerError { Message = ex.Message };
                    responseType = BlockingConsumerResponseTypes.Error;
                }

                if (properties.IsReplyToPresent())
                {
                    Respond(exchangeName, properties.ReplyTo, response, properties.CorrelationId, responseType);
                }
                else
                {
                    throw new ApplicationException("Blocking call requires a reply to in order to notify caller");
                }
            }
        }

        private void Respond(string originatingExchangeName, string replyTo, object response, string correlationId, string responseType)
        {
            using (LogContext.Create("Respond"))
            {

                try
                {
                    var routingKey = replyTo;

                    using (
                        var channel = _connection.OpenChannel(DefaultConfigValues.Model.RetryAttempts,
                            Convert.ToInt32(DefaultConfigValues.Model.RetryDelay.TotalMilliseconds), DefaultConfigValues.Model.RetryDelayGrowthFactor))
                    {
                        channel.ConfirmSelect();

                        var properties = channel.CreateBasicProperties();

                        properties.CorrelationId = correlationId;
                        properties.ResponseType = responseType;

                        //reply-to's do not use exchange names since there is a reply-to address
                        var replyToExchangeName = string.Empty;

                        channel.BasicPublish(replyToExchangeName, routingKey,
                            DefaultConfigValues.Model.Publish.NotMandatory,
                            DefaultConfigValues.Model.Publish.DoNotDeliverImmediatelyOrRequireAListener, properties,
                            _messageEncryptor.Encrypt(originatingExchangeName, _objectSerializer.ToBytes(response)));

                        channel.WaitForConfirmsOrDie(DefaultConfigValues.ConfirmationTimeout);
                    }

                }
                catch (Exception ex)
                {
                    _log.Error("Failed to respond to caller", ex);
                }
            }
        }
    }
}