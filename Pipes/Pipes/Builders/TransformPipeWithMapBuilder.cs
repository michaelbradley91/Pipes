using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface ITransformPipeWithMapBuilder<TSourceMessage, TTargetMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleInlet<TSourceMessage>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<Lazy<IPipe>, ISimpleOutlet<TTargetMessage>> Outlet { get; set; }

        /// <summary>
        /// This function may be run by pipes any number of times while resolving where a message
        /// is sent. It should also not rely on the acquisition of any other pipes.
        /// 
        /// Generally, this should be a simple stateless function - for the same input it should return the same output.
        /// Try other functions at your own risk
        /// </summary>
        Func<TSourceMessage, TTargetMessage> Map { get; set; }

        ITransformPipe<TSourceMessage, TTargetMessage> Build();
    }

    public class TransformPipeWithMapBuilder<TSourceMessage, TTargetMessage> : ITransformPipeWithMapBuilder<TSourceMessage, TTargetMessage>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TSourceMessage>> Inlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TTargetMessage>> Outlet { get; set; }
        public Func<TSourceMessage, TTargetMessage> Map { get; set; }

        public TransformPipeWithMapBuilder(Func<TSourceMessage, TTargetMessage> map)
        {
            Map = map;
            Inlet = p => new SimpleInlet<TSourceMessage>(p);
            Outlet = p => new SimpleOutlet<TTargetMessage>(p);
        }

        public ITransformPipe<TSourceMessage, TTargetMessage> Build()
        {
            TransformPipe<TSourceMessage, TTargetMessage>[] pipe = { null };
            var lazyPipe = new Lazy<IPipe>(() => pipe[0]);

            var inlet = Inlet(lazyPipe);
            var outlet = Outlet(lazyPipe);

            pipe[0] = new TransformPipe<TSourceMessage, TTargetMessage>(inlet, outlet, Map);

            return pipe[0];
        }
    }
}