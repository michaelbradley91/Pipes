using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface ITransformPipe<TSourceMessage, TTargetMessage> : IComplexPipe<TSourceMessage, TTargetMessage>
    {
        Func<TSourceMessage, TTargetMessage> Map { get; }
    }

    public class TransformPipe<TSourceMessage, TTargetMessage> : ComplexPipe<TSourceMessage, TTargetMessage>, ITransformPipe<TSourceMessage, TTargetMessage>
    {
        public Func<TSourceMessage, TTargetMessage> Map { get; }

        /// <summary>
        /// The function passed in may be run by pipes any number of times while resolving where a message
        /// is sent. It should also not rely on the acquisition of any other pipes.
        /// 
        /// Generally, this should be a simple stateless function - for the same input it should return the same output.
        /// Try other functions at your own risk
        /// </summary>
        public TransformPipe(ISimpleInlet<TSourceMessage> inlet, ISimpleOutlet<TTargetMessage> outlet, Func<TSourceMessage, TTargetMessage> map)
            : base(inlet, outlet)
        {
            Map = map;
        }

        protected override Action<TSourceMessage> FindReceiver(IInlet<TSourceMessage> inletSendingMessage)
        {
            var receiver = Outlet.FindReceiver();
            if (receiver == null) return null;
            return m => receiver(Map(m));
        }

        protected override Func<TTargetMessage> FindSender(IOutlet<TTargetMessage> outletReceivingMessage)
        {
            var sender = Inlet.FindSender();
            if (sender == null) return null;
            return () => Map(sender());
        }
    }
}
