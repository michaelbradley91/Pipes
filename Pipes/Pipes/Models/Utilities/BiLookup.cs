using System.Collections.Generic;

namespace Pipes.Models.Utilities
{
    public interface IBiLookup<TLeft, TRight>
    {
        void Add(TLeft first, TRight second);
        IList<TRight> this[TLeft first] { get; }
        IList<TLeft> this[TRight second] { get; }
        IList<TRight> GetByLeft(TLeft first);
        IList<TLeft> GetByRight(TRight second);
    }

    public class BiLookup<TLeft, TRight> : IBiLookup<TLeft, TRight>
    {
        private readonly IDictionary<TLeft, IList<TRight>> leftToRight = new Dictionary<TLeft, IList<TRight>>();
        private readonly IDictionary<TRight, IList<TLeft>> rightToLeft = new Dictionary<TRight, IList<TLeft>>();
        
        public void Add(TLeft first, TRight second)
        {
            IList<TLeft> firsts;
            IList<TRight> seconds;
            if (!leftToRight.TryGetValue(first, out seconds))
            {
                seconds = new List<TRight>();
                leftToRight[first] = seconds;
            }
            if (!rightToLeft.TryGetValue(second, out firsts))
            {
                firsts = new List<TLeft>();
                rightToLeft[second] = firsts;
            }
            seconds.Add(second);
            firsts.Add(first);
        }
        
        public IList<TRight> this[TLeft first] => GetByLeft(first);
        public IList<TLeft> this[TRight second] => GetByRight(second);

        public IList<TRight> GetByLeft(TLeft first)
        {
            IList<TRight> list;
            if (!leftToRight.TryGetValue(first, out list))
            {
                return new TRight[0];
            }
            return new List<TRight>(list);
        }

        public IList<TLeft> GetByRight(TRight second)
        {
            IList<TLeft> list;
            if (!rightToLeft.TryGetValue(second, out list))
            {
                return new TLeft[0];
            }
            return new List<TLeft>(list);
        }
    }
}