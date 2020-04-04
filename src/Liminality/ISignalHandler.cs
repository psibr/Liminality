using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface ISignalHandler<TSignal>
        where TSignal : class
    {
        ValueTask<ISignalResult?> InvokeAsync(SignalContext context, TSignal signal, CancellationToken cancellationToken = default);

        ValueTask<ISignalResult?> EmptyResult() => new ValueTask<ISignalResult?>((ISignalResult?)null);
    }

    public class SignalContext
    {
        public SignalContext(IStateMachine self, object startingState, object newState)
        {
            Self = self;
            StartingState = startingState;
            NewState = newState;
        }

        public IStateMachine Self { get; }

        public object StartingState { get; }

        public object NewState { get; }
    }

    public static class SignalContextExtensions
    {
        public static ValueTask<ISignalResult?> EmptyResult(this SignalContext _) => new ValueTask<ISignalResult?>((ISignalResult?)null);
    }
}
