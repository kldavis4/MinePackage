//public class StandardTerrainMediumCaves : ITerrainGenerationMethod
//{
//    private World m_World;

//    public World World
//    {
//        set { m_World = value; }
//    }


//    /// <summary>
//    /// Generate terrain data for a chunk.
//    /// </summary>
//    public void GenerateTerrain(WorldData worldData, Chunk chunk, int blockWorldXOffset)
//    {
//        int chunkBlockX = chunk.ArrayX * worldData.ChunkBlockWidth;
//        int chunkBlockY = chunk.ArrayY * worldData.ChunkBlockHeight;
//        for (int x = 0; x < worldData.ChunkBlockWidth; x++)
//        {
//            int blockX = chunkBlockX + x;

//            for (int y = 0; y < worldData.ChunkBlockHeight; y++)
//            {
//                int blockY = chunkBlockY + y;
//                //GenerateLargeValleys(chunk, x, y, blockX, blockY);
//                GenerateStandardTerrain(chunk, x, y, blockX, blockY, worldData.DepthInBlocks, blockWorldXOffset);
//            }
//        }
//    }

//    private void GenerateStandardTerrain(Chunk chunk, int x, int y, int blockX, int blockY, int worldDepthInBlocks,
//                                         int noiseBlockOffset)
//    {
//        int groundHeight = (int) GetBlockNoise(blockX, blockY);
//        if (groundHeight < 1)
//        {
//            groundHeight = 1;
//        }
//        else if (groundHeight > 128)
//        {
//            groundHeight = 96;
//        }

//        // Default to sunlit.. for caves
//        bool sunlit = true;
//        BlockType blockType = BlockType.Air;
//        chunk.Blocks[x, y, groundHeight].Type = BlockType.TopSoil;
//        chunk.Blocks[x, y, 0].Type = BlockType.Stone;
//        for (int z = worldDepthInBlocks - 1; z > 0; z--)
//        {
//            if (z > groundHeight)
//            {
//                blockType = BlockType.Air;
//            }

//                // Or we at or below ground height?
//            else if (z < groundHeight)
//            {
//                // Since we are at or below ground height, let's see if we need to make
//                // a cave
//                int noiseX = (blockX + noiseBlockOffset);
//                float octave1 = PerlinSimplexNoise.noise(noiseX * 0.009f, blockY * 0.009f, z * 0.009f) * 0.25f;

//                float initialNoise = octave1 +
//                                     PerlinSimplexNoise.noise(noiseX * 0.04f, blockY * 0.04f, z * 0.04f) * 0.15f;
//                initialNoise += PerlinSimplexNoise.noise(noiseX * 0.08f, blockY * 0.08f, z * 0.08f) * 0.05f;

//                if (initialNoise > 0.2f)
//                {
//                    blockType = BlockType.Air;
//                }
//                else
//                {
//                    // We've placed a block of dirt instead...nothing below us will be sunlit
//                    if (sunlit)
//                    {
//                        sunlit = false;
//                        blockType = BlockType.TopSoil;
//                        chunk.TopSoilBlocks.Add(new Vector3i(blockX, blockY, z));
//                    }
//                    else
//                    {
//                        blockType = BlockType.Dirt;
//                        if (octave1 < 0.2f)
//                        {
//                            blockType = BlockType.Stone;
//                        }
//                    }
//                }
//            }

//            chunk.Blocks[x, y, z].Type = blockType;
//        }
//    }

//    private float GetBlockNoise(int blockX, int blockY)
//    {
//        float mediumDetail = PerlinSimplexNoise.noise(blockX / 300.0f, blockY / 300.0f, 20);
//        float fineDetail = PerlinSimplexNoise.noise(blockX / 80.0f, blockY / 80.0f, 30);
//        float bigDetails = PerlinSimplexNoise.noise(blockX / 800.0f,
//                                                    blockY / 800.0f);
//        float noise = bigDetails * 64.0f + mediumDetail * 32.0f + fineDetail * 16.0f; // *(bigDetails * 64.0f);
//        return noise + 16;
//    }

//    //private void GenerateLargeValleys(Chunk chunk, int x, int y, int blockX, int blockY)
//    //{
//    //    int groundHeight = (int)GetBlockNoise(blockX, blockY);
//    //    if (groundHeight < 1)
//    //    {
//    //        groundHeight = 1;
//    //    }

//    //    // The bottom of the world should be stone
//    //    //chunk.Blocks[x, y, 8].Type = BlockType.Lava;
//    //    //chunk.Blocks[x, y, 8].LightAmount = 255;

//    //    if (groundHeight > 127)
//    //    {
//    //        groundHeight = 126;
//    //    }
//    //    // The blocks at ground height will be Topsoil
//    //    chunk.Blocks[x, y, groundHeight].Type = BlockType.TopSoil;
//    //    chunk.Blocks[x, y, 0].Type = BlockType.Stone;

//    //    // lower ground level, doesn't have caves.
//    //    float lowerLevelNoise = (PerlinSimplexNoise.noise(blockX * 0.005f, blockY * 0.005f) * 48) +
//    //                            PerlinSimplexNoise.noise(blockX * 0.005f, blockY * 0.005f) * 8f;
//    //    lowerLevelNoise += PerlinSimplexNoise.noise(blockX * 0.05f, blockY * 0.05f) * 4f;
//    //    int lowerGroundHeight = (int)lowerLevelNoise;

//    //    for (int z = 0; z < lowerGroundHeight; z++)
//    //    {
//    //        chunk.Blocks[x, y, z].Type = BlockType.Stone;
//    //    }

//    //    chunk.Blocks[x, y, lowerGroundHeight].Type = BlockType.TopSoil;

//    //    for (int z = Chunk.DepthInBlocks - 1; z > groundHeight; z--)
//    //    {
//    //        chunk.Blocks[x, y, z].Type = BlockType.Air;
//    //        chunk.Blocks[x, y, z].LightAmount = 255;
//    //    }

//    //    // Default to sunlit.. for caves
//    //    bool sunlit = true;

//    //    for (int z = groundHeight; z > lowerGroundHeight; z--)
//    //    {
//    //        BlockType blockType;

//    //        // Since we are at or below ground height, let's see if we need to make
//    //        // a cave
//    //        float initialNoise = PerlinSimplexNoise.noise(blockX * 0.01f, blockY * 0.01f, z * 0.01f) *
//    //                             (0.015f * z) + 0.1f;
//    //        initialNoise += PerlinSimplexNoise.noise(blockX * 0.01f, blockY * 0.01f, z * 0.1f) * 0.1f + 0.1f;
//    //        initialNoise += PerlinSimplexNoise.noise(blockX * 0.1f, blockY * 0.1f, z * 0.1f) * 0.02f + 0.01f;

//    //        // Cave...so make this block air.
//    //        if (initialNoise > 0.15f)
//    //        {
//    //            blockType = BlockType.Air;
//    //        }
//    //        else
//    //        {
//    //            // We've placed a block of dirt instead...nothing below us will be sunlit
//    //            if (sunlit)
//    //            {
//    //                sunlit = false;
//    //                blockType = BlockType.TopSoil;
//    //            }
//    //            else
//    //            {
//    //                blockType = BlockType.Stone;
//    //            }
//    //        }
//    //        chunk.Blocks[x, y, z].Type = blockType;
//    //        // Is the result of the current block sunlit?
//    //        if (sunlit)
//    //        {
//    //            chunk.Blocks[x, y, z].LightAmount = 255;
//    //            chunk.ContainsLitBlocks = true;
//    //        }
//    //        else
//    //        {
//    //            chunk.Blocks[x, y, z].LightAmount = 0;
//    //        }
//    //    }

//    //    //    for (int z = Chunk.DepthInBlocks - 1; z >= 0; z--)
//    //    //    {
//    //    //        // Or we at or below ground height?
//    //    //        if (z <= groundHeight)
//    //    //        {
//    //    //            // If we are a block below ground height, 
//    //    //            // make it stone
//    //    //            BlockType blockType;
//    //    //            if (z == lowerGroundHeight)
//    //    //            {
//    //    //                blockType = BlockType.TopSoil;
//    //    //            }
//    //    //            else if (z < lowerGroundHeight)
//    //    //            {
//    //    //                blockType = BlockType.Stone;
//    //    //            }
//    //    //            else
//    //    //            {

//    //    //            }

//    //    //            chunk.Blocks[x, y, z].Type = blockType;
//    //    //        }

//    //    //    }
//    //    //if (chunk.Blocks[x, y, 1].Type == BlockType.Air)
//    //    //{
//    //    //    chunk.Blocks[x, y, 1].LightAmount = 255;
//    //    //}
//    //}
//}