using System;
using System.Collections.Generic;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoInletPipe<TMessage>
    {
        Inlet<TMessage> LeftInlet { get; }
        Inlet<TMessage> RightInlet { get; }
        Outlet<TMessage> Outlet { get; }
    }

    public class TwoInletPipe<TMessage> : ITwoInletPipe<TMessage>, IPipe<TMessage>
    {
        public Inlet<TMessage> LeftInlet { get; private set; }
        public Inlet<TMessage> RightInlet { get; private set; }
        public Outlet<TMessage> Outlet { get; private set; }

        private readonly ITieBreaker tieBreaker;

        private TwoInletPipe(double leftProbability)
            : this(new RandomisingTieBreaker(leftProbability))
        {
        }

        private TwoInletPipe(Priority priority)
            : this(new PrioritisingTieBreaker(priority))
        {
        }

        private TwoInletPipe(Alternated alternated) 
            : this(new AlternatingTieBreaker(alternated))
        {
        }

        private TwoInletPipe(ITieBreaker tieBreaker)
        {
            this.tieBreaker = tieBreaker;

            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var leftInletResource = resourceGroup.CreateAndAcquireSharedResource();
            var rightInletResource = resourceGroup.CreateAndAcquireSharedResource();
            var outletResource = resourceGroup.CreateAndAcquireSharedResource();
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(leftInletResource, pipeResource);
            resourceGroup.ConnectSharedResources(rightInletResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, outletResource);

            LeftInlet = new Inlet<TMessage>(this, leftInletResource);
            RightInlet = new Inlet<TMessage>(this, rightInletResource);
            Outlet = new Outlet<TMessage>(this, outletResource);
            
            resourceGroup.FreeSharedResources();
        }

        internal static TwoInletPipe<TMessage> CreateRandomised(double leftProbability)
        {
            return new TwoInletPipe<TMessage>(leftProbability);
        }

        internal static TwoInletPipe<TMessage> CreatePrioritised(Priority priority)
        {
            return new TwoInletPipe<TMessage>(priority);
        }

        internal static TwoInletPipe<TMessage> CreateAlternated(Alternated alternated)
        {
            return new TwoInletPipe<TMessage>(alternated);
        }

        internal static TwoInletPipe<TMessage> Create(ITieBreaker tieBreaker)
        {
            return new TwoInletPipe<TMessage>(tieBreaker);
        }

        IReadOnlyCollection<Inlet<TMessage>> IPipe<TMessage>.Inlets
        {
            get { return new[] {LeftInlet, RightInlet}; }
        }

        IReadOnlyCollection<Outlet<TMessage>> IPipe<TMessage>.Outlets
        {
            get { return new[] {Outlet}; }
        }

        Action<TMessage> IPipe<TMessage>.FindReceiver()
        {
            if (Outlet.ConnectedInlet == null)
            {
                if (Outlet.HasWaitingReceiver())
                {
                    // TODO: need to pull other messages down
                    return message => Outlet.UseWaitingReceiver(message);
                }
                return null;
            }
            var nextPipe = Outlet.ConnectedInlet.Pipe;
            return nextPipe.FindReceiver();
        }

        Func<TMessage> IPipe<TMessage>.FindSender()
        {
            var leftSender = FindSenderFromInlet(LeftInlet);
            var rightSender = FindSenderFromInlet(RightInlet);

            if (leftSender == null) return rightSender;
            if (rightSender == null) return leftSender;

            var tieResult = tieBreaker.ResolveTie();
            switch (tieResult)
            {
                case TieResult.Left:
                    return leftSender;
                case TieResult.Right:
                    return rightSender;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Func<TMessage> FindSenderFromInlet(Inlet<TMessage> inlet)
        {
            if (inlet.ConnectedOutlet == null)
            {
                if (inlet.HasWaitingSender())
                {
                    // TODO: need to pull other messages down
                    return () => inlet.UseWaitingSender();
                }
                return null;
            }
            var previousPipe = inlet.ConnectedOutlet.Pipe;
            return previousPipe.FindSender();
        }
    }
}