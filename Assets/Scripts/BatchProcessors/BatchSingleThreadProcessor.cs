using System;
using System.Collections.Generic;

public class BatchSingleThreadProcessor<T> : IBatchProcessor<T> where T : class
{
    public void Process(List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish)
    {
        Process(1, itemsToProcess, action, waitUntilAllThreadsFinish);
    }

    public void Process(int numberOfThreads, List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish)
    {
        foreach (T item in itemsToProcess)
        {
            action(item);
        }
    }
}