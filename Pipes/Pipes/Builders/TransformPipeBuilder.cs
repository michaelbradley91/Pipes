using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;

namespace Pipes.Builders
{
    public interface ITransformPipeBuilder<TSourceMessage, TTargetMessage>
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

        ITransformPipeWithMapBuilder<TSourceMessage, TTargetMessage> WithMap(Func<TSourceMessage, TTargetMessage> map);
    }

    public class TransformPipeBuilder<TSourceMessage, TTargetMessage> : ITransformPipeBuilder<TSourceMessage, TTargetMessage>
    {
        public Func<Lazy<IPipe>, ISimpleInlet<TSourceMessage>> Inlet { get; set; }
        public Func<Lazy<IPipe>, ISimpleOutlet<TTargetMessage>> Outlet { get; set; }

        public TransformPipeBuilder()
        {
            Inlet = p => new SimpleInlet<TSourceMessage>(p, SharedResourceHelpers.CreateSharedResource());
            Outlet = p => new SimpleOutlet<TTargetMessage>(p, SharedResourceHelpers.CreateSharedResource());
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