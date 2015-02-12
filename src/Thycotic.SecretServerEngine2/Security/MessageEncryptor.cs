﻿using Thycotic.MessageQueueClient;

namespace Thycotic.SecretServerEngine2.Security
{
    /// <summary>
    /// Message encrytor which encrypts and decrypts based on exchange name
    /// </summary>
    public class MessageEncryptor : IMessageEncryptor
    {
        /// <summary>
        /// Encrypts the specified exchange name.
        /// </summary>
        /// <param name="exchangeName">Name of the exchange.</param>
        /// <param name="body">To bytes.</param>
        /// <returns></returns>
        public byte[] Encrypt(string exchangeName, byte[] body)
        {
            return body;
        }

        /// <summary>
        /// Decrypts the specified exchange name.
        /// </summary>
        /// <param name="exchangeName">Name of the exchange.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public byte[] Decrypt(string exchangeName, byte[] body)
        {
            return body;
        }

        //server
        //private object CallMethod(Client client, IRpcAgentServerCallBack callBack, string methodName, params object[] args)
        //{
        //    SymmetricEncryptor encryptor = new SymmetricEncryptor();
        //    byte[] methodNameBytes = Encoding.ASCII.GetBytes(methodName);
        //    ByteSaltProvider saltProvider = new ByteSaltProvider();
        //    byte[] saltedMethodName = saltProvider.Salt(methodNameBytes, RpcAgentServer.SALT_LENGTH);
        //    byte[] encryptedMethodName = encryptor.Encrypt(saltedMethodName, new SymmetricKey(client.SymmetricKey), new InitializationVector(client.InitalizationVector));
        //    SerializationService serializationService = new SerializationService();
        //    List<object> argumentList = args.ToList();
        //    argumentList.Insert(0, DateTime.UtcNow);
        //    args = argumentList.ToArray();
        //    byte[][] serializedObjects = new byte[args.Length][];
        //    for (int i = 0; i < args.Length; i++)
        //    {
        //        byte[] bytes = serializationService.Serialize(args[i], RpcAgentServer.SALT_LENGTH);
        //        byte[] saltBytes = new byte[RpcAgentServer.SALT_LENGTH];
        //        new AppCore.RandomProvider().NextBytes(saltBytes);
        //        saltBytes.CopyTo(bytes, 0);
        //        byte[] encryptedData = encryptor.Encrypt(bytes, new SymmetricKey(client.SymmetricKey), new InitializationVector(client.InitalizationVector));
        //        serializedObjects[i] = encryptedData;
        //    }
        //    byte[][] result = callBack.CallMethod(encryptedMethodName, serializedObjects);
        //    if (result == null)
        //    {
        //        return null;
        //    }
        //    object[] returnResults = new object[result.Length];
        //    for (int i = 0; i < result.Length; i++)
        //    {
        //        byte[] decryptedResult = encryptor.Decrypt(result[i], new SymmetricKey(client.SymmetricKey), new InitializationVector(client.InitalizationVector));
        //        byte[] unsaltedResult = saltProvider.Unsalt(decryptedResult, RpcAgentServer.SALT_LENGTH);
        //        returnResults[i] = serializationService.Deserialize(unsaltedResult);
        //    }
        //    DateTime timeStamp = (DateTime)returnResults[0];
        //    TimeSpan timeSpan = DateTime.UtcNow - timeStamp;
        //    if (Math.Abs(timeSpan.TotalHours) > 24)
        //    {
        //        throw new Exception("The Remote Agent's time is not within the acceptible range of the server's time.");
        //    }
        //    return returnResults[1];
        //}

        //client
        //public byte[][] CallMethod(byte[] encryptedMethodName, params byte[][] args)
        //{
        //    try
        //    {
        //        if (_enableAdvancedLogging)
        //        {
        //            EventLog.WriteEntry("SecretServerAgentService", "Method call received", EventLogEntryType.Information, EventLogIds.RemoteAgentMethodCall, EventLogCategory.RemoteAgent);
        //            EventLog.WriteEntry("SecretServerAgentService", "Symmetric key is : " + (_symmetricKey == null ? "NULL" : "NOT NULL"), EventLogEntryType.Information, EventLogIds.RemoteAgentSymetricKey, EventLogCategory.RemoteAgent);
        //        }
        //        SymmetricEncryptor encryptor = new SymmetricEncryptor();
        //        byte[] decryptedMethodName = encryptor.Decrypt(encryptedMethodName, _symmetricKey, _initializationVector);
        //        byte[] unsaltedMethodName = _saltProvider.Unsalt(decryptedMethodName, SALT_LENGTH);
        //        string methodName = Encoding.ASCII.GetString(unsaltedMethodName);
        //        Type clientType = typeof(RemoteAgentClient);
        //        MethodInfo[] methods = clientType.GetMethods(BindingFlags.Static | BindingFlags.Public);
        //        MethodInfo methodInfo = methods.Where(m => m.Name.ToLower() == methodName.ToLower()).First();
        //        object[] parameters = new object[args.Length];
        //        SerializationService serializationService = new SerializationService();
        //        for (int i = 0; i < args.Length; i++)
        //        {
        //            byte[] decryptedData = encryptor.Decrypt(args[i], _symmetricKey, _initializationVector);
        //            byte[] unsaltedData = _saltProvider.Unsalt(decryptedData, SALT_LENGTH);
        //            parameters[i] = serializationService.Deserialize(unsaltedData);
        //        }
        //        DateTime timeStamp = (DateTime)parameters[0];
        //        TimeSpan timeSpan = DateTime.UtcNow - timeStamp;
        //        if (_enableAdvancedLogging)
        //        {
        //            EventLog.WriteEntry("SecretServerAgentService", "Method name: " + methodName, EventLogEntryType.Information, EventLogIds.RemoteAgentMethodName, EventLogCategory.RemoteAgent);
        //        }
        //        if (Math.Abs(timeSpan.TotalHours) > 24)
        //        {
        //            ServiceLocator.Logger.Log(CoreLogLevel.Error, "Validation Exception: Timestamp was outside acceptable range.");
        //            EventLog.WriteEntry("SecretServerAgentService", "Validation Exception: Timestamp was outside acceptable range.", EventLogEntryType.Warning, EventLogIds.RemoteAgentValidationException, EventLogCategory.RemoteAgent);
        //            byte[] returnBadTime = serializationService.Serialize(DateTime.MinValue);
        //            returnBadTime = _saltProvider.Salt(returnBadTime, SALT_LENGTH);
        //            byte[] returnBytes = encryptor.Encrypt(returnBadTime, _symmetricKey, _initializationVector);
        //            return new byte[][] { returnBytes, returnBytes };
        //        }
        //        List<object> parameterList = parameters.ToList();
        //        parameterList.RemoveAt(0);
        //        parameters = parameterList.ToArray();
        //        object invokeResult = methodInfo.Invoke(null, parameters);
        //        if (invokeResult == null)
        //        {
        //            return null;
        //        }

        //        byte[] returnTime = serializationService.Serialize(DateTime.UtcNow);
        //        byte[] returnData = serializationService.Serialize(invokeResult);
        //        returnTime = _saltProvider.Salt(returnTime, SALT_LENGTH);
        //        returnData = _saltProvider.Salt(returnData, SALT_LENGTH);
        //        return new byte[][] { encryptor.Encrypt(returnTime, _symmetricKey, _initializationVector), encryptor.Encrypt(returnData, _symmetricKey, _initializationVector) };
        //    }
        //    catch (Exception exception)
        //    {
        //        ServiceLocator.Logger.Log(CoreLogLevel.Error, "Exception: " + exception);
        //        EventLog.WriteEntry("SecretServerAgentService", "Error in Call Method. Exception: " + exception, EventLogEntryType.Warning, EventLogIds.RemoteAgentMethodException, EventLogCategory.RemoteAgent);
        //        return null;
        //    }
        //}

        //private IRemoteAgent SaveAgentInDatabase(string publicKey, string name, string version)
        //{
        //    lock (lockObject)
        //    {
        //        IRemoteAgent agent = _remoteAgentProvider.GetAgentOrNew(publicKey, name);
        //        DateTime now = ServiceLocator.DateTime.Now;
        //        agent.LastConnected = now;
        //        agent.ConnectionStatus = RemoteAgentConnectionStatus.Online;
        //        agent.Version = version;
        //        agent.LastActivity = now;
        //        _remoteAgentProvider.Save(agent);
        //        return agent;
        //    }
        //}

        //private IRemoteAgent UpdateAgent(IRemoteAgent agent, int connectionStatus)
        //{
        //    lock (lockObject)
        //    {
        //        DateTime now = ServiceLocator.DateTime.Now;
        //        agent.ConnectionStatus = connectionStatus;
        //        agent.LastActivity = now;
        //        _remoteAgentProvider.Save(agent);
        //        return agent;
        //    }
        //}
    }
}