using System.Collections.Generic;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    internal interface IPipe<TMessage>
    {
        IReadOnlyCollection<Inlet<TMessage>> Inlets { get; }
        IReadOnlyCollection<Outlet<TMessage>> Outlets { get; }

        void TryToSend(WaitingSender<TMessage> sender);
        void TryToReceive(WaitingReceiver<TMessage> receiver);
    }
}
