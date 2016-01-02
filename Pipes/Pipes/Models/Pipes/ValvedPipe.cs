using System;
using System.Collections.Generic;
using Pipes.Builders;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;

namespace Pipes.Models.Pipes
{
    public interface IValvedPipe<TReceive, TSend, out TTieBreaker> : IPipe where TTieBreaker : ITieBreaker
    {
        ISimpleInlet<TReceive> Inlet { get; }
        ISimpleOutlet<TSend> Outlet { get; }
        IValve<TReceive, TSend> Valve { get; }

        /// <summary>
        /// If "left" is the resolution of a tie, a message will be sent through the valve. If "right" is the resolution of a tie, a message will be received through the valve.
        /// </summary>
        TTieBreaker TieBreaker { get; }
    }

    public class ValvedPipe<TReceive, TSend, TTieBreaker> : Pipe, IValvedPipe<TReceive, TSend, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        protected override IReadOnlyCollection<IInlet> PipeInlets => new IInlet[] {Inlet, adapterInlet};
        protected override IReadOnlyCollection<IOutlet> PipeOutlets => new IOutlet[] {Outlet, adapterOutlet};

        public ISimpleInlet<TReceive> Inlet { get; }
        public ISimpleOutlet<TSend> Outlet { get; }
        public IValve<TReceive, TSend> Valve { get; }
        public TTieBreaker TieBreaker { get; }

        private readonly IAdapterInlet<TSend> adapterInlet;
        private readonly IAdapterOutlet<TReceive> adapterOutlet;

        public ValvedPipe(ISimpleInlet<TReceive> inlet, ISimpleOutlet<TSend> outlet, TTieBreaker tieBreaker) : base(new[] {inlet}, new[] {outlet})
        {
            Inlet = inlet;
            Outlet = outlet;
            TieBreaker = tieBreaker;

            adapterOutlet = new AdapterOutlet<TReceive>(new Lazy<IPipe>(() => this), SharedResourceHelpers.CreateAndConnectSharedResource(SharedResource));
            adapterInlet = new AdapterInlet<TSend>(new Lazy<IPipe>(() => this), SharedResourceHelpers.CreateAndConnectSharedResource(SharedResource));

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

            // We do not allow the connect methods to check for a non-tree, as this pipe has not been fully constructed yet.
            // As this is our internal pipe system, we can guarantee it forms a tree.
            adapterInlet.ConnectTo(splittingPipe.LeftOutlet, false);
            adapterOutlet.ConnectTo(receiveTransformPipe.Inlet, false);

            Valve = new Valve<TReceive, TSend>(preparationCapacityPipe.Inlet, flushEitherOutletPipe.LeftOutlet, eitherInletPipe.Outlet);
        }

        protected override Action<TMessage> FindReceiverFor<TMessage>(IInlet<TMessage> inletSendingMessage)
        {
            return inletSendingMessage == Inlet ? adapterOutlet.FindReceiver<TMessage>() : Outlet.FindReceiver<TMessage>();
        }

        protected override Func<TMessage> FindSenderFor<TMessage>(IOutlet<TMessage> outletReceivingMessage)
        {
            return outletReceivingMessage == Outlet ? adapterInlet.FindSender<TMessage>() : Inlet.FindSender<TMessage>();
        }
    }
}
