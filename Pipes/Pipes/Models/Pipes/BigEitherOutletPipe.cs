using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IBigEitherOutletPipe<TMessage, out TTieBreaker> : IPipe where TTieBreaker : ITieBreaker
    {
        ISimpleInlet<TMessage> Inlet { get; }
        IReadOnlyList<ISimpleOutlet<TMessage>> Outlets { get; }
        TTieBreaker TieBreaker { get; }
    }

    public class BigEitherOutletPipe<TMessage, TTieBreaker> : SimplePipe<TMessage>, IBigEitherOutletPipe<TMessage, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        public ISimpleInlet<TMessage> Inlet { get; }
        public IReadOnlyList<ISimpleOutlet<TMessage>> Outlets { get; }
        public TTieBreaker TieBreaker { get; }

        public BigEitherOutletPipe(ISimpleInlet<TMessage> inlet, IReadOnlyList<ISimpleOutlet<TMessage>> outlets, TTieBreaker tieBreaker)
            : base(new[] {inlet}, outlets)
        {
            Inlet = inlet;
            Outlets = outlets;

            TieBreaker = tieBreaker;
        }

        protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
        {
            var receivers = Outlets.Select((outlet, index) => new Tuple<int, Action<TMessage>>(index, outlet.FindReceiver())).Where(t => t.Item2 != null).ToList();
            if (!receivers.Any()) return null;

            var result = TieBreaker.ResolveTie(receivers.Select(s => s.Item1));

            return receivers.Single(s => s.Item1 == result).Item2;
        }

        protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
        {
            return Inlet.FindSender();
        }
    }
}
