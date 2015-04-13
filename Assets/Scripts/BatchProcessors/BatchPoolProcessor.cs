using System;
using System.Collections.Generic;
using System.Threading;

public class BatchPoolProcessor<T> : IBatchProcessor<T> where T : class
{
    public void Process(List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinishh)
    {
        Process(1, itemsToProcess, action, waitUntilAllThreadsFinishh);
    }

    public void Process(int numberOfThreads, List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish)
    {
        ThreadPool.SetMaxThreads(10, 10);
        if (itemsToProcess.Count == 0)
        {
            return;
        }
        CountdownWaitHandle countdownWaitHandle = new CountdownWaitHandle(itemsToProcess.Count);
        foreach (T item in itemsToProcess)
        {
            T item1 = item;
            ThreadPool.QueueUserWorkItem(state =>
                                             {
                                                 action(item1);
                                                 countdownWaitHandle.Signal();
                                             });
        }

        if (true)
        {
            countdownWaitHandle.WaitOne();
        }
    }


}
