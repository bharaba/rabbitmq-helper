using System.ServiceModel;

namespace Thycotic.MemoryMq.Subsystem
{
    /// <summary>
    /// Callback channel provider
    /// </summary>
    public class CallbackChannelProvider : ICallbackChannelProvider
    {
        /// <summary>
        /// Gets the callback channel using OperationContext.Current.
        /// </summary>
        /// <returns></returns>
        public IMemoryMqServerCallback GetCallbackChannel()
        {
            return OperationContext.Current.GetCallbackChannel<IMemoryMqServerCallback>();
        }
    }
}