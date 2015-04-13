using System;
using System.Collections.Generic;
using UnityEngine;

public enum QueueType
{
    NewTerrain,
    Decoration,
    Lighting,
    MeshData,
    Mesh
}


public interface IChunkProcessor
{
    //void AddChunksToDecorationQueue(List<Chunk> chunks);
    void AddChunksToLightingQueue(List<Chunk> chunks);
    void AddChunksToMeshDataCreationQueue(List<Chunk> chunks);
    void AddChunksToMeshCreationQueue(List<Chunk> chunks);
    List<Chunk> GetChunksForTerrainGeneration();
    List<Chunk> GetChunksForDecoration();
    List<Chunk> GetChunksForLighting();
    List<Chunk> GetChunksForMeshDataGeneration();
    void ClearDecorationQueue();
    void AddChunkToTerrainQueue(Chunk chunk);
    bool ChunksAreBeingAdded { get; set; }
    bool ChunkBatchesAreReadyForProcessing { get; }
    void AddChunkToDecorationQueue(Chunk chunk);
    void AddBatchOfChunks(List<Chunk> chunks, BatchType batchType);
    ChunkBatch GetBatchOfChunksToProcess();
}

public class ChunkProcessor : IChunkProcessor
{
    private readonly TQueue<Chunk> m_TerrainQueue = new TQueue<Chunk>();
    private readonly TQueue<Chunk> m_DecorationQueue = new TQueue<Chunk>();
    private readonly TQueue<Chunk> m_LightingQueue = new TQueue<Chunk>();
    private readonly TQueue<Chunk> m_MeshDataCreationQueue = new TQueue<Chunk>();
    private readonly TQueue<Chunk> m_MeshCreationQueue = new TQueue<Chunk>();
    private readonly Queue<Chunk> m_PrefabRemovalQueue = new Queue<Chunk>();

    public TQueue<Chunk> LightingQueue
    {
        get { return m_LightingQueue; }
    }

    public TQueue<Chunk> MeshDataCreationQueue
    {
        get { return m_MeshDataCreationQueue; }
    }

    public TQueue<Chunk> MeshCreationQueue
    {
        get { return m_MeshCreationQueue; }
    }

    public Queue<Chunk> PrefabRemovalQueue
    {
        get { return m_PrefabRemovalQueue; }
    }

    public void AddChunksToLightingQueue(List<Chunk> chunks)
    {
        EnqueueChunks(chunks, LightingQueue);
    }

    public void AddChunksToMeshDataCreationQueue(List<Chunk> chunks)
    {
        EnqueueChunks(chunks, MeshDataCreationQueue);
    }

    public void AddChunksToMeshCreationQueue(List<Chunk> chunks)
    {
        EnqueueChunks(chunks, MeshCreationQueue);
    }

    public List<Chunk> GetChunksForTerrainGeneration()
    {
        List<Chunk> chunksForTerrainGeneration = DequeueChunks(m_TerrainQueue); 
        return chunksForTerrainGeneration; 
    }

    /// <summary>
    /// This makes sure that every process after terrain generation ignores
    /// border chunks.
    /// </summary>
    /// <returns></returns>
    public List<Chunk> GetChunksForDecoration()
    {
        return GetChunksNotOnTheBorderInQueue(m_DecorationQueue);
    }

    private static List<Chunk> GetChunksNotOnTheBorderInQueue(TQueue<Chunk> queue)
    {
        List<Chunk> chunks = new List<Chunk>();
        while (queue.Count > 0)
        {
            Chunk chunk = queue.Dequeue();
            if (!chunk.IsOnTheBorder)
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    /// <summary>
    /// Lighting needs to only process border chunks
    /// Note: We do this here AND in decoration because someone may just want to relight a chunk
    /// </summary>
    /// <returns></returns>
    public List<Chunk> GetChunksForLighting()
    {
        return GetChunksNotOnTheBorderInQueue(m_LightingQueue); 
    }

    public List<Chunk> GetChunksForMeshDataGeneration()
    {
        return DequeueChunks(m_MeshDataCreationQueue);
    }

    public void ClearDecorationQueue()
    {
        m_DecorationQueue.Clear();
    }

    private static List<Chunk> DequeueChunks(TQueue<Chunk> queue)
    {
        List<Chunk> chunks = new List<Chunk>();
        while (queue.Count > 0)
        {
            chunks.Add(queue.Dequeue());
        }

        return chunks;
    }

    private static void EnqueueChunks(List<Chunk> chunks, TQueue<Chunk> queue)
    {
        foreach (Chunk chunk in chunks)
        {
            queue.Enqueue(chunk);
        }
    }

    public void AddChunkToTerrainQueue(Chunk chunk)
    {
        m_TerrainQueue.Enqueue(chunk);
    }

    public bool ChunksAreBeingAdded { get; set; }

    public bool ChunkBatchesAreReadyForProcessing
    {
        get { return m_ChunkBatches.Count > 0; }
    }

    public void AddChunkToDecorationQueue(Chunk chunk)
    {
        if (!chunk.IsOnTheBorder && !m_DecorationQueue.Contains(chunk))
        {
            m_DecorationQueue.Enqueue(chunk);
        }
    }

    public void ClearLightingQueue()
    {
        m_LightingQueue.Clear();
    }

    public void AddChunkToLightingQueue(Chunk originalChunk)
    {
        m_LightingQueue.Enqueue(originalChunk);
    }

    private readonly TQueue<ChunkBatch> m_ChunkBatches = new TQueue<ChunkBatch>();
    public void AddBatchOfChunks(List<Chunk> chunks, BatchType batchType)
    {
        m_ChunkBatches.Enqueue(new ChunkBatch(chunks, batchType));
    }

    public ChunkBatch GetBatchOfChunksToProcess()
    {
        if (m_ChunkBatches.Count == 0)
        {
            return null;
        }
        return m_ChunkBatches.Dequeue();
    }
}