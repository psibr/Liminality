using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace PSIBR.Liminality.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddLiminality(this IServiceCollection services)
        {
            services.AddScoped<Resolver>();
            services.AddScoped<StateMachineFactory>();
        }

        public static void AddTypedStateMachineFactory<TFactory, TStateMachine>(
            this IServiceCollection services,
            Func<StateMachineBuilder, StateMachineDefinition> definitionBuilder)
        where TFactory : class
        where TStateMachine : StateMachine<TStateMachine>
        {
            RegisterStateMachineDependencies<TStateMachine>(services, definitionBuilder);

            services.AddScoped<TFactory>();
        }

        public static void AddTypedStateMachineFactory<TFactory, TStateMachine>(
            this IServiceCollection services,
            Func<StateMachineBuilder, StateMachineDefinition> definitionBuilder,
            Func<IServiceProvider, TFactory> implementationProvider)
        where TFactory : class
        where TStateMachine : StateMachine<TStateMachine>
        {
            RegisterStateMachineDependencies<TStateMachine>(services, definitionBuilder);

            services.AddScoped<TFactory>(implementationProvider);
        }

        private static void RegisterStateMachineDependencies<TStateMachine>(
            IServiceCollection services,
            Func<StateMachineBuilder, StateMachineDefinition> definitionBuilder)
        where TStateMachine : StateMachine<TStateMachine>
        {
            var definition = (StateMachineDefinition<TStateMachine>)definitionBuilder(new StateMachineBuilder<TStateMachine>());

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

            services.AddSingleton<StateMachineDefinition<TStateMachine>>(definition);
        }
    }
}
