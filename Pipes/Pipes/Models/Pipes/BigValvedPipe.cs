using System.Collections.Generic;
using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IBigValvedPipe<TReceive, TSend, out TTieBreaker> : IPipe where TTieBreaker : ITieBreaker
    {
        IReadOnlyCollection<ISimpleInlet<TReceive>> Inlets { get; }
        IReadOnlyCollection<ISimpleOutlet<TSend>> Outlets { get; }
        IValve<TReceive, TSend> Valve { get; }

        /// <summary>
        /// Competitors in the tie breaker are resolved as Inlets and then Outlets. So the second outlet would be given competitor
        /// id of Inlets.Count + 1. (Ids start from 0)
        /// </summary>
        TTieBreaker TieBreaker { get; }
    }

    public class BigValvedPipe<TReceive, TSend, TTieBreaker> : CompositePipe, IBigValvedPipe<TReceive, TSend, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        public IReadOnlyCollection<ISimpleInlet<TReceive>> Inlets { get; }
        public IReadOnlyCollection<ISimpleOutlet<TSend>> Outlets { get; }
        public IValve<TReceive, TSend> Valve { get; }
        public TTieBreaker TieBreaker { get; }

        public BigValvedPipe(IReadOnlyCollection<ISimpleInlet<TReceive>> inlets, IReadOnlyCollection<ISimpleOutlet<TSend>> outlets, TTieBreaker tieBreaker)
            : base(inlets, outlets)
        {
            Inlets = inlets;
            Outlets = outlets;
            TieBreaker = tieBreaker;
            
            var preparationCapacityPipe = PipeBuilder.New.CapacityPipe<TSend>().WithCapacity(1).Build();
            var flushEitherOutletPipe = PipeBuilder.New.EitherOutletPipe<TSend>().Build();
            var splittingPipe = PipeBuilder.New.SplittingPipe<TSend>().Build();
            var sendTransformPipe = PipeBuilder.New.TransformPipe<TSend, ReceiveOrSendResult<TReceive, TSend>>().WithMap(m => ReceiveOrSendResult<TReceive, TSend>.CreateSendResult()).Build();
            var receiveTransformPipe = PipeBuilder.New.TransformPipe<TReceive, ReceiveOrSendResult<TReceive, TSend>>().WithMap(ReceiveOrSendResult<TReceive, TSend>.CreateReceiveResult).Build();
            var eitherInletPipe = PipeBuilder.New.EitherInletPipe<ReceiveOrSendResult<TReceive, TSend>>().WithTieBreaker(TieBreaker).Build();

            preparationCapacityPipe.Outlet.ConnectTo(flushEitherOutletPipe.Inlet);
            flushEitherOutletPipe.RightOutlet.ConnectTo(splittingPipe.Inlet);
            splittingPipe.RightOutlet.ConnectTo(sendTransformPipe.Inlet);
            sendTransformPipe.Outlet.ConnectTo(eitherInletPipe.LeftInlet);
            receiveTransformPipe.Outlet.ConnectTo(eitherInletPipe.RightInlet);

            CreateAndConnectAdapter(splittingPipe.LeftOutlet, Outlet);
            CreateAndConnectAdapter(receiveTransformPipe.Inlet, Inlet);

            Valve = new Valve<TReceive, TSend>(preparationCapacityPipe.Inlet, flushEitherOutletPipe.LeftOutlet, eitherInletPipe.Outlet);
        }
    }
}
