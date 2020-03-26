using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface ISignalHandler<TSignal>
    {
        ValueTask InvokeAsync(TSignal signal, CancellationToken cancellationToken = default) => new ValueTask();
    }
}
