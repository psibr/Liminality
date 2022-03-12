using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PSIBR.Liminality;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samples
{
    using static Samples.OperationOrchestrator;

    public static class OperationOrchestratorExtentions
    {
        public static void AddOperationOrchestrator(this IServiceCollection services)
        {
            services.AddStateMachineDependenciesFromAttributes<OperationOrchestrator>();

            services.AddScoped<Repository>();
        }
    }

    [InitialState<Created>]
    public partial class OperationOrchestrator : StateMachine<OperationOrchestrator>
    {
        public OperationOrchestrator(
            LiminalEngine engine!!,
            StateMachineDefinition<OperationOrchestrator> definition!!,
            string orchestrationId)
        : base(engine, definition)
        {
            if (string.IsNullOrWhiteSpace(orchestrationId))
            {
                throw new ArgumentException($"'{nameof(orchestrationId)}' cannot be null or whitespace.", nameof(orchestrationId));
            }
            OrchestrationId = orchestrationId;
        }

        protected object State { get; private set; } = new Created();
        public string OrchestrationId { get; }

        public async Task<AggregateSignalResult> SignalAsync<TSignal>(TSignal signal)
        where TSignal : class, new()
        {
            var valueTask = SignalAsync(
                signal,
                cancellationToken => new ValueTask<object>(State),
                (state, cancellationToken) => { State = state; return new ValueTask(); },
                default);

            return await valueTask.ConfigureAwait(false);
        }

    }
}
