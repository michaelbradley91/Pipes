using System;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IBasicPipe<TMessage> : IStraightPipe<TMessage>
    {
    }

    public class BasicPipe<TMessage> : StraightPipe<TMessage>, IBasicPipe<TMessage>
    {
        public BasicPipe(IInlet<TMessage> inlet, IOutlet<TMessage> outlet)
            : base(inlet, outlet)
        {
        }

        public override Action<TMessage> FindReceiver()
        {
            return Outlet.FindReceiver();
        }

        public override Func<TMessage> FindSender()
        {
            return Inlet.FindSender();
        }
    }
}