using System;
using System.Collections.Generic;
using System.Threading;

public interface IBatchProcessor<T>
{
    void Process(List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish);
    void Process(int numberOfThreads, List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish);
}

/// <summary>
/// Given a list of items to process (like generating terrain for chunks), process them on multiple
/// threads.
/// </summary>
/// <typeparam name="T"></typeparam>
public class BatchProcessor<T> : IBatchProcessor<T> where T : class
{
    private const int NumberOfThreads = 2;
    private List<T>[] itemLists;
    // Process a list of items on the ideal number of threads
    public void Process(List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish)
    {
        Process(NumberOfThreads, itemsToProcess, action, waitUntilAllThreadsFinish);
    }

    // Process a list of items using the given number of threads.
    public void Process(int numberOfThreads, List<T> itemsToProcess, Action<T> action, bool waitUntilAllThreadsFinish)
    {
        itemLists = new List<T>[NumberOfThreads];
        List<Thread> newThreads = new List<Thread>();

        int threadIndex = 0;
        foreach (T item in itemsToProcess)
        {
            itemLists[threadIndex].Add(item);
            threadIndex = (threadIndex + 1) % itemsToProcess.Count;
        }
        for (int tIndex = 0; tIndex < numberOfThreads; tIndex++)
        {
            int index = tIndex;
            Thread newThread =
                new Thread(() => ActionAgainstMultiple(itemsToProcess, 0, itemLists[index].Count, action));
            newThreads.Add(newThread);
        }

        if (waitUntilAllThreadsFinish)
        {
            foreach (Thread newThread in newThreads)
            {
                newThread.Join();
            }
        }
    }


    private static void ActionAgainstMultiple(IList<T> items, int startIndex, int length, Action<T> action)
    {
        for (int index = startIndex; index < startIndex + length; index++)
        {
            T item = items[index];
            if (item != null)
            {
                action(item);
            }

            Thread.Sleep(1);
        }
    }
}