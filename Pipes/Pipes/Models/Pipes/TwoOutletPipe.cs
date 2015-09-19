﻿using System;
using System.Collections.Generic;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoOutletPipe<TMessage>
    {
        Inlet<TMessage> Inlet { get; }
        Outlet<TMessage> LeftOutlet { get; }
        Outlet<TMessage> RightOutlet { get; }
    }

    public class TwoOutletPipe<TMessage> : ITwoOutletPipe<TMessage>, IPipe<TMessage>
    {
        public Inlet<TMessage> Inlet { get; private set; }
        public Outlet<TMessage> LeftOutlet { get; private set; }
        public Outlet<TMessage> RightOutlet { get; private set; }

        private readonly ITieBreaker tieBreaker;

        private TwoOutletPipe(double leftProbability)
            : this(new RandomisingTieBreaker(leftProbability))
        {
        }

        private TwoOutletPipe(Priority priority)
            : this(new PrioritisingTieBreaker(priority))
        {
        }

        private TwoOutletPipe(Alternated alternated) 
            : this(new AlternatingTieBreaker(alternated))
        {
        }

        private TwoOutletPipe(ITieBreaker tieBreaker)
        {
            this.tieBreaker = tieBreaker;

            var resourceGroup = SharedResourceGroup.CreateWithNoAcquiredSharedResources();
            var inletResource = resourceGroup.CreateAndAcquireSharedResource();
            var leftOutletResource = resourceGroup.CreateAndAcquireSharedResource();
            var rightOutletResource = resourceGroup.CreateAndAcquireSharedResource();
            var pipeResource = resourceGroup.CreateAndAcquireSharedResource();

            pipeResource.AssociatedObject = this;

            resourceGroup.ConnectSharedResources(inletResource, pipeResource);
            resourceGroup.ConnectSharedResources(pipeResource, leftOutletResource);
            resourceGroup.ConnectSharedResources(pipeResource, rightOutletResource);

            Inlet = new Inlet<TMessage>(this, inletResource);
            LeftOutlet = new Outlet<TMessage>(this, leftOutletResource);
            RightOutlet = new Outlet<TMessage>(this, rightOutletResource);

            resourceGroup.FreeSharedResources();
        }

        internal static TwoOutletPipe<TMessage> CreateRandomised(double leftProbability)
        {
            return new TwoOutletPipe<TMessage>(leftProbability);
        }

        internal static TwoOutletPipe<TMessage> CreatePrioritised(Priority priority)
        {
            return new TwoOutletPipe<TMessage>(priority);
        }

        internal static TwoOutletPipe<TMessage> CreateAlternated(Alternated alternated)
        {
            return new TwoOutletPipe<TMessage>(alternated);
        }

        internal static TwoOutletPipe<TMessage> CreateDuplicator()
        {
            return new TwoOutletPipe<TMessage>(null);
        }

        internal static TwoOutletPipe<TMessage> Create(ITieBreaker tieBreaker)
        {
            return new TwoOutletPipe<TMessage>(tieBreaker);
        }

        IReadOnlyCollection<Inlet<TMessage>> IPipe<TMessage>.Inlets
        {
            get { return new[] {Inlet}; }
        }

        IReadOnlyCollection<Outlet<TMessage>> IPipe<TMessage>.Outlets
        {
            get { return new[] {LeftOutlet, RightOutlet}; }
        }

        Action<TMessage> IPipe<TMessage>.FindReceiver()
        {
            var leftReceiver = FindReceiverFromOutlet(LeftOutlet);
            var rightReceiver = FindReceiverFromOutlet(RightOutlet);

            if (leftReceiver == null) return rightReceiver;
            if (rightReceiver == null) return leftReceiver;

            var tieResult = tieBreaker.ResolveTie();
            switch (tieResult)
            {
                case TieResult.Left:
                    return leftReceiver;
                case TieResult.Right:
                    return rightReceiver;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Func<TMessage> IPipe<TMessage>.FindSender()
        {
            if (Inlet.ConnectedOutlet == null)
            {
                if (Inlet.HasWaitingSender())
                {
                    // TODO: need to pull other messages down
                    return () => Inlet.UseWaitingSender();
                }
                return null;
            }
            var previousPipe = Inlet.ConnectedOutlet.Pipe;
            return previousPipe.FindSender();
        }

        private static Action<TMessage> FindReceiverFromOutlet(Outlet<TMessage> outlet)
        {
            if (outlet.ConnectedInlet == null)
            {
                if (outlet.HasWaitingReceiver())
                {
                    // TODO: need to pull other messages down
                    return message => outlet.UseWaitingReceiver(message);
                }
                return null;
            }
            var nextPipe = outlet.ConnectedInlet.Pipe;
            return nextPipe.FindReceiver();
        }
    }
}