﻿using System;
using System.Collections.Generic;
using System.Configuration;
using Thycotic.AppCore;
using Thycotic.AppCore.Cryptography;
using Thycotic.DistributedEngine.Web.Common;
using Thycotic.DistributedEngine.Web.Common.Request;
using Thycotic.DistributedEngine.Web.Common.Response;
using Thycotic.ihawu.Business;
using Thycotic.Logging;
using Thycotic.DistributedEngine.Security;
using Thycotic.DistributedEngine.Logic;
using Thycotic.Utility;
using Thycotic.Utility.Security;
using Thycotic.Utility.Serialization;

namespace Thycotic.DistributedEngine.Configuration
{
    /// <summary>
    /// Remote configuration provider
    /// </summary>
    public class RemoteConfigurationProvider : IRemoteConfigurationProvider
    {
        private readonly IEngineIdentificationProvider _engineIdentificationProvider;
        private readonly ILocalKeyProvider _localKeyProvider;
        private readonly IRestCommunicationProvider _restCommunicationProvider;
        private readonly IObjectSerializer _objectSerializer;

        private readonly ILogWriter _log = Log.Get(typeof(MessageEncryptionKeyProvider));

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteConfigurationProvider" /> class.
        /// </summary>
        /// <param name="engineIdentificationProvider">The engine identification provider.</param>
        /// <param name="localKeyProvider">The local key provider.</param>
        /// <param name="restCommunicationProvider">The remote communication provider.</param>
        /// <param name="objectSerializer">The message serializer.</param>
        public RemoteConfigurationProvider(IEngineIdentificationProvider engineIdentificationProvider, ILocalKeyProvider localKeyProvider, IRestCommunicationProvider restCommunicationProvider, IObjectSerializer objectSerializer)
        {
            _engineIdentificationProvider = engineIdentificationProvider;
            _localKeyProvider = localKeyProvider;
            _restCommunicationProvider = restCommunicationProvider;
            _objectSerializer = objectSerializer;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetConfiguration()
        {
            try
            {
                PublicKey publicKey;
                PrivateKey privateKey;
                _localKeyProvider.GetKeys(out publicKey, out privateKey);

                var response =
                    _restCommunicationProvider.Post<EngineConfigurationResponse>(
                        _restCommunicationProvider.GetEndpointUri(EndPoints.EngineWebService.Prefix,
                            EndPoints.EngineWebService.Actions.GetConfiguration),
                        new EngineConfigurationRequest
                        {
                            IdentityGuid = _engineIdentificationProvider.IdentityGuid,
                            FriendlyName = _engineIdentificationProvider.FriendlyName,
                            PublicKey = Convert.ToBase64String(publicKey.Value),
                            Version = ReleaseInformationHelper.GetVersionAsDouble()
                        });

                if (!response.Success)
                {
                    throw new ConfigurationErrorsException(response.ErrorMessage);
                }

                var saltProvider = new ByteSaltProvider();

                var asymmetricEncryptor = new AsymmetricEncryptor();
                var decryptedConfiguration = asymmetricEncryptor.DecryptWithKey(privateKey, response.Configuration);
                var unsaltedConfiguration = saltProvider.Unsalt(decryptedConfiguration, MessageEncryption.SaltLength);

                return _objectSerializer.ToObject<Dictionary<string, string>>(unsaltedConfiguration);
            }

            catch (Exception ex)
            {
                _log.Error("Failed to retrieve exchange key", ex);
                throw;
            }
        }
    }
}