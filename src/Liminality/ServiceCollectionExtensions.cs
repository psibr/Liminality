using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace PSIBR.Liminality
{
    public static class ServiceCollectionExtensions
    {
        public static void AddStateMachine<TStateMachine>(
            this IServiceCollection services,
            Func<StateMachineBuilder, StateMachineDefinition> definitionBuilder)
        where TStateMachine : StateMachine<TStateMachine>
        {
            RegisterStateMachineDependencies<TStateMachine>(services, definitionBuilder);
        }

        private static void RegisterStateMachineDependencies<TStateMachine>(
            IServiceCollection services,
            Func<StateMachineBuilder, StateMachineDefinition> definitionBuilder)
        where TStateMachine : StateMachine<TStateMachine>
        {
            StateMachineDefinition<TStateMachine> definition = (StateMachineDefinition<TStateMachine>)definitionBuilder(new StateMachineBuilder<TStateMachine>());

            services.AddSingleton(definition);
            services.AddScoped<Engine<TStateMachine>>();

            foreach (var stateMachineComponent in definition.Values
                .Select(transition =>
                    transition.PreconditionType is null
                        ? new[] { transition.NewStateType }
                        : new[] { transition.NewStateType, transition.PreconditionType })
                .SelectMany(types => types)
                .Distinct())
            {
                services.AddScoped(stateMachineComponent);
            }
        }
    }
}
