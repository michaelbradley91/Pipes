using System;

namespace Pipes.Models.Utilities
{
    public interface IPromised<T>
    {
        T GetPromisedObject();

        /// <summary>
        /// Sets the promised object with the value given. Also returns the value supplied for convenience.
        /// </summary>
        TP Fulfill<TP>(TP p) where TP : T;
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
       
        public TP Fulfill<TP>(TP p) where TP : T
        {
            promiseFulfilled = true;
            obj = p;
            return p;
        }
    }
}
