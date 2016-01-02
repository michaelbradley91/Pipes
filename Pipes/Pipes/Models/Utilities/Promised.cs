using System;

namespace Pipes.Models.Utilities
{
    public interface IPromised<T>
    {
        T GetPromisedObject();

        /// <summary>
        /// Sets the promised object with the value given. Also returns the value supplied for convenience.
        /// TP must be castable to T.
        /// </summary>
        TP Fulfill<TP>(TP p);
    }

    public class Promised<T> : IPromised<T>
    {
        private T obj;
        private bool promiseFulfilled;

        public T GetPromisedObject()
        {
            if (!promiseFulfilled) throw new InvalidOperationException("Cannot retrieve promised object until the promise has been fulfilled");
            return obj;
        }
       
        public TP Fulfill<TP>(TP p)
        {
            promiseFulfilled = true;
            obj = (T) (object) p;
            return p;
        }
    }
}
