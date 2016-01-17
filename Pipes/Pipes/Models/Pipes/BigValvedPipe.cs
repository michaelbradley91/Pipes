using System;
using System.Collections.Generic;
using System.Linq;
using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IBigValvedPipe<TReceive, TSend, out TTieBreaker> : IPipe where TTieBreaker : ITieBreaker
    {
        IReadOnlyList<ISimpleInlet<TReceive>> Inlets { get; }
        IReadOnlyList<ISimpleOutlet<TSend>> Outlets { get; }
        IValve<TReceive, TSend> Valve { get; }

        /// <summary>
        /// Competitors in the tie breaker are resolved as Inlets and then Outlets. So the second outlet would be given competitor
        /// id of Inlets.Count + 1. (Ids start from 0)
        /// </summary>
        TTieBreaker TieBreaker { get; }
    }

    public class BigValvedPipe<TReceive, TSend, TTieBreaker> : CompositePipe, IBigValvedPipe<TReceive, TSend, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        public IReadOnlyList<ISimpleInlet<TReceive>> Inlets { get; }
        public IReadOnlyList<ISimpleOutlet<TSend>> Outlets { get; }
        public IValve<TReceive, TSend> Valve { get; }
        public TTieBreaker TieBreaker { get; }

        public BigValvedPipe(IReadOnlyList<ISimpleInlet<TReceive>> inlets, IReadOnlyList<ISimpleOutlet<TSend>> outlets, TTieBreaker tieBreaker)
            : base(inlets, outlets)
        {
            Inlets = inlets;
            Outlets = outlets;
            TieBreaker = tieBreaker;
            
            // Construct all necessary pipes
            var preparationCapacityPipe = PipeBuilder.New.CapacityPipe<TSend>().WithCapacity(1).Build();
            var flushEitherOutletPipe = PipeBuilder.New.BigEitherOutletPipe<TSend>().WithSize(Outlets.Count + 1).Build();

            var splittingPipes = Enumerable.Repeat<Func<ISplittingPipe<TSend>>>(() =>
                PipeBuilder.New.SplittingPipe<TSend>().Build(), Outlets.Count)
                .Select(f => f()).ToList();

            var sendTransformPipes = Enumerable.Repeat<Func<ITransformPipe<TSend, ReceiveOrSendResult<TReceive, TSend>>>>(() =>
                PipeBuilder.New.TransformPipe<TSend, ReceiveOrSendResult<TReceive, TSend>>().WithMap(m => ReceiveOrSendResult<TReceive, TSend>.CreateSendResult()).Build(), Outlets.Count)
                .Select(f => f()).ToList();

            var receiveTransformPipes = Enumerable.Repeat<Func<ITransformPipe<TReceive, ReceiveOrSendResult<TReceive, TSend>>>>(() =>
                PipeBuilder.New.TransformPipe<TReceive, ReceiveOrSendResult<TReceive, TSend>>().WithMap(ReceiveOrSendResult<TReceive, TSend>.CreateReceiveResult).Build(), Inlets.Count)
                .Select(f => f()).ToList();

            var resultPipe = PipeBuilder.New.BigEitherInletPipe<ReceiveOrSendResult<TReceive, TSend>>().WithSize(inlets.Count + outlets.Count).WithTieBreaker(TieBreaker).Build();

            // Form the pipe system. Note that this DOES form a non-tree, but its correctness is guaranteed
            // by the fact the individual receivers of the splitting pipes are distinct.
            preparationCapacityPipe.Outlet.ConnectTo(flushEitherOutletPipe.Inlet, false);

            for (var i = 0; i < Outlets.Count; i++)
            {
                flushEitherOutletPipe.Outlets[i].ConnectTo(splittingPipes[i].Inlet, false);
                splittingPipes[i].RightOutlet.ConnectTo(sendTransformPipes[i].Inlet, false);
                sendTransformPipes[i].Outlet.ConnectTo(resultPipe.Inlets[Inlets.Count + i], false);

                CreateAndConnectAdapter(splittingPipes[i].LeftOutlet, Outlets[i]);
            }

            for (var i = 0; i < Inlets.Count; i++)
            {
                receiveTransformPipes[i].Outlet.ConnectTo(resultPipe.Inlets[i], false);

                CreateAndConnectAdapter(receiveTransformPipes[i].Inlet, Inlets[i]);
            }

            Valve = new Valve<TReceive, TSend>(preparationCapacityPipe.Inlet, flushEitherOutletPipe.Outlets[outlets.Count], resultPipe.Outlet);
        }
    }
}
