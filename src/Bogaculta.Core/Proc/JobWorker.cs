using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Bogaculta.Models;

namespace Bogaculta.Proc
{
    public sealed class JobWorker
    {
        private readonly ConcurrentQueue<Job> _taskQueue;
        private readonly int _taskCount;

        public JobWorker()
        {
            _taskQueue = new ConcurrentQueue<Job>();
            _taskCount = Environment.ProcessorCount;
        }

        public void Enqueue(Job job)
        {
            _taskQueue.Enqueue(job);
        }

        private CancellationTokenSource _token;

        public void Start()
        {
            _token = new CancellationTokenSource();
            var threads = new Task[_taskCount];
            for (var i = 0; i < _taskCount; i++)
            {
                threads[i] = Task.Factory.StartNew(Consume);
            }
        }

        private void Consume()
        {
            while (!_token.IsCancellationRequested)
            {
                if (_taskQueue.TryDequeue(out var task))
                {
                    var thread = Thread.CurrentThread;
                    var threadId = thread.ManagedThreadId;

                    Console.WriteLine($"Thread {threadId} processes task {task}");
                    Thread.Sleep(100);
                }
            }
        }
    }
}