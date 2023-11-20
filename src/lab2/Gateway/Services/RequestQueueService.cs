using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using Gateway.Utils;

namespace Gateway.Services
{
    public class RequestQueueService
    {
        private readonly HttpClient _httpClient = new();
        private static object locker = new();
        private readonly CircuitBreaker _circuitBreaker;
        private readonly ConcurrentQueue<HttpRequestMessage> _requestMessagesQueue = new();

        private const int TimeoutInSeconds = 10;


        public RequestQueueService()
        {
            _circuitBreaker = CircuitBreaker.Instance;
        }

        public void AddRequestToQueue(HttpRequestMessage httpRequestMessage)
        {
            lock (locker)
            {
                _requestMessagesQueue.Enqueue(httpRequestMessage);
            }
        }

        public void StartWorker()
        {
            new Thread(Start).Start();
        }

     

        private async void Start(object? state)
        {
            while (true)
            {
                lock (locker)
                {
                    if (!_requestMessagesQueue.TryPeek(out var req))
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    try
                    {
                        var res = _httpClient.Send(req);
                        if (res.IsSuccessStatusCode)
                        {
                            _requestMessagesQueue.TryDequeue(out _);
                            _circuitBreaker.ResetFailureCount();
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(TimeoutInSeconds));
                        }
                    }
                    catch (Exception e)
                    {
                        var reqClone = HttpRequestMessageHelper.CloneHttpRequestMessageAsync(req).GetAwaiter().GetResult();
                        _requestMessagesQueue.TryDequeue(out _);
                        _requestMessagesQueue.Enqueue(reqClone);

                        Thread.Sleep(TimeSpan.FromSeconds(TimeoutInSeconds));
                    }
                }
            }
        }
    }
}
