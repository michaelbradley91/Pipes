using Pipes.Builders;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IValvedPipe<TReceive, TSend, out TTieBreaker> : IPipe where TTieBreaker : ITwoWayTieBreaker
    {
        ISimpleInlet<TReceive> Inlet { get; }
        ISimpleOutlet<TSend> Outlet { get; }
        IValve<TReceive, TSend> Valve { get; }

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        TTieBreaker TieBreaker { get; }
    }

    public class ValvedPipe<TReceive, TSend, TTieBreaker> : CompositePipe, IValvedPipe<TReceive, TSend, TTieBreaker> where TTieBreaker : ITwoWayTieBreaker
    {
        public ISimpleInlet<TReceive> Inlet { get; }
        public ISimpleOutlet<TSend> Outlet { get; }
        public IValve<TReceive, TSend> Valve { get; }
        public TTieBreaker TieBreaker { get; }

        public ValvedPipe(ISimpleInlet<TReceive> inlet, ISimpleOutlet<TSend> outlet, TTieBreaker tieBreaker) : base(new[] {inlet}, new[] {outlet})
        {
            Inlet = inlet;
            Outlet = outlet;
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
