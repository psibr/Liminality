using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface ISignalHandler<TStateMachine, TSignal>
    where TStateMachine : StateMachine<TStateMachine>
    where TSignal : class
    {
        ValueTask<AggregateSignalResult?> InvokeAsync(SignalContext<TStateMachine> context, TSignal signal, CancellationToken cancellationToken = default);
    }

    public class SignalContext<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        public SignalContext(TStateMachine self, object startingState, object newState)
        {
            Self = self;
            StartingState = startingState;
            NewState = newState;
        }

        public TStateMachine Self { get; }
        
        public object StartingState { get; }

        public object NewState { get; }
    }
}
