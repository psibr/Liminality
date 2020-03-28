using System;
using System.Collections.Generic;

namespace PSIBR.Liminality
{
    public class Resolver
    {
        private readonly IServiceProvider _serviceProvider;

        public Resolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Resolution<TSignal>? Resolve<TSignal>(StateMachineDefinition definition, Type stateType)
            where TSignal : class, new()
            {
                 if(!definition.TryGetValue(new StateMachineDefinition.Input(stateType: stateType, signalType: typeof(TSignal)), out var transition))
                    return new Nullable<Resolution<TSignal>>();

                IPrecondition<TSignal>? precondition = !(transition.PreconditionType is null)
                    ? _serviceProvider.GetService(transition.PreconditionType) as IPrecondition<TSignal> 
                    : null;
                    
                var state = (ISignalHandler<TSignal>)_serviceProvider.GetService(transition.NewStateType);

                return new Resolution<TSignal>(precondition, state);
            }
    }

    public struct Resolution<TSignal>
        where TSignal : class, new()
    {
        public IPrecondition<TSignal>? Precondition;
        public ISignalHandler<TSignal> State;

        public Resolution(IPrecondition<TSignal>? precondition, ISignalHandler<TSignal> state)
        {
            this.Precondition = precondition;
            this.State = state;
        }

        public void Deconstruct(out IPrecondition<TSignal>? precondition, out ISignalHandler<TSignal>? state)
        {
            precondition = this.Precondition;
            state = this.State;
        }

        public override bool Equals(object? obj)
        {
            return obj is Resolution<TSignal> resolution &&
                   EqualityComparer<IPrecondition<TSignal>?>.Default.Equals(Precondition, resolution.Precondition) &&
                   EqualityComparer<ISignalHandler<TSignal>?>.Default.Equals(State, resolution.State);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Precondition, State);
        }
    }
}
