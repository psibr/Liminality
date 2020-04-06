using System;
using Microsoft.Extensions.DependencyInjection;

namespace PSIBR.Liminality
{
    public sealed class StateMachineFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public StateMachineFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public StateMachineScope<TStateMachine> CreateScopedStateMachine<TStateMachine>(Func<Engine<TStateMachine>, TStateMachine> factoryFunc)
        where TStateMachine : StateMachine<TStateMachine>
        {
            var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            
            var engine = scope.ServiceProvider.GetRequiredService<Engine<TStateMachine>>();

            var stateMachine = factoryFunc(engine);

            return new StateMachineScope<TStateMachine>(scope, stateMachine);
        }

        public TStateMachine CreateStateMachine<TStateMachine>(Func<Engine<TStateMachine>, TStateMachine> factoryFunc)
        where TStateMachine : StateMachine<TStateMachine>
        {           
            var engine = _serviceProvider.GetRequiredService<Engine<TStateMachine>>();

            var stateMachine = factoryFunc(engine);

            return stateMachine;
        }
    }

    public class StateMachineScope<TStateMachine>
        : IDisposable
    where TStateMachine : StateMachine<TStateMachine>
    {
        private readonly IServiceScope _serviceScope;

        public StateMachineScope(IServiceScope serviceScope, TStateMachine stateMachine)
        {
            _serviceScope = serviceScope;
            StateMachine = stateMachine;
        }

        public TStateMachine StateMachine { get; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _serviceScope.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}