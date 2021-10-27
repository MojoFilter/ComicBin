using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace ComicBin.Client
{
    internal class ComicBinClient : IComicBinClient
    {
        public ComicBinClient(HttpClient httpClient, IUriBuilder uriBuilder)
        {
            _uriBuilder = uriBuilder;
            _httpClient = httpClient;
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
            var taskSource = new TaskCompletionSource<Stream>();
            lock (_taskQueue)
            {
                _taskQueue.Enqueue(async () =>
                {
                    try
                    {
                        var uri = _uriBuilder.Build("cover", bookId);
                        var stream = await _httpClient.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);
                        taskSource.SetResult(stream);
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
        }

        private async void ProcessQueue()
        {
            _processingQueue = true;
            while (_taskQueue.TryDequeue(out var task))
            {
                await task().ConfigureAwait(false);
            }
            _processingQueue = false;
        }

        private bool _processingQueue;

        private readonly ConcurrentQueue<Func<Task>> _taskQueue;
        private readonly IUriBuilder _uriBuilder;
        private readonly HttpClient _httpClient;
    }
}
