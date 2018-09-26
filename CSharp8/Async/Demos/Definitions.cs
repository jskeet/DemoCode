using System.Threading.Tasks;

namespace System
{
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}

namespace System.Collections.Generic
{
    public interface IAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetAsyncEnumerator();
    }

    public interface IAsyncEnumerator<out T>
    {
        Task<bool> WaitForNextAsync();
        T TryGetNext(out bool success);
    }
}
