using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ICapacityPipeBuilder<TMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        int Capacity { get; set; }

        ICapacityPipeBuilder<TMessage> WithCapacity(int capacity);

        ICapacityPipe<TMessage> Build();
    }

    public class CapacityPipeBuilder<TMessage> : ICapacityPipeBuilder<TMessage>
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }
        public int Capacity { get; set; }

        public CapacityPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TMessage>(p);
            Outlet = p => new SimpleOutlet<TMessage>(p);
        }   

        public ICapacityPipe<TMessage> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlet = Inlet(promisedPipe);
            var outlet = Outlet(promisedPipe);

            return promisedPipe.Fulfill(new CapacityPipe<TMessage>(inlet, outlet, Capacity));
        }

        public ICapacityPipeBuilder<TMessage> WithCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity), "The pipeCapacity of a pipeCapacity pipe cannot be negative");
            Capacity = capacity;
            return this;
        }
    }
}