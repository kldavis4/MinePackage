using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMover
{
    private readonly WorldData m_WorldData;
    private readonly ChunkProcessor m_ChunkProcessor;

    public ChunkMover(WorldData worldData, ChunkProcessor chunkProcessor)
    {
        m_WorldData = worldData;
        m_ChunkProcessor = chunkProcessor;
    }


    public void ShiftWorldChunks(Vector3i increment)
    {
        m_ChunkProcessor.ChunksAreBeingAdded = true;
        // We copy the original, because if we moved each chunk it's a
        // "destructive" move, possibly overwriting another chunk that also needs
        // to move.
        Chunk[, ,] copyOfOriginalChunks = CreateCopyOfOriginalChunkArray(); 
        List<Chunk> newCunksAddedToMap = new List<Chunk>();
        List<Chunk> chunksToDecorate = new List<Chunk>();

        m_WorldData.MapChunkOffset = m_WorldData.MapChunkOffset - increment;
        //m_WorldData.MapChunkOffset.X -= increment.X;
        //m_WorldData.MapChunkOffset.Y -= increment.Y;
        for (int x = 0; x < m_WorldData.ChunksWide; x++)
        {
            for (int y = 0; y < m_WorldData.ChunksHigh; y++)
            {
                for (int z = 0; z < m_WorldData.ChunksDeep; z++)
                {
                    MoveChunk(copyOfOriginalChunks[x, y, z], increment, newCunksAddedToMap, chunksToDecorate);
                }
            }
        }
        m_ChunkProcessor.AddBatchOfChunks(newCunksAddedToMap, BatchType.TerrainGeneration);
        m_ChunkProcessor.AddBatchOfChunks(chunksToDecorate, BatchType.Decoration);
        m_ChunkProcessor.ChunksAreBeingAdded = false;
    }

    private Chunk[,,] CreateCopyOfOriginalChunkArray()
    {
        Chunk[,,] copy =  new Chunk[m_WorldData.Chunks.GetLength(0), m_WorldData.Chunks.GetLength(1), m_WorldData.Chunks.GetLength(2)];

        for (int x = 0; x < m_WorldData.ChunksWide; x++)
        {
            for (int y = 0; y < m_WorldData.ChunksHigh; y++)
            {
                for (int z = 0; z < m_WorldData.ChunksDeep; z++)
                {
                    copy[x, y, z] = m_WorldData.Chunks[x,y,z];
                }
            }
        }

        return copy;
    }

    private void MoveChunk(Chunk originalChunk, Vector3i increment, List<Chunk> newCunksAddedToMap, List<Chunk> chunksToDecorate)
    {
        // The array location of the chunk about to move.
        Vector3i originalPosition = new Vector3i(originalChunk.ArrayX, originalChunk.ArrayY, originalChunk.ArrayZ);
        bool wasOnBorder = originalChunk.IsOnTheBorder;
        Vector3i newPosition = new Vector3i(originalPosition.X + increment.X, originalPosition.Y+ increment.Y, 0);

        //// would this chunk shift off of the map?
        if (ChunkWouldShiftOffTheMap(newPosition))
        {
            // It will be replaced, but let's get rid of the mesh. 
            AddChunkToPrefabRemovalQueue(originalChunk);
            return;
        }

        // Was this chunk on the border, and now needs decoration, lighting, etc?
        //TODO: Handle decorated chunks moved into the border, then back inside. We don't need to redecorate them.
        MoveChunkToNewPosition(originalChunk, newPosition);
        if (wasOnBorder && IsInsideBorder(newPosition))
        {
            chunksToDecorate.Add(originalChunk);
        }

        

        // Is this a brand new chunk that needs terrain generation?
        if (ChunkMovingHereIsANewChunk(originalPosition.X, originalPosition.Y, increment))
        {
            Chunk newChunk = CreateNewChunkAt(originalPosition);
            newCunksAddedToMap.Add(newChunk);
        }
    }

    private void AddChunkToPrefabRemovalQueue(Chunk chunk)
    {
        m_ChunkProcessor.PrefabRemovalQueue.Enqueue(chunk);
    }

    private void MoveChunkToNewPosition(Chunk chunk, Vector3i newPosition)
    {
        chunk.ArrayX = newPosition.X;
        chunk.ArrayY = newPosition.Y;
        chunk.ArrayZ = newPosition.Z;

        m_WorldData.Chunks[newPosition.X, newPosition.Y, newPosition.Z] = chunk;
    }

    private bool IsInsideBorder(Vector3i position)
    {
        return position.X > 0 && position.Y > 0 && position.X < m_WorldData.RightChunkBorderColumn &&
                   position.Y < m_WorldData.TopChunkBorderRow;
    }

    /// <summary>
    /// Is this a chunk that would shift off of the map?
    /// </summary>
    private bool ChunkWouldShiftOffTheMap(Vector3i newPosition)
    {
        return (newPosition.X < 0 || newPosition.X > m_WorldData.RightChunkBorderColumn
                || newPosition.Y < 0 || newPosition.Y > m_WorldData.TopChunkBorderRow);
    }

    // Create a new chunk for this location
    private Chunk CreateNewChunkAt(Vector3i currentLocation)
    {
        Chunk chunk = Chunk.CreateChunk(currentLocation, m_WorldData);
        m_ChunkProcessor.AddChunkToTerrainQueue(chunk);
        return chunk;
    }

    /// <summary>
    /// Was the chunk moving to this location coming from off of the map?
    /// </summary>
    private bool ChunkMovingHereIsANewChunk(int x, int y, Vector3i increment)
    {
        return (x - increment.X < 0) || (x - increment.X >= m_WorldData.ChunksWide) ||
               (y - increment.Y < 0) || (y - increment.Y >= m_WorldData.ChunksWide);
    }
}