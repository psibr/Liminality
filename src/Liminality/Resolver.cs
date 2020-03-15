using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    using TransitionMap = Dictionary<(Type CurrentState, Type Signal), (Type? Precondition, Type NewState)>;

    public class Resolver
    {
        private readonly IServiceProvider _serviceProvider;

        public Resolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public MaybeResolution<TSignal> Resolve<TSignal>(TransitionMap transitionMap, Type state)
            where TSignal : class, new()
            => transitionMap.TryGetValue((state, typeof(TSignal)), out var destination)
                ? new MaybeResolution<TSignal>(new Resolution<TSignal>(/* TODO: Actually resolve */))
                : new MaybeResolution<TSignal>();
    }

    public struct MaybeResolution<TSignal> where TSignal : class, new()
    {
        public bool foundMatch;
        public Resolution<TSignal> resolution;

        public MaybeResolution(Resolution<TSignal> resolution)
        {
            this.foundMatch = true;
            this.resolution = resolution;
        }

        public override bool Equals(object? obj)
        {
            return obj is MaybeResolution<TSignal> other &&
                   foundMatch == other.foundMatch &&
                   EqualityComparer<Resolution<TSignal>>.Default.Equals(resolution, other.resolution);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(foundMatch, resolution);
        }

        public void Deconstruct(out bool foundMatch, out Resolution<TSignal> resolution)
        {
            foundMatch = this.foundMatch;
            resolution = this.resolution;
        }
    }

    public struct Resolution<TSignal>
        where TSignal : class, new()
    {
        public IPrecondition<TSignal> Precondition;
        public ISignalHandler<TSignal> State;

        public Resolution(IPrecondition<TSignal> precondition, ISignalHandler<TSignal> state)
        {
            this.Precondition = precondition;
            this.State = state;
        }

        public override bool Equals(object? obj)
        {
            return obj is Resolution<TSignal> other &&
                   EqualityComparer<IPrecondition<TSignal>>.Default.Equals(Precondition, other.Precondition) &&
                   EqualityComparer<ISignalHandler<TSignal>>.Default.Equals(State, other.State);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Precondition, State);
        }

        public void Deconstruct(out IPrecondition<TSignal> precondition, out ISignalHandler<TSignal> state)
        {
            precondition = this.Precondition;
            state = this.State;
        }
    }
}
