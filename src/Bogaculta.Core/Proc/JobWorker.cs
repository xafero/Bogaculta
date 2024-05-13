using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bogaculta.Check;
using Bogaculta.IO;
using Bogaculta.Models;

namespace Bogaculta.Proc
{
    public sealed class JobWorker
    {
        private readonly BlockingCollection<Job> _taskQueue;
        private readonly int _taskCount;

        public JobWorker()
        {
            _taskQueue = new BlockingCollection<Job>();
            _taskCount = Environment.ProcessorCount;
        }

        public void Enqueue(Job job)
        {
            _taskQueue.Add(job);
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

        private async void Consume()
        {
            try
            {
                await ConsumeIn();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task ConsumeIn()
        {
            while (!_token.IsCancellationRequested)
            {
                if (_taskQueue.Take(_token.Token) is { } task)
                {
                    var thread = Thread.CurrentThread;
                    var threadId = thread.ManagedThreadId;

                    task.Worker = $"{threadId}";
                    switch (task.Kind)
                    {
                        case JobKind.Move:
                            await FileTask.DoMove(task, _token.Token);
                            break;
                        case JobKind.Verify:
                            await HashTask.DoVerify(task, _token.Token);
                            break;
                        case JobKind.Hash:
                            await HashTask.DoHash(task, _token.Token);
                            break;
                        default:
                            task.SetError("Kind is unspecified!");
                            break;
                    }
                }
            }
        }

        public void Stop()
        {
            _token.Cancel();
            _taskQueue.Dispose();
        }
    }
}