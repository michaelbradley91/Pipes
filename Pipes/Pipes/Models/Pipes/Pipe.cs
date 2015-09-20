using System;
using System.Collections.Generic;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IPipe<TMessage>
    {
        IReadOnlyCollection<Inlet<TMessage>> Inlets { get; }
        IReadOnlyCollection<Outlet<TMessage>> Outlets { get; }

        Action<TMessage> FindReceiver();
        Func<TMessage> FindSender();
    }
}
