using System;
using Pipes.Extensions;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public interface ILet<TMessage>
    {
        /// <summary>
        /// The pipe this in/outlet is a part of
        /// </summary>
        IPipe<TMessage> Pipe { get; } 
        
        /// <summary>
        /// The shared resource held by this in/outlet.
        /// </summary>
        SharedResource SharedResource { get; }
    }

    public abstract class Let<TMessage> : ILet<TMessage>
    {
        public IPipe<TMessage> Pipe => pipe.Value;
        public SharedResource SharedResource { get; }

        private readonly Lazy<IPipe<TMessage>> pipe;
        private SharedResourceGroup activeResourceGroup;

        protected Let(Lazy<IPipe<TMessage>> pipe, SharedResource sharedResource)
        {
            SharedResource = sharedResource;
            this.pipe = pipe;

            sharedResource.AssociatedObject = this;
            activeResourceGroup = null;
        }

        protected void Lock()
        {
            activeResourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(SharedResource);
        }

        protected void LockWith(ILet<TMessage> otherLet)
        {
            activeResourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(SharedResource, otherLet.SharedResource);
        }

        protected void Unlock()
        {
            activeResourceGroup.FreeSharedResources();
        }

        protected void Connect(IInlet<TMessage> inlet, IOutlet<TMessage> outlet, bool checkPipeSystemFormsTree)
        {
            Try(() =>
            {
                if (!inlet.CanConnect()) throw new InvalidOperationException("The inlet was not ready to connect.");
                if (!outlet.CanConnect()) throw new InvalidOperationException("The outlet was not ready to connect.");

                inlet.ConnectedOutlet = outlet;
                outlet.ConnectedInlet = inlet;

                if (checkPipeSystemFormsTree && !inlet.Pipe.CreateGraphOfPipeSystem().IsTree())
                {
                    inlet.ConnectedOutlet = null;
                    outlet.ConnectedInlet = null;
                    throw new InvalidOperationException("Connecting these pipes results in a pipe system that does not form a tree.");
                }

                activeResourceGroup.ConnectSharedResources(inlet.SharedResource, outlet.SharedResource);

                while (true)
                {
                    var sender = outlet.Pipe.FindSender(outlet);
                    if (sender == null) return;

                    var receiver = inlet.Pipe.FindReceiver(inlet);
                    if (receiver != null)
                    {
                        receiver(sender());
                    }
                    else
                    {
                        return;
                    }
                }
            });
        }

        protected void Disconnect(IInlet<TMessage> inlet, IOutlet<TMessage> outlet)
        {
            Try(() =>
            {
                if (inlet.ConnectedOutlet != outlet || outlet.ConnectedInlet != inlet) throw new InvalidOperationException("The inlet and outlet were not connected");

                inlet.ConnectedOutlet = null;
                outlet.ConnectedInlet = null;

                activeResourceGroup.DisconnectSharedResources(inlet.SharedResource, outlet.SharedResource);
            });
        }

        private void Try(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                activeResourceGroup.FreeSharedResources();
                throw;
            }
        }
    }
}
