using System;
using System.Threading;

namespace SharedResources.Helpers
{
    /// <summary>
    /// A gate allows for multiple threads to enter through a gate until a thread closes the gate. When the gate is closed,
    /// new threads will not allowed through the gate until every thread that entered the gate has left.
    /// 
    /// This is a useful tool to enforce fairness between threads.
    /// 
    /// Locking only occurs between threads that asked for the gate to be closed, so there is practically speaking no global lock.
    /// </summary>
    internal interface IGateway
    {
        /// <summary>
        /// Enter through the gate. Returns a pass to use in future method calls.
        /// A gate pass is not thread safe and should be used by only one thread at once.
        /// </summary>
        Gateway.Pass Enter();

        /// <summary>
        /// Leave through the gate. Expires your pass, so you must enter the gate to get a new one.
        /// </summary>
        void Leave(Gateway.Pass pass);

        /// <summary>
        /// Closes the gate until everyone who asked for the gate to be closed, including you, has left through the gate.
        /// </summary>
        void Close(Gateway.Pass pass);
    }

    internal class Gateway : IGateway
    {
        private bool gateClosed;
        private readonly Semaphore gateSemaphore;
        private int numberOfGateClosers;
        private readonly Semaphore gateControlSemaphore;

        public Gateway()
        {
            gateClosed = false;
            gateSemaphore = new Semaphore(1, 1);
            numberOfGateClosers = 0;
            gateControlSemaphore = new Semaphore(1, 1);
        }

        public Pass Enter()
        {
            if (!gateClosed) return new Pass();

            gateSemaphore.WaitOne();
            gateSemaphore.Release();
            return new Pass();
        }

        public void Leave(Pass pass)
        {
            if (pass.Expired) throw new ArgumentException("You cannot leave through the gate with an expired gate pass", "pass");

            if (pass.ClosedTheGate)
            {
                gateControlSemaphore.WaitOne();
                numberOfGateClosers--;
                if (numberOfGateClosers == 0)
                {
                    gateClosed = false;
                    gateSemaphore.Release();
                }
                gateControlSemaphore.Release();
            }
            
            pass.Expired = true;
        }

        public void Close(Pass pass)
        {
            if (pass.Expired) throw new ArgumentException("You cannot close the gate with an expired gate pass", "pass");
            if (pass.ClosedTheGate) return;

            gateControlSemaphore.WaitOne();
            if (!gateClosed)
            {
                gateSemaphore.WaitOne();
                // This is safe if it is reordered by the compiler because the other wait above is followed by an immediate release.
                gateClosed = true;
            }
            numberOfGateClosers++;
            pass.ClosedTheGate = true;
            gateControlSemaphore.Release();
        }

        internal class Pass
        {
            internal bool ClosedTheGate;
            internal bool Expired;
        }
    }
}
