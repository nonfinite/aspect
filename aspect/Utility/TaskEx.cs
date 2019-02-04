using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Aspect.Utility
{
    public static class TaskEx
    {
        public static ConfiguredTaskAwaitable DontCaptureContext(this Task task) => task.ConfigureAwait(false);

        public static ConfiguredTaskAwaitable<TResult> DontCaptureContext<TResult>(this Task<TResult> task) =>
            task.ConfigureAwait(false);
    }
}
