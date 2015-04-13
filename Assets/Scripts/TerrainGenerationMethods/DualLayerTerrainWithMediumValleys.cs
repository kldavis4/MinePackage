// This would be a good basis for a river world
public class DualLayerTerrainWithMediumValleys : ITerrainGenerationMethod
{
    public void GenerateTerrain(WorldData worldData, Chunk chunk)
    {
        int chunkBlockX = chunk.ArrayX * worldData.ChunkBlockWidth;
        int chunkBlockY = chunk.ArrayY * worldData.ChunkBlockHeight;

        for (int x = 0; x < worldData.ChunkBlockWidth; x++)
        {
            int globalBlockX = chunkBlockX + x + worldData.MapBlockOffset.X;

            for (int y = 0; y < worldData.ChunkBlockHeight; y++)
            {
                int globalBlockY = chunkBlockY + y + worldData.MapBlockOffset.Y;

                float lowerGroundHeight = GetLowerGroundHeight(chunk, globalBlockX, globalBlockY, x, y,
                                                               worldData.DepthInBlocks);
                int groundHeight = GetUpperGroundHeight(worldData, globalBlockX, globalBlockY, lowerGroundHeight);

                bool sunlit = true;
                for (int z = worldData.DepthInBlocks - 1; z >= 0; z--)
                {
                    // Everything above ground height...is air.
                    BlockType blockType;
                    if (z > groundHeight)
                    {
                        blockType = BlockType.Air;
                    }
                        // Are we above the lower ground height?
                    else if (z > lowerGroundHeight)
                    {
                        // Let's see about some caves er valleys!
                        float octave1 =
                            PerlinSimplexNoise.noise(globalBlockX * 0.01f, globalBlockY * 0.01f, z * 0.01f) *
                            (0.015f * z) + 0.1f;
                        float caveNoise = octave1 +
                                          PerlinSimplexNoise.noise(globalBlockX * 0.01f, globalBlockY * 0.01f, z * 0.1f) *
                                          0.06f +
                                          0.1f;
                        caveNoise += PerlinSimplexNoise.noise(globalBlockX * 0.2f, globalBlockY * 0.2f, z * 0.2f) *
                                     0.03f +
                                     0.01f;
                        // We have a cave, draw air here.
                        if (caveNoise > 0.2f)
                        {
                            blockType = BlockType.Air;
                        }
                        else
                        {
                            if (sunlit)
                            {
                                blockType = BlockType.TopSoil;
                                // Remember, this adds the block global coordinates
                                chunk.TopSoilBlocks.Add(new Vector3i(globalBlockX, globalBlockY, z));
                                sunlit = false;
                            }
                            else
                            {
                                blockType = BlockType.Dirt;
                                if (caveNoise < 0.2f)
                                {
                                    blockType = BlockType.Stone;
                                }
                            }
                        }
                    }
                    else
                    {
                        // We are at the lower ground level
                        if (sunlit)
                        {
                            blockType = BlockType.TopSoil;
                            chunk.TopSoilBlocks.Add(new Vector3i(globalBlockX, globalBlockY, z));
                            sunlit = false;
                        }
                        else
                        {
                            blockType = BlockType.Dirt;
                        }
                    }

                    chunk.Blocks[x, y, z].Type = blockType;
                }
            }
        }
    }

    private static int GetUpperGroundHeight(WorldData worldData, int blockWorldX, int blockWorldY,
                                            float lowerGroundHeight)
    {
        float octave1 = PerlinSimplexNoise.noise(blockWorldX * 0.001f, blockWorldY * 0.001f) * 0.5f;
        float octave2 = PerlinSimplexNoise.noise((blockWorldX + 100) * 0.002f, blockWorldY * 0.002f) * 0.25f;
        float octave3 = PerlinSimplexNoise.noise((blockWorldX + 100) * 0.01f, blockWorldY * 0.01f) * 0.25f;
        float octaveSum = octave1 + octave2 + octave3;
        return (int) (octaveSum * (worldData.DepthInBlocks / 2f)) + (int) (lowerGroundHeight);
    }


    private static float GetLowerGroundHeight(Chunk chunk, int blockWorldX, int blockWorldY, int blockXInChunk,
                                              int blockYInChunk, int worldDepthInBlocks)
    {
        int minimumGroundheight = worldDepthInBlocks / 4;
        int minimumGroundDepth = (int) (worldDepthInBlocks * 0.5f);

        float octave1 = PerlinSimplexNoise.noise(blockWorldX * 0.0001f, blockWorldY * 0.0001f) * 0.5f;
        float octave2 = PerlinSimplexNoise.noise(blockWorldX * 0.0005f, blockWorldY * 0.0005f) * 0.35f;
        float octave3 = PerlinSimplexNoise.noise(blockWorldX * 0.02f, blockWorldY * 0.02f) * 0.15f;
        float lowerGroundHeight = octave1 + octave2 + octave3;
        lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;

        for (int z = (int) lowerGroundHeight; z >= 0; z--)
        {
            chunk.Blocks[blockXInChunk, blockYInChunk, z].Type = BlockType.Dirt;
        }

        return lowerGroundHeight;
    }
}