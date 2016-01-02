using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface IBasicPipeBuilder<TMessage>
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

        IBasicPipe<TMessage> Build();
    }

    public class BasicPipeBuilder<TMessage> : IBasicPipeBuilder<TMessage>
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TMessage>> Outlet { get; set; }

        public BasicPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TMessage>(p);
            Outlet = p => new SimpleOutlet<TMessage>(p);
        }

        public IBasicPipe<TMessage> Build()
        {
            var promisedPipe = new Promised<IPipe>();

            var inlet = Inlet(promisedPipe);
            var outlet = Outlet(promisedPipe);

            return promisedPipe.Fulfill(new BasicPipe<TMessage>(inlet, outlet));
        }
    }
}