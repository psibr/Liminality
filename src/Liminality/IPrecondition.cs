using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface IPrecondition<TSignal>
    {
        ValueTask<AggregateException?> CheckAsync(TSignal signal, CancellationToken cancellationToken = default);
    }
}
