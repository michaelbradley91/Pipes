using System;
using System.Collections.Generic;
using Pipes.Models.Lets;

namespace Pipes.Models.Pipes
{
    public abstract class CompositePipe : Pipe
    {
        protected CompositePipe(IReadOnlyCollection<IInlet> connectableInlets, IReadOnlyCollection<IOutlet> connectableOutlets) : base(connectableInlets, connectableOutlets)
        {
        }

        /// <summary>
        /// A convenience method for creating an adapter inlet. This can be used by composite pipes - pipes whose internal implementation uses existing pipes.
        /// </summary>
        protected IAdapterInlet<TMessage> CreateAdapterInlet<TMessage>()
        {
            return new AdapterInlet<TMessage>(new Lazy<IPipe>(() => this), SharedResource.CreateAndConnect());
        }

        /// <summary>
        /// A convenience method for creating an adapter outlet. This can be used by composite pipes - pipes whose internal implementation uses existing pipes.
        /// </summary>
        protected IAdapterOutlet<TMessage> CreateAdapterOutlet<TMessage>()
        {
            return new AdapterOutlet<TMessage>(new Lazy<IPipe>(() => this), SharedResource.CreateAndConnect());
        }
    }
}
