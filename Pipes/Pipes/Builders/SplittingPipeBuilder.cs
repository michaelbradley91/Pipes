using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface ISplittingPipeBuilder<TMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the left outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> LeftOutlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the right outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> RightOutlet { get; set; }

        ISplittingPipe<TMessage> Build();
    }

    public class SplittingPipeBuilder<TMessage> : ISplittingPipeBuilder<TMessage>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TMessage>> Inlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> LeftOutlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TMessage>> RightOutlet { get; set; }

        public SplittingPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TMessage>(p);
            LeftOutlet = p => new SimpleOutlet<TMessage>(p);
            RightOutlet = p => new SimpleOutlet<TMessage>(p);
        }

        public ISplittingPipe<TMessage> Build()
        {
            SplittingPipe<TMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe>(() => pipe[0]);

            var inlet = Inlet(lazyPipe);
            var leftOutlet = LeftOutlet(lazyPipe);
            var rightOutlet = RightOutlet(lazyPipe);

            pipe[0] = new SplittingPipe<TMessage>(inlet, leftOutlet, rightOutlet);

            return pipe[0];
        }
    }
}