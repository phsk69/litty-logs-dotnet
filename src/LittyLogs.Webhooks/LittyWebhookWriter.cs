using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using LittyLogs.Webhooks.Formatters;

namespace LittyLogs.Webhooks;

/// <summary>
/// the async engine that batches log messages and yeets them to webhooks via HTTP 🪝🔥
/// Channel-based: Enqueue() returns immediately (non-blocking),
/// background task collects batches and POSTs them.
/// best-effort delivery — if the webhook is bricked after retries, we drop the batch and move on.
/// we NEVER crash the app over a failed webhook no cap 🔒
/// </summary>
internal sealed class LittyWebhookWriter : IAsyncDisposable, IDisposable
{
    private readonly LittyWebhookOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebhookPayloadFormatter _formatter;
    private readonly Channel<WebhookMessage> _channel;
    private readonly Task _consumerTask;
    private readonly CancellationTokenSource _cts = new();

    public LittyWebhookWriter(
        IHttpClientFactory httpClientFactory,
        IWebhookPayloadFormatter formatter,
        LittyWebhookOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _formatter = formatter;
        _options = options;

        // bounded channel with 10k capacity — if the app logs faster than we can POST,
        // we drop the oldest entries. better than OOM-ing bestie 💀
        _channel = Channel.CreateBounded<WebhookMessage>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        // fire up the background consumer — batching + HTTP all the way down 🔥
        _consumerTask = Task.Run(() => ConsumeAsync(_cts.Token));
    }

    /// <summary>
    /// enqueues a formatted message for async webhook delivery. returns immediately — non-blocking king 👑
    /// </summary>
    public void Enqueue(WebhookMessage message)
    {
        _channel.Writer.TryWrite(message);
    }

    private async Task ConsumeAsync(CancellationToken ct)
    {
        var batch = new List<string>();

        try
        {
            while (!ct.IsCancellationRequested)
            {
                batch.Clear();

                // wait for at least one message to arrive
                if (!await _channel.Reader.WaitToReadAsync(ct))
                    break; // channel completed, we out

                // Slack gets 50 blocks total, so one header + 49 logs is the hard ceiling 🔒🔥
                var batchSize = GetSafeBatchSize();

                // drain up to the safe batch size, or until BatchInterval expires
                using var batchCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                batchCts.CancelAfter(_options.BatchInterval);

                try
                {
                    while (batch.Count < batchSize)
                    {
                        if (_channel.Reader.TryRead(out var msg))
                        {
                            batch.Add(msg.FormattedText);
                        }
                        else
                        {
                            // no more messages right now — wait for more or timeout
                            if (!await _channel.Reader.WaitToReadAsync(batchCts.Token))
                                break; // channel completed
                        }
                    }
                }
                catch (OperationCanceledException) when (batchCts.IsCancellationRequested && !ct.IsCancellationRequested)
                {
                    // batch interval expired, flush what we got — this is normal bestie 🫡
                }

                if (batch.Count > 0)
                {
                    await FlushBatchAsync(batch);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // disposal cancellation, we out 🫡
        }
        finally
        {
            // drain any remaining items and flush one last batch before shutting down
            var finalBatch = new List<string>();
            var finalBatchSize = GetSafeBatchSize();
            while (_channel.Reader.TryRead(out var remaining))
            {
                finalBatch.Add(remaining.FormattedText);
                if (finalBatch.Count == finalBatchSize)
                {
                    await FlushBatchAsync(finalBatch);
                    finalBatch.Clear();
                }
            }

            if (finalBatch.Count > 0)
            {
                await FlushBatchAsync(finalBatch);
            }
        }
    }

    private int GetSafeBatchSize() => _options.Platform == WebhookPlatform.Slack
        ? Math.Clamp(_options.BatchSize, 1, 49)
        : Math.Max(_options.BatchSize, 1);

    private async Task FlushBatchAsync(List<string> batch)
    {
        try
        {
            var payload = _formatter.FormatPayload(batch, _options);
            var httpClient = _httpClientFactory.CreateClient("LittyWebhooks");

            using var content = new StringContent(payload, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var response = await httpClient.PostAsync(_options.WebhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                // webhook returned an L but we dont crash — best effort delivery no cap
                Console.Error.WriteLine(
                    $"[LittyWebhooks] webhook returned {(int)response.StatusCode} thats not it 💀 " +
                    $"(batch of {batch.Count} messages dropped)");
            }
        }
        catch (ObjectDisposedException)
        {
            // DI container or resilience pipeline disposed during shutdown — totally normal
            // the app is going down anyway, silently drop the batch 🫡
        }
        catch (Exception ex)
        {
            // something is mega bricked but we still dont crash the app
            // polly already retried, circuit breaker already tripped — we just drop it
            Console.Error.WriteLine(
                $"[LittyWebhooks] webhook POST failed after retries: {ex.Message} 💀 " +
                $"(batch of {batch.Count} messages dropped, app keeps vibing)");
        }
    }

    public async ValueTask DisposeAsync()
    {
        // complete the channel — consumer will drain remaining items 🔒
        _channel.Writer.Complete();

        // give the consumer a sec to flush, then cancel if its stuck
        using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            await _consumerTask.WaitAsync(shutdownCts.Token);
        }
        catch (TimeoutException)
        {
            // consumer is stuck, cancel it — we tried bestie 🫡
            await _cts.CancelAsync();
            await _consumerTask;
        }

        _cts.Dispose();
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        _consumerTask.GetAwaiter().GetResult();
        _cts.Dispose();
    }
}
