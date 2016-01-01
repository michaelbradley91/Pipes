using System;
using System.Collections.Generic;
using Pipes.Builders;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface IValvedPipe<TReceive, TSend, out TTieBreaker> : IPipe where TTieBreaker : ITieBreaker
    {
        ISimpleInlet<TReceive> Inlet { get; }
        ISimpleOutlet<TSend> Outlet { get; }
        IValve<TReceive, TSend> Valve { get; }
        TTieBreaker TieBreaker { get; }
    }

    public class ValvedPipe<TReceive, TSend, TTieBreaker> : IValvedPipe<TReceive, TSend, TTieBreaker> where TTieBreaker : ITieBreaker
    {
        public IReadOnlyCollection<IInlet> AllInlets => new[] {Inlet};
        public IReadOnlyCollection<IOutlet> AllOutlets => new[] {Outlet};
        public SharedResource SharedResource { get; }

        public ISimpleInlet<TReceive> Inlet { get; }
        public ISimpleOutlet<TSend> Outlet { get; }
        public IValve<TReceive, TSend> Valve { get; }
        public TTieBreaker TieBreaker { get; }

        private readonly IAdapterInlet<TSend> adapterInlet;
        private readonly IAdapterOutlet<TReceive> adapterOutlet;

        public ValvedPipe(ISimpleInlet<TReceive> inlet, ISimpleOutlet<TSend> outlet, TTieBreaker tieBreaker)
        {
            var pipeResource = SharedResourceHelpers.CreateAndConnectSharedResources(inlet.SharedResource, outlet.SharedResource);
            SharedResource = pipeResource;
            adapterOutlet = new AdapterOutlet<TReceive>(new Lazy<IPipe>(() => this), SharedResourceHelpers.CreateAndConnectSharedResource(pipeResource));
            adapterInlet = new AdapterInlet<TSend>(new Lazy<IPipe>(() => this), SharedResourceHelpers.CreateAndConnectSharedResource(pipeResource));

            var preparationCapacityPipe = PipeBuilder.New.CapacityPipe<TSend>().WithCapacity(1).Build();
            var flushEitherOutletPipe = PipeBuilder.New.EitherOutletPipe<TSend>().Build();
            var splittingPipe = PipeBuilder.New.SplittingPipe<TSend>().Build();
            var sendTransformPipe = PipeBuilder.New.TransformPipe<TSend, ReceiveOrSendResult<TReceive, TSend>>().WithMap(m => ReceiveOrSendResult<TReceive, TSend>.CreateSendResult()).Build();
            var receiveTransformPipe = PipeBuilder.New.TransformPipe<TReceive, ReceiveOrSendResult<TReceive, TSend>>().WithMap(ReceiveOrSendResult<TReceive, TSend>.CreateReceiveResult).Build();
            var eitherInletPipe = PipeBuilder.New.EitherInletPipe<ReceiveOrSendResult<TReceive, TSend>>().WithTieBreaker(tieBreaker).Build();

            preparationCapacityPipe.Outlet.ConnectTo(flushEitherOutletPipe.Inlet);
            flushEitherOutletPipe.RightOutlet.ConnectTo(splittingPipe.Inlet);
            splittingPipe.RightOutlet.ConnectTo(sendTransformPipe.Inlet);
            sendTransformPipe.Outlet.ConnectTo(eitherInletPipe.LeftInlet);
            receiveTransformPipe.Outlet.ConnectTo(eitherInletPipe.RightInlet);

            adapterInlet.ConnectTo(splittingPipe.LeftOutlet);
            adapterOutlet.ConnectTo(receiveTransformPipe.Inlet);

            Valve = new Valve<TReceive, TSend>(preparationCapacityPipe.Inlet, flushEitherOutletPipe.LeftOutlet, eitherInletPipe.Outlet);
            Inlet = inlet;
            Outlet = outlet;
            TieBreaker = tieBreaker;
        }
        
        public Action<TMessage> FindReceiver<TMessage>(IInlet<TMessage> inletSendingMessage, bool checkInlet = true)
        {
            if (inletSendingMessage == Inlet)
            {
                var receiver = adapterOutlet.FindReceiver();
                if (receiver == null) return null;
                return m => receiver((TReceive) (object) m);
            }
            if (inletSendingMessage == adapterInlet)
            {
                var receiver = Outlet.FindReceiver();
                if (receiver == null) return null;
                return m => receiver((TSend) (object) m);
            }
            throw new InvalidOperationException("The outlet receiving the message is not associated to this pipe.");
        }

        public Func<TMessage> FindSender<TMessage>(IOutlet<TMessage> outletReceivingMessage, bool checkOutlet = true)
        {
            if (outletReceivingMessage == Outlet)
            {
                var sender = adapterInlet.FindSender();
                if (sender == null) return null;
                return () => (TMessage)(object)sender();
            }
            if (outletReceivingMessage == adapterOutlet)
            {
                var sender = Inlet.FindSender();
                if (sender == null) return null;
                return () => (TMessage)(object)sender();
            }
            throw new InvalidOperationException("The inlet sending the message is not associated to this pipe.");
        }
    }
}
