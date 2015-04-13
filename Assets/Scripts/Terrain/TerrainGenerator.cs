using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITerrainGenerator
{
    //void CreateTerrainData(List<Chunk> chunks);
    void GenerateTerrain(Chunk chunk);
    void GenerateTerrain(List<Chunk> chunks);
}

public class TerrainGenerator : ITerrainGenerator
{
    private readonly WorldData m_WorldData;
    private readonly IChunkProcessor m_ChunkProcessor;
    private readonly IBatchProcessor<Chunk> m_BatchProcessor;
    private readonly ITerrainGenerationMethod m_TerrainGenerationMethod;


    public TerrainGenerator(WorldData worldData,IChunkProcessor chunkProcessor, IBatchProcessor<Chunk> batchProcessor, ITerrainGenerationMethod terrainGenerationMethod)
    {
        m_WorldData = worldData;
        m_ChunkProcessor = chunkProcessor;
        m_BatchProcessor = batchProcessor;
        m_TerrainGenerationMethod = terrainGenerationMethod;
    }

    public void GenerateTerrain(Chunk chunk)
    {
        try
        {
            m_TerrainGenerationMethod.GenerateTerrain(m_WorldData, chunk);
            m_ChunkProcessor.AddChunkToDecorationQueue(chunk);
        }
        catch(Exception e)
        {
            Debug.Log("TG exception: " + e.Message);
        }
    }


    public void GenerateTerrain(List<Chunk> chunks)
    {
        if (chunks.Count == 0)
        {
            return;
        }
        m_BatchProcessor.Process(chunks, GenerateTerrain, true);
    }

}