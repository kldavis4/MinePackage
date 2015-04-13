using System;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldDecorator
{
    void Decorate(List<Chunk> chunks);

    /// <summary>
    /// For the given chunk's topsoil blocks, ask each decorator (tree, bush, etc)
    /// to decorate here. Of course, the decorator ultimately decides if it wants to 
    /// be here or not.
    /// </summary>
    /// <param name="chunk"></param>
    void GenerateDecorationsForChunk(Chunk chunk);
}

/// <summary>
/// Adds trees, shrubs, etc to the world.
/// </summary>
public class WorldDecorator : IWorldDecorator
{
    private readonly WorldData m_WorldData;
    private readonly IBatchProcessor<Chunk> m_BatchProcessor;
    private readonly List<IDecoration> m_Decorations;
    private readonly IChunkProcessor m_ChunkProcessor;

    public WorldDecorator(WorldData worldData, IBatchProcessor<Chunk> batchProcessor, IChunkProcessor chunkProcessor)
        : this(worldData, batchProcessor, LoadDecorators(worldData), chunkProcessor)
    {
    }

    /// <summary>
    /// This only exists for testing, not to be called directly by production code.
    /// </summary>
    public WorldDecorator(WorldData worldData, IBatchProcessor<Chunk> batchProcessor, List<IDecoration> decorations,
                          IChunkProcessor chunkProcessor)
    {
        m_WorldData = worldData;
        m_BatchProcessor = batchProcessor;
        m_ChunkProcessor = chunkProcessor;
        m_Decorations = decorations;
    }


    /// <summary>
    /// Finds all decorations in this assembly, creates instances of each in memory,
    /// and adds them to the list.
    /// </summary>
    public static List<IDecoration> LoadDecorators(WorldData worldData)
    {
        List<IDecoration> decorations = new List<IDecoration>();
        foreach (Type type in typeof (IDecoration).Assembly.GetTypes())
        {
            if (typeof(IDecoration).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
            {
                IDecoration decoration =
                    Activator.CreateInstance(type, new object[] {worldData}) as IDecoration;
                decorations.Add(decoration);
            }
        }

        return decorations;
    }

    public void Decorate(List<Chunk> chunks)
    {
        m_BatchProcessor.Process(chunks, GenerateDecorationsForChunk, true);
    }

    /// <summary>
    /// For the given chunk's topsoil blocks, ask each decorator (tree, bush, etc)
    /// to decorate here. Of course, the decorator ultimately decides if it wants to 
    /// be here or not.
    /// </summary>
    /// <param name="chunk"></param>
    public void GenerateDecorationsForChunk(Chunk chunk)
    {
        if (chunk.IsOnTheBorder)
        {
            return;
        }
        IRandom random = new DotNetRandom();
        try
        {
            foreach (Vector3i topSoilBlock in chunk.TopSoilBlocks)
            {
                foreach (IDecoration decoration in m_Decorations)
                {
                    // This passes in the LOCAL map block coordinate for the topsoil block
                    // we are considering for decoration.
                    if (decoration.Decorate(chunk, topSoilBlock - m_WorldData.MapBlockOffset, random))
                    {
                        continue;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception decorating " + e.Message  + "\r\n\r\n" + e.StackTrace);
        }
    }
}