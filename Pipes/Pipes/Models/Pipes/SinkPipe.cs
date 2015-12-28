using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface ISinkPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> Inlet { get; }
    }

    public class SinkPipe<TMessage> : SimplePipe<TMessage>, ISinkPipe<TMessage>
    {
        public ISimpleInlet<TMessage> Inlet { get; }

        protected SinkPipe(ISimpleInlet<TMessage> inlet) : base(new[] {inlet}, new IOutlet[] {})
        {
            Inlet = inlet;
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            return m => { };
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            return Inlet.FindSender();
        }
    }
}
