using System.Threading;
using System.Threading.Tasks;

namespace PSIBR.Liminality
{
    public interface IPrecondition<TSignal>
    {
        ValueTask<bool> CheckAsync(TSignal signal, CancellationToken cancellationToken = default);
    }
}
