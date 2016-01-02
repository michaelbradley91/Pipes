using System;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using Pipes.Models.Utilities;

namespace Pipes.Builders
{
    public interface ITransformPipeBuilder<TSourceMessage, TTargetMessage>
    {
        /// <summary>
        /// A function that, given the pipe, will produce the inlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleInlet<TSourceMessage>> Inlet { get; set; }

        /// <summary>
        /// A function that, given the pipe, will produce the outlet to be used by that pipe.
        /// The pipe is wrapped in a lazy construct as it does not exist at the time this is called, so you cannot access
        /// the pipe in the inlet's constructor.
        /// </summary>
        Func<IPromised<IPipe>, ISimpleOutlet<TTargetMessage>> Outlet { get; set; }

        /// <summary>
        /// The function passed in may be run by pipes any number of times while resolving where a message
        /// is sent. It should also not rely on the acquisition of any other pipes.
        /// 
        /// Generally, this should be a simple stateless function - for the same input it should return the same output.
        /// Try other functions at your own risk
        /// </summary>
        ITransformPipeWithMapBuilder<TSourceMessage, TTargetMessage> WithMap(Func<TSourceMessage, TTargetMessage> map);
    }

    public class TransformPipeBuilder<TSourceMessage, TTargetMessage> : ITransformPipeBuilder<TSourceMessage, TTargetMessage>
    {
        public Func<IPromised<IPipe>, ISimpleInlet<TSourceMessage>> Inlet { get; set; }
        public Func<IPromised<IPipe>, ISimpleOutlet<TTargetMessage>> Outlet { get; set; }

        public TransformPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TSourceMessage>(p);
            Outlet = p => new SimpleOutlet<TTargetMessage>(p);
        }

        public ITransformPipeWithMapBuilder<TSourceMessage, TTargetMessage> WithMap(Func<TSourceMessage, TTargetMessage> map)
        {
            var transformPipeWithBuilder = new TransformPipeWithMapBuilder<TSourceMessage, TTargetMessage>(map)
            {
                Inlet = Inlet,
                Outlet = Outlet
            };
            return transformPipeWithBuilder;
        }
    }
}