using System;
using System.Diagnostics.Contracts;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Thycotic.MessageQueue.Client.QueueClient.AzureServiceBus
{
    /// <summary>
    /// Azure service bus connection
    /// </summary>
    public class AzureServiceBusConnection : IAzureServiceBusConnection
    {
        /// <summary>
        /// Subscription names
        /// </summary>
        public static class SubscriptionNames
        {
            /// <summary>
            /// The routing key
            /// </summary>
            public const string RoutingKey = "RoutingKey";
        }

        private readonly string _connectionString;
        private readonly string _sharedAccessKeyName;
        private readonly string _sharedAccessKeyValue;

        /// <summary>
        /// Server version
        /// </summary>
        public string ServerVersion {
            get
            {   
                //TODO: flesh out the version
                return "";
            }
        }

        /// <summary>
        /// Gets or sets the connection created.
        /// </summary>
        /// <value>
        /// The connection created.
        /// </value>
        public EventHandler ConnectionCreated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusConnection" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sharedAccessKeyName">Name of the user.</param>
        /// <param name="sharedAccessKeyValue">The sharedAccessKeyValue.</param>
        public AzureServiceBusConnection(string connectionString, string sharedAccessKeyName, string sharedAccessKeyValue)
        {
            Contract.Requires<ArgumentNullException>(connectionString != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(connectionString));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sharedAccessKeyName));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sharedAccessKeyValue));

            _connectionString = connectionString;
            _sharedAccessKeyName = sharedAccessKeyName;
            _sharedAccessKeyValue = sharedAccessKeyValue;
        }

        /// <summary>
        /// Creates the manager.
        /// </summary>
        /// <returns></returns>
        public IAzureServiceBusManager CreateManager()
        {
            return new AzureServiceBusManager(_connectionString, _sharedAccessKeyName, _sharedAccessKeyValue);
        }

        private MessagingFactory GetFactory()
        {
            var uri = new Uri(_connectionString);
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_sharedAccessKeyName, _sharedAccessKeyValue);
            return MessagingFactory.Create(uri, tokenProvider);
        }

        /// <summary>
        /// Creates the topic client.
        /// </summary>
        /// <param name="topicPath">The topic path.</param>
        /// <returns></returns>
        public TopicClient CreateTopicClient(string topicPath)
        {
            var messagingFactory = GetFactory();

            return messagingFactory.CreateTopicClient(topicPath);
        }

        /// <summary>
        /// Creates the message sender.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <returns></returns>
        public MessageSender CreateSender(string entityName)
        {
            var messagingFactory = GetFactory();

            return messagingFactory.CreateMessageSender(entityName);
        }
        
        /// <summary>
        /// Creates the queue client.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <returns></returns>
        public MessageReceiver CreateReceiver(string entityName)
        {
            var messagingFactory = GetFactory();

            return messagingFactory.CreateMessageReceiver(entityName, ReceiveMode.PeekLock);
        }
       
       

        /// <summary>
        /// Opens the model/channel.
        /// </summary>
        /// <param name="retryAttempts">The retry attempts.</param>
        /// <param name="retryDelayMs">The retry delay ms.</param>
        /// <param name="retryDelayGrowthFactor">The retry delay growth factor.</param>
        /// <returns></returns>
        public ICommonModel OpenChannel(int retryAttempts, int retryDelayMs, float retryDelayGrowthFactor)
        {
            return new AzureServiceBusModel(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //nothing to dispose
        }

        
    }
}