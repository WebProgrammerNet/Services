using System;
using System.Collections.Concurrent;//ConcurrentQueue
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Services.TaskQueue
{
    class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> workItems = 
            new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim signal = new SemaphoreSlim(0);

        public int Size => workItems.Count;

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken);
            workItems.TryDequeue(out var workItem);
            return workItem;
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItems == null)
            {
                throw new ArgumentNullException(nameof(workItems));
            }
            workItems.Enqueue(workItem);
            signal.Release();
        }
    }
}
