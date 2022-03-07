using System;
using System.Collections.Generic;
using System.Linq;

namespace PSIBR.Liminality
{
    public class StateMachineDefinition<TStateMachine>
    where TStateMachine : StateMachine<TStateMachine>
    {
        public StateMachineDefinition(StateMachineStateMap map!!)
        {
            StateMap = map;
        }

        public StateMachineStateMap StateMap { get; }

        public IEnumerable<Type> GetStateTypes() =>
            StateMap.Values
                .Select(transition => new[] { transition.NewStateType })
                .SelectMany(types => types)
                .Distinct();
    }
}