//internal class DualLayerTerrainWithMediumValleysForRivers : ITerrainGenerationMethod
//{
//    public void GenerateTerrain(WorldData worldData, Chunk chunk, int blockWorldXOffset)
//    {
//        int waterLevel = (int) (worldData.DepthInBlocks * 0.5f);
//        for (int x = 0; x < worldData.ChunkBlockWidth; x++)
//        {
//            int blockX = chunk.ArrayX * worldData.ChunkBlockWidth + x + blockWorldXOffset;

//            for (int y = 0; y < worldData.ChunkBlockHeight; y++)
//            {
//                int blockY = chunk.ArrayY * worldData.ChunkBlockHeight + y;

//                float lowerGroundHeight = GetLowerGroundHeight(chunk, blockX, blockY, x, y, worldData.DepthInBlocks);
//                int upperGroundHeight = GetUpperGroundHeight(worldData, blockX, blockY, lowerGroundHeight);

//                bool sunlit = true;

//                for (int z = worldData.DepthInBlocks - 1; z >= 0; z--)
//                {
//                    // Everything above ground height...is air.
//                    BlockType blockType;
//                    if (z > upperGroundHeight)
//                    {
//                        blockType = BlockType.Air;
//                    }
//                        // Are we above the lower ground height?
//                    else if (z > lowerGroundHeight)
//                    {
//                        // Let's see about some caves er valleys!
//                        float caveNoise = PerlinSimplexNoise.noise(blockX * 0.01f, blockY * 0.01f, z * 0.01f) *
//                                          (0.015f * z) + 0.1f;
//                        caveNoise += PerlinSimplexNoise.noise(blockX * 0.01f, blockY * 0.01f, z * 0.1f) * 0.06f + 0.1f;
//                        caveNoise += PerlinSimplexNoise.noise(blockX * 0.2f, blockY * 0.2f, z * 0.2f) * 0.03f + 0.01f;
//                        // We have a cave, draw air here.
//                        if (caveNoise > 0.2f)
//                        {
//                            blockType = BlockType.Air;
//                        }
//                        else
//                        {
//                            if (sunlit)
//                            {
//                                blockType = BlockType.TopSoil;
//                                sunlit = false;
//                            }
//                            else
//                            {
//                                blockType = BlockType.Dirt;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        // We are at the lower ground level
//                        if (sunlit)
//                        {
//                            blockType = BlockType.TopSoil;
//                            sunlit = false;
//                        }
//                        else
//                        {
//                            blockType = BlockType.Dirt;
//                        }
//                    }

//                    if (blockType == BlockType.Air)
//                    {
//                        if (z <= waterLevel)
//                        {
//                            blockType = BlockType.Lava;
//                        }
//                    }
//                    chunk.Blocks[x, y, z].Type = blockType;
//                }
//            }
//        }
//    }

//    private static int GetUpperGroundHeight(WorldData worldData, int blockX, int blockY, float lowerGroundHeight)
//    {
//        float octave1 = PerlinSimplexNoise.noise((blockX + 100) * 0.001f, blockY * 0.001f) * 0.5f;
//        float octave2 = PerlinSimplexNoise.noise((blockX + 100) * 0.002f, blockY * 0.002f) * 0.25f;
//        float octave3 = PerlinSimplexNoise.noise((blockX + 100) * 0.01f, blockY * 0.01f) * 0.25f;
//        float octaveSum = octave1 + octave2 + octave3;
//        return (int) (octaveSum * (worldData.DepthInBlocks / 2f)) + (int) (lowerGroundHeight);
//    }


//    private static float GetLowerGroundHeight(Chunk chunk, int blockX, int blockY, int blockXInChunk,
//                                              int blockYInChunk, int worldDepthInBlocks)
//    {
//        int minimumGroundheight = worldDepthInBlocks / 4;
//        int minimumGroundDepth = (int) (worldDepthInBlocks * 0.5f);

//        float octave1 = PerlinSimplexNoise.noise(blockX * 0.0001f, blockY * 0.0001f) * 0.5f;
//        float octave2 = PerlinSimplexNoise.noise(blockX * 0.0005f, blockY * 0.0005f) * 0.35f;
//        float octave3 = PerlinSimplexNoise.noise(blockX * 0.02f, blockY * 0.02f) * 0.15f;
//        float lowerGroundHeight = octave1 + octave2 + octave3;
//        lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;

//        for (int z = (int) lowerGroundHeight; z >= 0; z--)
//        {
//            chunk.Blocks[blockXInChunk, blockYInChunk, z].Type = BlockType.Dirt;
//        }

//        return lowerGroundHeight;
//    }
//}