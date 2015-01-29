﻿using System;
using Thycotic.Logging;
using Thycotic.MessageQueueClient;
using Thycotic.Messages.Areas.POC.Request;
using Thycotic.Messages.Common;

namespace Thycotic.SecretServerAgent2.Logic.Areas.POC
{
    /// <summary>
    /// Simple Hello World consumer
    /// </summary>
    public class ChainMessageConsumer : IConsumer<ChainMessage>
    {
        private readonly IRequestBus _bus;
        private readonly ILogWriter _log = Log.Get(typeof(ChainMessage));

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainMessageConsumer"/> class.
        /// </summary>
        /// <param name="bus">The bus.</param>
        public ChainMessageConsumer(IRequestBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// Consumes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Consume(ChainMessage request)
        {
            _log.Info("CONSUMER: Received chain message");
            //throw new ApplicationException();

            Console.WriteLine("Posting next message...");

            _bus.Publish(new HelloWorldMessage { Content = "Hello from the chain consumer!"});

            Console.WriteLine("Posted");
        }
    }
}