using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public class SignalOptions
    {
        public SignalOptions(
            bool throwWhenTransitionNotFound = false,
            bool throwWhenRejectedByPrecondition = false)
        {
            ThrowWhenTransitionNotFound = throwWhenTransitionNotFound;
            ThrowWhenRejectedByPrecondition = throwWhenRejectedByPrecondition;
        }

        public bool ThrowWhenTransitionNotFound { get; set; }
        public bool ThrowWhenRejectedByPrecondition { get; }

        public override bool Equals(object? obj)
        {
            return obj is SignalOptions options &&
                   ThrowWhenTransitionNotFound == options.ThrowWhenTransitionNotFound &&
                   ThrowWhenRejectedByPrecondition == options.ThrowWhenRejectedByPrecondition;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ThrowWhenTransitionNotFound, ThrowWhenRejectedByPrecondition);
        }
    }
}
