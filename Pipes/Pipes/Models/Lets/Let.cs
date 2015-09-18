using System;
using Pipes.Extensions;
using Pipes.Models.Pipes;
using SharedResources.SharedResources;

namespace Pipes.Models.Lets
{
    public abstract class Let<TMessage>
    {
        internal readonly IPipe<TMessage> Pipe;

        private readonly SharedResource resource;
        private SharedResourceGroup activeResourceGroup;

        internal Let(IPipe<TMessage> pipe, SharedResource resource)
        {
            this.resource = resource;
            Pipe = pipe;

            resource.AssociatedObject = this;
            activeResourceGroup = null;
        }

        protected void Lock()
        {
            activeResourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource);
        }

        protected void LockWith(Let<TMessage> otherLet)
        {
            activeResourceGroup = SharedResourceGroup.CreateAcquiringSharedResources(resource, otherLet.resource);
        }

        protected void Unlock()
        {
            activeResourceGroup.FreeSharedResources();
        }

        protected void Connect(Inlet<TMessage> inlet, Outlet<TMessage> outlet, bool checkForCycles)
        {
            Try(() =>
            {
                if (!inlet.ReadyToConnect()) throw new InvalidOperationException("The inlet was not ready to connect.");
                if (!outlet.ReadyToConnect()) throw new InvalidOperationException("The outlet was not ready to connect.");

                inlet.ConnectedOutlet = outlet;
                outlet.ConnectedInlet = inlet;

                if (checkForCycles && inlet.Pipe.CreateGraphOfPipeSystem().ContainsCycle())
                {
                    inlet.ConnectedOutlet = null;
                    outlet.ConnectedInlet = null;
                    throw new InvalidOperationException("Connecting these pipes creates a cycle.");
                }

                activeResourceGroup.ConnectSharedResources(inlet.resource, outlet.resource);

                // TODO RESOLVE PIPE SYSTEM
            });
        }

        /// <summary>
        /// Called when the pipe system is already locked.
        /// Return true if and only if this "let" can be connected.
        /// </summary>
        protected abstract bool ReadyToConnect();

        protected void Disconnect(Inlet<TMessage> inlet, Outlet<TMessage> outlet)
        {
            Try(() =>
            {
                if (inlet.ConnectedOutlet != outlet || outlet.ConnectedInlet != inlet) throw new InvalidOperationException("The inlet and outlet were not connected");

                inlet.ConnectedOutlet = null;
                outlet.ConnectedInlet = null;

                activeResourceGroup.DisconnectSharedResources(inlet.resource, outlet.resource);

                // TODO RESOLVE PIPE SYSTEM (if necessary)
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
