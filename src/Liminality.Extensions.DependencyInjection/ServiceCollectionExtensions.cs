using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace PSIBR.Liminality.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddLiminality(this IServiceCollection services)
        {
            services.AddSingleton<Resolver>();
        }

        public static void AddTypedStateMachine<TStateMachine>(this IServiceCollection services, Func<StateMachineBuilder, StateMachineDefinition> definitionBuilder)
            where TStateMachine : StateMachine<TStateMachine>
        {
            var definition = (StateMachineDefinition<TStateMachine>)definitionBuilder(new StateMachineBuilder<TStateMachine>());

            foreach (var stateMachineComponent in definition.Values
                .Select(transition => 
                    transition.PreconditionType is null
                        ? new [] { transition.NewStateType }
                        : new [] { transition.NewStateType, transition.PreconditionType } )
                .SelectMany(types => types)
                .Distinct())
            {
                services.AddTransient(stateMachineComponent);
            }

            services.AddSingleton<StateMachineDefinition<TStateMachine>>(definition);

            services.AddTransient<TStateMachine>();
        }
    }
}
