using System.Collections.Generic;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoOutletPipe<TMessageType>
    {
        Inlet<TMessageType> Inlet { get; }
        Outlet<TMessageType> LeftOutlet { get; }
        Outlet<TMessageType> RightOutlet { get; }
    }

    public class TwoOutletPipe<TMessageType> : ITwoOutletPipe<TMessageType>, IPipe<TMessageType>
    {
        public Inlet<TMessageType> Inlet { get; private set; }
        public Outlet<TMessageType> LeftOutlet { get; private set; }
        public Outlet<TMessageType> RightOutlet { get; private set; }

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

            Inlet = new Inlet<TMessageType>(this, inletResource);
            LeftOutlet = new Outlet<TMessageType>(this, leftOutletResource);
            RightOutlet = new Outlet<TMessageType>(this, rightOutletResource);

            resourceGroup.FreeSharedResources();
        }

        internal static TwoOutletPipe<TMessageType> CreateRandomised(double leftProbability)
        {
            return new TwoOutletPipe<TMessageType>(leftProbability);
        }

        internal static TwoOutletPipe<TMessageType> CreatePrioritised(Priority priority)
        {
            return new TwoOutletPipe<TMessageType>(priority);
        }

        internal static TwoOutletPipe<TMessageType> CreateAlternated(Alternated alternated)
        {
            return new TwoOutletPipe<TMessageType>(alternated);
        }

        internal static TwoOutletPipe<TMessageType> CreateDuplicator()
        {
            return new TwoOutletPipe<TMessageType>(null);
        }

        internal static TwoOutletPipe<TMessageType> Create(ITieBreaker tieBreaker)
        {
            return new TwoOutletPipe<TMessageType>(tieBreaker);
        }

        IReadOnlyCollection<Inlet<TMessageType>> IPipe<TMessageType>.Inlets
        {
            get { return new[] {Inlet}; }
        }

        IReadOnlyCollection<Outlet<TMessageType>> IPipe<TMessageType>.Outlets
        {
            get { return new[] {LeftOutlet, RightOutlet}; }
        }
    }
}