using System.Threading;

namespace Pipes.Tests.Helpers
{
    public static class ThreadHelpers
    {
        public static Thread RunInThread(ThreadStart threadStart)
        {
            var thread = new Thread(threadStart);
            thread.Start();
            return thread;
        }
    }
}
