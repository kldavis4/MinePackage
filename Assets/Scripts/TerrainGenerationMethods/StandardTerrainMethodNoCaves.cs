//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;


//public class StandardTerrainMethodNoCaves : ITerrainGenerationMethod
//    {
//    public void GenerateTerrain(WorldData worldData, Chunk chunk, int blockWorldXOffset)
//        {
//            for (int x = 0; x < worldData.ChunkBlockWidth; x++)
//            {
//                int blockX = chunk.ArrayX * worldData.ChunkBlockWidth + x + blockWorldXOffset;

//                for (int y = 0; y < worldData.ChunkBlockHeight; y++)
//                {
//                    int blockY = chunk.ArrayY * worldData.ChunkBlockHeight + y;
//                    //GenerateLargeValleys(chunk, x, y, blockX, blockY);
//                    GenerateTerrain(chunk, x, y, blockX, blockY, worldData.DepthInBlocks);
//                }
//            }
//        }

//        private static void GenerateTerrain(Chunk chunk, int blockXInChunk, int blockYInChunk, int blockX, int blockY, int worldDepthInBlocks)
//        {
//            // The lower ground level is at least this high.
//            int minimumGroundheight = worldDepthInBlocks / 4;
//            int minimumGroundDepth =(int)(worldDepthInBlocks * 0.75f);

//            float octave1 = PerlinSimplexNoise.noise(blockX * 0.0001f, blockY * 0.0001f) * 0.5f;
//            float octave2 = PerlinSimplexNoise.noise(blockX * 0.0005f, blockY * 0.0005f) * 0.25f;
//            float octave3 = PerlinSimplexNoise.noise(blockX * 0.005f, blockY * 0.005f) * 0.12f;
//            float octave4 = PerlinSimplexNoise.noise(blockX * 0.01f, blockY * 0.01f) * 0.12f;
//            float octave5 = PerlinSimplexNoise.noise(blockX * 0.03f, blockY * 0.03f) * octave4;
//            float lowerGroundHeight = octave1 + octave2 + octave3 + octave4 + octave5;
//            lowerGroundHeight = lowerGroundHeight * minimumGroundDepth + minimumGroundheight;
//            bool sunlit = true;
//            BlockType blockType = BlockType.Air;
//            for (int z = worldDepthInBlocks - 1; z >= 0; z--)
//            {

//                if (z <= lowerGroundHeight)
//                {
//                    if (sunlit)
//                    {
//                        blockType = BlockType.TopSoil;
//                        sunlit = false;
//                    }
//                    else 
//                    {
//                        blockType = BlockType.Dirt;
//                    }
//                }
                
//                chunk.Blocks[blockXInChunk, blockYInChunk, z].Type = blockType;
//            }
//        }
//    }

