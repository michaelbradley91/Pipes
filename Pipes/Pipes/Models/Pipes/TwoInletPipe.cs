using System.Collections.Generic;
using Pipes.Constants;
using Pipes.Models.Lets;
using Pipes.Models.TieBreakers;
using SharedResources.SharedResources;

namespace Pipes.Models.Pipes
{
    public interface ITwoInletPipe<TMessageType>
    {
        Inlet<TMessageType> LeftInlet { get; }
        Inlet<TMessageType> RightInlet { get; }
        Outlet<TMessageType> Outlet { get; }
    }

    public class TwoInletPipe<TMessageType> : ITwoInletPipe<TMessageType>, IPipe<TMessageType>
    {
        public Inlet<TMessageType> LeftInlet { get; private set; }
        public Inlet<TMessageType> RightInlet { get; private set; }
        public Outlet<TMessageType> Outlet { get; private set; }

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

            LeftInlet = new Inlet<TMessageType>(this, leftInletResource);
            RightInlet = new Inlet<TMessageType>(this, rightInletResource);
            Outlet = new Outlet<TMessageType>(this, outletResource);
            
            resourceGroup.FreeSharedResources();
        }

        internal static TwoInletPipe<TMessageType> CreateRandomised(double leftProbability)
        {
            return new TwoInletPipe<TMessageType>(leftProbability);
        }

        internal static TwoInletPipe<TMessageType> CreatePrioritised(Priority priority)
        {
            return new TwoInletPipe<TMessageType>(priority);
        }

        internal static TwoInletPipe<TMessageType> CreateAlternated(Alternated alternated)
        {
            return new TwoInletPipe<TMessageType>(alternated);
        }

        internal static TwoInletPipe<TMessageType> Create(ITieBreaker tieBreaker)
        {
            return new TwoInletPipe<TMessageType>(tieBreaker);
        }

        IReadOnlyCollection<Inlet<TMessageType>> IPipe<TMessageType>.Inlets
        {
            get { return new[] {LeftInlet, RightInlet}; }
        }

        IReadOnlyCollection<Outlet<TMessageType>> IPipe<TMessageType>.Outlets
        {
            get { return new[] {Outlet}; }
        }
    }
}