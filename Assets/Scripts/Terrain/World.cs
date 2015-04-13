using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public interface IWorld
{

}

public class World : IWorld
{
    private readonly ITerrainGenerator m_TerrainGenerator;
    private readonly WorldData m_WorldData;
    private readonly ILightProcessor m_LightProcessor;
    private readonly IMeshDataGenerator m_MeshDataGenerator;
    private readonly IWorldDecorator m_WorldDecorator;
    private readonly IChunkProcessor m_ChunkProcessor;
    private Thread m_ProcessingThread;

    private bool m_Processing;


    public World(WorldData worldData, ITerrainGenerator terrainGenerator, ILightProcessor lightProcessor,
                 IMeshDataGenerator meshDataGenerator, IWorldDecorator worldDecorator, IChunkProcessor chunkProcessor)
    {
        m_WorldData = worldData;
        m_LightProcessor = lightProcessor;
        m_MeshDataGenerator = meshDataGenerator;
        m_WorldDecorator = worldDecorator;
        m_ChunkProcessor = chunkProcessor;
        m_TerrainGenerator = terrainGenerator;
        ContinueProcessingChunks = true;
    }

    private ChunkBatch m_CurrentBatchBeingProcessed;

    private int m_BatchCount;

    /// <summary>
    /// This is the main method which processes the batches of chunks.
    /// It is responsible for calling the terrain generation, decorating, lighting,
    /// and mesh generation.
    /// It's running in a seperate thread, and never stops checking for more work to do.
    /// </summary>
    private void ProcessChunks()
    {
        while (ContinueProcessingChunks)
        {
            // Complete each batch before moving onto the next
            if (m_CurrentBatchBeingProcessed!= null || m_ChunkProcessor.ChunksAreBeingAdded)
            {
                continue;
            }


            ChunkBatch batch = m_ChunkProcessor.GetBatchOfChunksToProcess();
            if (batch == null)
            {
                continue;
            }

            m_CurrentBatchBeingProcessed = batch;
            
            DateTime start = DateTime.Now;

            if (batch.BatchType == BatchType.TerrainGeneration)
            {
                m_TerrainGenerator.GenerateTerrain(batch.Chunks);
                m_CurrentBatchBeingProcessed = null;
                continue;
            }

            m_WorldDecorator.Decorate(batch.Chunks);
            m_LightProcessor.LightChunks(batch.Chunks);
            m_MeshDataGenerator.GenerateMeshData(batch.Chunks);
            m_CurrentBatchBeingProcessed = null;

            if (batch.Chunks.Count > 0)
            {
                Debug.Log("Total Time: " + (DateTime.Now - start));
            }

            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// When set to false, stops the chunk processing thread
    /// </summary>
    public bool ContinueProcessingChunks { get; set; }

    public WorldData WorldData
    {
        get { return m_WorldData; }
    }

    public IChunkProcessor ChunkProcessor
    {
        get { return m_ChunkProcessor; }
    }

    public void InitializeGridChunks()
    {
        WorldData.InitializeGridChunks();
    }


    private static void ClearRegenerationStatus(IEnumerable<Chunk> chunks)
    {
        foreach (Chunk chunk in chunks)
        {
            chunk.NeedsRegeneration = false;
        }
    }


    // Regenerates the target chunk first, followed by any others that need regeneration.

    public void RegenerateChunks(int chunkX, int chunkY, int chunkZ)
    {
        m_ChunkProcessor.AddBatchOfChunks(new List<Chunk>(){m_WorldData.Chunks[chunkX, chunkY, chunkZ]}, BatchType.TerrainGeneration );
        //List<Chunk> chunksNeedingRegeneration = WorldData.ChunksNeedingRegeneration;
        //if (chunksNeedingRegeneration.Count == 0)
        //{
        //    return;
        //}

        //Chunk targetChunk = WorldData.Chunks[chunkX, chunkY, chunkZ];

        ////Put our target chunk as the first in the list.
        //if (chunksNeedingRegeneration.Contains(targetChunk))
        //{
        //    chunksNeedingRegeneration.Remove(targetChunk);
        //    chunksNeedingRegeneration.Insert(0, targetChunk);
        //}

        //RegenerateChunks(chunksNeedingRegeneration);
    }

    public void RegenerateChunks()
    {
        List<Chunk> chunksNeedingRegeneration = WorldData.ChunksNeedingRegeneration;
        if (chunksNeedingRegeneration.Count == 0)
        {
            return;
        }

        RegenerateChunks(chunksNeedingRegeneration);
    }

    public void RegenerateChunks(List<Chunk> chunksNeedingRegeneration)
    {
        m_ChunkProcessor.AddChunksToLightingQueue(chunksNeedingRegeneration);
        //m_LightProcessor.LightChunks(chunksNeedingRegeneration);
        //m_MeshDataGenerator.GenerateMeshData(chunksNeedingRegeneration);
        ClearRegenerationStatus(chunksNeedingRegeneration);
    }

    /// <summary>
    /// Generic block removal. 
    /// </summary>
    /// <param name="blockMapPosition"></param>
    public void RemoveBlockAt(Vector3i blockMapPosition)
    {
        WorldData.SetBlockTypeWithRegeneration(blockMapPosition.X, blockMapPosition.Y, blockMapPosition.Z, BlockType.Air);
        m_LightProcessor.RecalculateLightingAroundBlock(blockMapPosition.X, blockMapPosition.Y, blockMapPosition.Z);
        RegenerateChunks(blockMapPosition.X / WorldData.ChunkBlockHeight,
                         blockMapPosition.Y / WorldData.ChunkBlockHeight,
                         blockMapPosition.Z / WorldData.ChunkBlockDepth);
    }

    private int m_DiggingAmount = 100;

    private Vector3i m_DiggingLocation;

    private DateTime m_LastDigTime;

    private readonly TimeSpan m_DigDuration = TimeSpan.FromSeconds(0.25);

    public readonly Queue<Vector3> Diggings = new Queue<Vector3>();

    public bool ThreadisAlive
    {
        get { return m_ProcessingThread.IsAlive; }
    }

    /// <summary>
    /// Begins digging at the targetLocation. This is just simple digging now, it 
    /// doesn't know about different block types.
    /// </summary>
    /// <param name="blockMapPosition">The 'raw' map location of the block to dig in.</param>
    /// <param name="globalPosition">The exact dig point, in Unity coordinates.</param>
    public void Dig(Vector3i blockMapPosition, Vector3 globalPosition)
    {
        DateTime currentDigTime = DateTime.Now;

        // If we are digging but shift to a different block, we lose the digging amount at the original
        // block and start over.
        if (blockMapPosition != m_DiggingLocation)
        {
            // Three hits will remove the block
            m_DiggingAmount = 30;
            // Save the current digging location
            m_DiggingLocation = blockMapPosition;
            m_LastDigTime = currentDigTime;
            // Let the sparks and awesome digging sound fly
            Diggings.Enqueue(globalPosition);
        }
        else
        {
            // Have we been working on this block long enough?
            if (currentDigTime - m_LastDigTime > m_DigDuration)
            {
                // Eventually, we'll just spawn these like the gameobject decorations
                Diggings.Enqueue(globalPosition);
                // Knock off 10 whacks from the block
                m_DiggingAmount = m_DiggingAmount - 10;
                m_LastDigTime = currentDigTime;

                // All gone?
                if (m_DiggingAmount <= 0)
                {
                    RemoveBlockAt(blockMapPosition);
                    m_DiggingAmount = 100;
                }
            }
        }
    }

    /// <summary>
    /// Given a 'global' position in the entire world, get the local position
    /// of the block on the map. 
    /// </summary>
    /// <param name="targetLocation"></param>
    /// <returns></returns>
    private Vector3i GetBlockMapPosition(Vector3i targetLocation)
    {
        return targetLocation - m_WorldData.MapBlockOffset;
    }

    /// <summary>
    /// Should only be called when the application quits
    /// </summary>
    /// <param name="chunk"></param>
    public static void DestroyChunk(Chunk chunk)
    {
        GameObject chunkPrefab = chunk.ChunkTransform as GameObject;
        if (chunkPrefab != null)
        {
            Transform transform = (chunkPrefab.transform);
            MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            Object.Destroy(meshFilter.mesh);
            meshFilter.mesh = null;
            Object.Destroy(meshFilter);
            Object.Destroy(transform);
            chunk.ChunkTransform = null;
        }
        chunk = null;
    }


    //public void GenerateTerrainMeshData(DateTime start)
    //{
    //    List<Chunk> chunksForMeshDataGeneration = ChunkProcessor.GetChunksForMeshDataGeneration();
    //    if (chunksForMeshDataGeneration.Count == 0)
    //    {
    //        return;
    //    }

    //    Debug.Log(" Total Time: " + (DateTime.Now - start));
    //    m_MeshDataGenerator.GenerateMeshData(chunksForMeshDataGeneration);
    //}

    //public void LightTerrain()
    //{
    //    m_LightProcessor.LightChunks(ChunkProcessor.GetChunksForLighting());
    //}

    //public void DecorateTerrain()
    //{
    //    m_WorldDecorator.Decorate(ChunkProcessor.GetChunksForDecoration());
    //}

    //public void GenerateNewTerrain()
    //{
    //    m_TerrainGenerator.CreateTerrainData(ChunkProcessor.GetChunksForTerrainGeneration());
    //}


    public Block GetBlock(Vector3i blockLocation)
    {
        return m_WorldData.GetBlock(blockLocation.X, blockLocation.Y, blockLocation.Z);
    }

    public void StopProcessing()
    {
        m_ProcessingThread.Abort();
    }

    public void StartProcessingThread()
    {
        m_ProcessingThread = new Thread(ProcessChunks);
        m_ProcessingThread.Start();
    }

    /// <summary>
    /// Convert a block position in the global map position (from Unity space)
    /// to the local map visible to the player.
    /// </summary>
    /// <param name="globalBlockPosition"></param>
    /// <returns></returns>
    public Vector3i GlobalToLocalMapBlockPosition(Vector3i globalBlockPosition)
    {
        return globalBlockPosition - m_WorldData.MapBlockOffset;
    }
}