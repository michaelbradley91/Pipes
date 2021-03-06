﻿namespace Pipes.Builders
{
    public interface IPipeBuilder
    {
        IBasicPipeBuilder<TMessage> BasicPipe<TMessage>();
        ICapacityPipeBuilder<TMessage> CapacityPipe<TMessage>();
        IEitherInletPipeBuilder<TMessage> EitherInletPipe<TMessage>();
        IEitherOutletPipeBuilder<TMessage> EitherOutletPipe<TMessage>();
        ISplittingPipeBuilder<TMessage> SplittingPipe<TMessage>();
        ITransformPipeBuilder<TSourceMessage, TTargetMessage> TransformPipe<TSourceMessage, TTargetMessage>();
        ISinkPipeBuilder<TMessage> SinkPipe<TMessage>();
        ISourcePipeBuilder<TMessage> SourcePipe<TMessage>();
        IValvedPipeBuilder<TReceive, TSend> ValvedPipe<TReceive, TSend>();
        IBigEitherInletPipeBuilder<TMessage> BigEitherInletPipe<TMessage>();
        IBigEitherOutletPipeBuilder<TMessage> BigEitherOutletPipe<TMessage>();
        IBigSplittingPipeBuilder<TMessage> BigSplittingPipe<TMessage>();
        IBigValvedPipeBuilder<TReceive, TSend> BigValvedPipe<TReceive, TSend>();
    }

    public class PipeBuilder : IPipeBuilder
    {
        public static readonly IPipeBuilder New = new PipeBuilder();

        public IBasicPipeBuilder<TMessage> BasicPipe<TMessage>()
        {
            return new BasicPipeBuilder<TMessage>();
        }

        public ICapacityPipeBuilder<TMessage> CapacityPipe<TMessage>()
        {
            return new CapacityPipeBuilder<TMessage>();
        }

        public IEitherInletPipeBuilder<TMessage> EitherInletPipe<TMessage>()
        {
            return new EitherInletPipeBuilder<TMessage>();
        }

        public IEitherOutletPipeBuilder<TMessage> EitherOutletPipe<TMessage>()
        {
            return new EitherOutletPipeBuilder<TMessage>();
        }

        public ISplittingPipeBuilder<TMessage> SplittingPipe<TMessage>()
        {
            return new SplittingPipeBuilder<TMessage>();
        }

        public ITransformPipeBuilder<TSourceMessage, TTargetMessage> TransformPipe<TSourceMessage, TTargetMessage>()
        {
            return new TransformPipeBuilder<TSourceMessage, TTargetMessage>();
        }

        public ISinkPipeBuilder<TMessage> SinkPipe<TMessage>()
        {
            return new SinkPipeBuilder<TMessage>();
        }

        public ISourcePipeBuilder<TMessage> SourcePipe<TMessage>()
        {
            return new SourcePipeBuilder<TMessage>();
        }

        public IValvedPipeBuilder<TReceive, TSend> ValvedPipe<TReceive, TSend>()
        {
            return new ValvedPipeBuilder<TReceive, TSend>();
        }

        public IBigEitherInletPipeBuilder<TMessage> BigEitherInletPipe<TMessage>()
        {
            return new BigEitherInletPipeBuilder<TMessage>();
        }

        public IBigEitherOutletPipeBuilder<TMessage> BigEitherOutletPipe<TMessage>()
        {
            return new BigEitherOutletPipeBuilder<TMessage>();
        }

        public IBigSplittingPipeBuilder<TMessage> BigSplittingPipe<TMessage>()
        {
            return new BigSplittingPipeBuilder<TMessage>();
        }

        public IBigValvedPipeBuilder<TReceive, TSend> BigValvedPipe<TReceive, TSend>()
        {
            return new BigValvedPipeBuilder<TReceive, TSend>();
        }
    }
}