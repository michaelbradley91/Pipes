using System.Collections.Generic;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    internal interface IPipe<TMessageType>
    {
        IReadOnlyCollection<Inlet<TMessageType>> Inlets { get; }
        IReadOnlyCollection<Outlet<TMessageType>> Outlets { get; }
    }
}
