using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IBigEitherInletPipe<TMessage, out TTieBreaker> : IPipe where TTieBreaker : ITieBreaker
    {
        IReadOnlyList<ISimpleInlet<TMessage>> Inlets { get; }
        ISimpleOutlet<TMessage> Outlet { get; }
        TTieBreaker TieBreaker { get; }
    }

    public class BigEitherInletPipe<TMessage, TTieBreaker> : SimplePipe<TMessage>, IBigEitherInletPipe<TMessage, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        public IReadOnlyList<ISimpleInlet<TMessage>> Inlets { get; }
        public ISimpleOutlet<TMessage> Outlet { get; }
        public TTieBreaker TieBreaker { get; }

        public BigEitherInletPipe(IReadOnlyList<ISimpleInlet<TMessage>> inlets, ISimpleOutlet<TMessage> outlet, TTieBreaker tieBreaker)
            : base(inlets, new[] { outlet })
        {
            Inlets = inlets;
            Outlet = outlet;

            TieBreaker = tieBreaker;
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            return Outlet.FindReceiver();
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            var senders = Inlets.Select((inlet, index) => new Tuple<int, Func<TMessage>>(index, inlet.FindSender())).Where(t => t.Item2 != null).ToList();
            if (!senders.Any()) return null;

            var result = TieBreaker.ResolveTie(senders.Select(s => s.Item1));

            return senders.Single(s => s.Item1 == result).Item2;
        }
    }
}
