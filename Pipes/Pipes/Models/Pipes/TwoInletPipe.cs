using System;
using System.Collections.Generic;
using Pipes.Models.Lets;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoInletPipe<TMessage> : IPipe
    {
        ISimpleInlet<TMessage> LeftInlet { get; }
        ISimpleInlet<TMessage> RightInlet { get; }
        ISimpleOutlet<TMessage> Outlet { get; }
    }

    public abstract class TwoInletPipe<TMessage> : SimplePipe<TMessage>, ITwoInletPipe<TMessage>
    {
        public ISimpleInlet<TMessage> LeftInlet { get; }
        public ISimpleInlet<TMessage> RightInlet { get; }
        public ISimpleOutlet<TMessage> Outlet { get; }

        protected TwoInletPipe(ISimpleInlet<TMessage> leftInlet, ISimpleInlet<TMessage> rightInlet, ISimpleOutlet<TMessage> outlet)
            : base(new[] {leftInlet, rightInlet}, new[] {outlet})
        {
            LeftInlet = leftInlet;
            RightInlet = rightInlet;
            Outlet = outlet;
        }
    }
}