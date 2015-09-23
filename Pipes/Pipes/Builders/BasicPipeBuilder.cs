using System;
using Pipes.Helpers;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Builders
{
    public interface IBasicPipeBuilder<TMessage>
    {
        IBasicPipe<TMessage> Build();
    }

    public class BasicPipeBuilder<TMessage> : IBasicPipeBuilder<TMessage>
    {
        public IBasicPipe<TMessage> Build()
        {
            BasicPipe<TMessage>[] pipe = {null};
            var lazyPipe = new Lazy<IPipe<TMessage>>(() => pipe[0]);
            
            var inlet = new Inlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());
            var outlet = new Outlet<TMessage>(lazyPipe, SharedResourceHelpers.CreateSharedResource());

            pipe[0] = new BasicPipe<TMessage>(inlet, outlet);

            return pipe[0];
        }
    }
}