using System;
using System.Collections.Generic;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public interface IPipe<TMessage>
    {
        IReadOnlyCollection<IInlet<TMessage>> Inlets { get; }
        IReadOnlyCollection<IOutlet<TMessage>> Outlets { get; }

        Action<TMessage> FindReceiver();
        Func<TMessage> FindSender();
    }
}
