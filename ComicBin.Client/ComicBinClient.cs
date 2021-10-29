using Akavache;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Reactive.Linq;

namespace ComicBin.Client
{
    internal class ComicBinClient : IComicBinClient
    {
        public ComicBinClient(HttpClient httpClient, IUriBuilder uriBuilder, IBlobCache cache)
        {
            _uriBuilder = uriBuilder;
            _httpClient = httpClient;
            _cache = cache;
            _taskQueue = new ConcurrentQueue<Func<Task>>();
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken)
        {
            var uri = _uriBuilder.Build("allbooks");
            return await _httpClient.GetFromJsonAsync<IEnumerable<Book>>(uri, cancellationToken).ConfigureAwait(false)
                ?? Enumerable.Empty<Book>();
        }

        public async Task<Stream> GetCoverAsync(string bookId, CancellationToken cancellationToken)
        {
            var data = await _cache.GetOrFetchObject<byte[]>(
                $"cover/{bookId}",
                async () =>
                {
                    var taskSource = new TaskCompletionSource<byte[]>();
                    lock (_taskQueue)
                    {
                        _taskQueue.Enqueue(async () =>
                        {
                            try
                            {
                                var uri = _uriBuilder.Build("cover", bookId);
                                var bytes = await _httpClient.GetByteArrayAsync(uri, cancellationToken).ConfigureAwait(false);
                                taskSource.SetResult(bytes);
                            }
                            catch (Exception ex)
                            {
                                taskSource.SetException(ex);
                            }
                        });
                        if (!_processingQueue)
                        {
                            this.ProcessQueue();
                        }
                    }
                    return await taskSource.Task;
                });
            return new MemoryStream(data);
        }

        private async void ProcessQueue()
        {
            _processingQueue = true;
            async Task RunProcessLoopAsync()
            {
                while (_taskQueue.TryDequeue(out var task))
                {
                    await task().ConfigureAwait(false);
                }
            }
            var loopTasks = Enumerable.Range(0, QueueConcurrency).Select(_ => RunProcessLoopAsync()).ToList();
            await Task.WhenAll(loopTasks).ConfigureAwait(false);
            _processingQueue = false;
        }


        private bool _processingQueue;

        private readonly ConcurrentQueue<Func<Task>> _taskQueue;
        private readonly IUriBuilder _uriBuilder;
        private readonly HttpClient _httpClient;
        private readonly Akavache.IBlobCache _cache;

        private static readonly int QueueConcurrency = 4;
    }
}
