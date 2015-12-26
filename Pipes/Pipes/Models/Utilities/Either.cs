using System;

namespace Pipes.Models.Utilities
{
    public class Either<TLeft, TRight>
    {
        private readonly TLeft left;
        private readonly TRight right;

        public bool IsLeft { get; }
        public bool IsRight => !IsLeft;

        public Either(TLeft left)
        {
            IsLeft = true;
            this.left = left;
        }

        public Either(TRight right)
        {
            IsLeft = false;
            this.right = right;
        }

        public TLeft GetLeft()
        {
            if (!IsLeft) throw new InvalidOperationException();
            
            return left;
        }

        public TRight GetRight()
        {
            if (!IsRight) throw new InvalidOperationException();

            return right;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null) return false;
            return obj.GetType() == GetType() && Equals((Either<TLeft, TRight>)obj);
        }

        private bool Equals(Either<TLeft, TRight> other)
        {
            if (other.IsLeft != IsLeft) return false;
            return other.IsLeft ? Equals(GetLeft(), other.GetLeft()) : Equals(GetRight(), other.GetRight());
        }

        public override int GetHashCode()
        {
            if (IsLeft)
            {
                if (GetLeft() == null) return 3;
                return 3*GetLeft().GetHashCode();
            }

            if (GetRight() == null) return 5;
            return 5*GetRight().GetHashCode();
        }
    }
}
