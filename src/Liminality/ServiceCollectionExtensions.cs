using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace PSIBR.Liminality
{
    public static class ServiceCollectionExtensions
    {
        public static void AddLiminality(this IServiceCollection services)
        {
            services.AddScoped<LiminalEngine>();
        }

        public static void AddStateMachine<TStateMachine>(
            this IServiceCollection services,
            Func<StateMachineBuilder, StateMachineStateMap> definitionBuilder)
        where TStateMachine : StateMachine<TStateMachine>
        {
            var stateMap = definitionBuilder(new StateMachineBuilder());
            var definition = new StateMachineDefinition<TStateMachine>(stateMap);

            services.AddSingleton(definition);

            services.AddTransient<TStateMachine>();

            RegisterStateMachineDependencies(services, definition);
        }

        private static void RegisterStateMachineDependencies<TStateMachine>(
            IServiceCollection services,
            StateMachineDefinition<TStateMachine> definition)
        where TStateMachine : StateMachine<TStateMachine>
        {
            foreach (var stateMachineComponent in definition.GetStateTypes())
            {
                services.AddScoped(stateMachineComponent);
            }
        }
    }
}
