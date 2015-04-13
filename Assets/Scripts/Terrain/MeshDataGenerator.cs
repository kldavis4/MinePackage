using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMeshDataGenerator
{
    void GenerateMeshData(List<Chunk> chunks);
    void GenerateMeshData(Chunk chunk);
}

public class MeshDataGenerator : IMeshDataGenerator
{
    private readonly IBatchProcessor<Chunk> m_BatchProcessor;
    private readonly WorldData m_WorldData;
    private readonly ChunkProcessor m_ChunkProcessor;
    private readonly Queue<Chunk> m_Finished = new Queue<Chunk>();

    public MeshDataGenerator(IBatchProcessor<Chunk> batchProcessor, WorldData worldData, ChunkProcessor chunkProcessor)
    {
        m_BatchProcessor = batchProcessor;
        m_WorldData = worldData;
        m_ChunkProcessor = chunkProcessor;
    }


    public void GenerateMeshData(List<Chunk> chunks)
    {
        m_BatchProcessor.Process(chunks, GenerateMeshData, true);
        chunks.Sort(ChunksComparedByDistanceFromMapCenter);
        m_ChunkProcessor.AddChunksToMeshCreationQueue(chunks);
    }

    // Chunk distance comparison

    private int ChunksComparedByDistanceFromMapCenter(Chunk firstChunk,
                                                      Chunk secondChunk)
    {
        Vector3 mapCenter = new Vector3(m_WorldData.CenterChunkX, m_WorldData.CenterChunkY, 0);
        return Vector3.Distance(
            new Vector3(firstChunk.ArrayX, firstChunk.ArrayY, firstChunk.ArrayZ), mapCenter).
            CompareTo(
                (int)
                Vector3.Distance(new Vector3(secondChunk.ArrayX, secondChunk.ArrayY, secondChunk.ArrayZ), mapCenter));
    }

    public void GenerateMeshData(Chunk chunk)
    {
        if (chunk.IsOnTheBorder)
        {
            return;
        }
        GenerateChunkMeshData(chunk);
    }


    /// <summary>
    /// We don't actually generate the mesh, that needs to happen on the main
    /// Unity thread. But we CAN go ahead and generate the mesh DATA, based on block visiblity.
    /// </summary>
    /// <param name="chunk"></param>
    private void GenerateChunkMeshData(Chunk chunk)
    {

        //DateTime start = DateTime.Now;
        int index = 0;
        chunk.Vertices = new List<Vector3>();
        chunk.Indices = new List<int>();
        chunk.Uvs = new List<Vector2>();
        chunk.Colors = new List<Color>();
        int chunkX = chunk.ArrayX * m_WorldData.ChunkBlockWidth;
        int chunkY = chunk.ArrayY * m_WorldData.ChunkBlockHeight;
        int chunkZ = chunk.ArrayZ * m_WorldData.ChunkBlockDepth;

        for (int x = 0; x < m_WorldData.ChunkBlockWidth; x++)
        {
            int blockX = chunkX + x;
            for (int y = 0; y < m_WorldData.ChunkBlockHeight; y++)
            {
                int blockY = chunkY + y;
                for (int z = 1; z < m_WorldData.ChunkBlockDepth - 1; z++)
                {
                    int blockZ = chunkZ + z;

                    try
                    {
                        index = CreateDataMeshForBlock(blockX, blockY, blockZ, chunk, x, y, z, index);
                    }
                    catch (Exception e)
                    {
                        // This should only happen if our chunks are too big and have a mesh that has
                        // too many veticies for Unity
                        Debug.Log("Mesh data exception " + e.Message + " Chunk: " + chunk + ", " + e.StackTrace);
                    }
                }
            }
        }

        //Debug.Log("Mesh Data generation took " + (DateTime.Now - start));
    }

    private int CreateDataMeshForBlock(int blockX, int blockY, int blockZ, Chunk chunk, int x, int y, int z, int index)
    {
        Block currentBlock = chunk.Blocks[x, y, z];
        // Bail if the current block is solid we are processing.
        if ((currentBlock.Type != BlockType.Air))
        {
            return index;
        }

        byte lightAmount = currentBlock.LightAmount;

        // "South" side
        BlockType blockType = m_WorldData.GetBlock(blockX, blockY - 1, blockZ).Type;

        if (blockType != BlockType.Air)
        {
            // The block is solid. Just add its info to the mesh,
            // using our current air block's light amount for its lighting.

            AddBlockSide(x + 1, y, z, x + 1, y,
                         z + 1, x, y,
                         z + 1, x, y, z, 0.5f, chunk, index, blockType, BlockFace.Side, lightAmount);
            index += 4;
        }

        // west block
        blockType = m_WorldData.GetBlock(blockX - 1, blockY, blockZ).Type;
        if (blockType != BlockType.Air)
        {
            AddBlockSide(x, y, z,
                         x, y, z + 1,
                         x, y + 1, z + 1,
                         x, y + 1, z, 0.8f, chunk, index, blockType, BlockFace.Side, lightAmount);
            index += 4;
        }

        // north side
        blockType = m_WorldData.GetBlock(blockX, blockY + 1, blockZ).Type;
        if (blockType != BlockType.Air)
        {
            AddBlockSide(x, y + 1, z,
                         x, y + 1, z + 1,
                         x + 1, y + 1, z + 1,
                         x + 1, y + 1, z,
                         0.9f, chunk, index, blockType, BlockFace.Side, lightAmount);

            index += 4;
        }

        // East side
        blockType = m_WorldData.GetBlock(blockX + 1, blockY, blockZ).Type;
        if (blockType != BlockType.Air)
        {
            AddBlockSide(x + 1, y + 1,
                         z, x + 1, y + 1,
                         z + 1, x + 1, y,
                         z + 1, x + 1, y, z, 0.7f, chunk, index, blockType, BlockFace.Side, lightAmount);

            index += 4;
        }

        // Block above
        blockType = m_WorldData.GetBlock(blockX, blockY, blockZ + 1).Type;
        if (blockType != BlockType.Air)
        {
            AddBlockSide(x + 1, y,
                         z + 1, x + 1, y + 1,
                         z + 1, x, y + 1,
                         z + 1, x, y, z + 1, 0.4f, chunk, index, blockType, BlockFace.Bottom, lightAmount);

            index += 4;
        }

        // Block below
        blockType = m_WorldData.GetBlock(blockX, blockY, blockZ - 1).Type;
        if (blockType != BlockType.Air)
        {
            AddBlockSide(x, y,
                         z, x, y + 1,
                         z, x + 1, y + 1,
                         z, x + 1, y, z,
                         1.0f, chunk, index, blockType, BlockFace.Top, lightAmount);

            index += 4;
        }

        return index;
    }


    private void AddBlockSide(int x1, int z1, int y1, int x2, int z2, int y2, int x3, int z3, int y3, int x4,
                              int z4, int y4, float color, Chunk chunk, int index, BlockType blockType,
                              BlockFace blockFace,
                              byte blockLight)
    {
        // Add a block's side, add the information to the mesh lists for the chunk.

        float actualColor = (color * blockLight) / 255;
        const float epsilon = 0.001f;
        chunk.Vertices.Add(new Vector3(x1, y1, z1));
        chunk.Vertices.Add(new Vector3(x2, y2, z2));
        chunk.Vertices.Add(new Vector3(x3, y3, z3));
        chunk.Vertices.Add(new Vector3(x4, y4, z4));

        var item = new Color(actualColor, actualColor, actualColor);
        chunk.Colors.Add(item);
        chunk.Colors.Add(item);
        chunk.Colors.Add(item);
        chunk.Colors.Add(item);

        chunk.Indices.Add(index);
        chunk.Indices.Add(index + 1);
        chunk.Indices.Add(index + 2);

        chunk.Indices.Add(index + 2);
        chunk.Indices.Add(index + 3);
        chunk.Indices.Add(index);

        Rect worldTextureAtlasUv =
            m_WorldData.BlockUvCoordinates[(int) blockType].BlockFaceUvCoordinates[(int) blockFace];

        chunk.Uvs.Add(new Vector2(worldTextureAtlasUv.x + epsilon, worldTextureAtlasUv.y + epsilon));
        chunk.Uvs.Add(new Vector2(worldTextureAtlasUv.x + epsilon,
                                  worldTextureAtlasUv.y + worldTextureAtlasUv.height - epsilon));
        chunk.Uvs.Add(new Vector2(worldTextureAtlasUv.x + worldTextureAtlasUv.width - epsilon,
                                  worldTextureAtlasUv.y + worldTextureAtlasUv.height - epsilon));
        chunk.Uvs.Add(new Vector2(worldTextureAtlasUv.x + worldTextureAtlasUv.width - epsilon,
                                  worldTextureAtlasUv.y + epsilon));
    }
}