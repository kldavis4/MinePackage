using UnityEngine;

public class PineTreeDecorator : IDecoration
{
    private readonly WorldData m_WorldData;

    public PineTreeDecorator(WorldData worldData)
    {
        m_WorldData = worldData;
    }


    public bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random)
    {
        if (IsAValidLocationforDecoration(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random))
        {
            CreateDecorationAt(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Determines if a tree decoration even wants to be at this location.
    /// </summary>
    /// <param name="blockX"></param>
    /// <param name="blockY"></param>
    /// <param name="blockZ"></param>
    /// <param name="random"></param>
    /// <returns></returns>
    private bool IsAValidLocationforDecoration(int blockX, int blockY, int blockZ, IRandom random)
    {
        // We don't want TOO many trees...make it a 1% chance to be drawn there.
        if (random.RandomRange(1, 1000) < 999)
        {
            return false;
        }

        // Trees don't like to grow too high
        if (blockZ >= m_WorldData.DepthInBlocks - 20)
        {
            return false;
        }

        // Trees like to have a minimum amount of space to grow in.
        return SpaceAboveIsEmpty(blockX, blockY, blockZ, 8, 2, 2);
    }

    private void CreateDecorationAt(int blockX, int blockY, int blockZ, IRandom random)
    {
        //PINE TREES
        int trunkLength = random.RandomRange(10, 20);
        // Trunk
        for (int z = blockZ + 1; z <= blockZ + trunkLength; z++)
        {
            CreateTrunkAt(blockX, blockY, z);
        }
        //leaves
        for (int z = blockZ + 3; z <= blockZ + trunkLength; z += 2)
        {
            CreateDiskAt(blockX, blockY, z, (blockZ + trunkLength - z) / 3);
        }
        CreateLeavesAt(blockX, blockY, blockZ + trunkLength);

    }

    /// <summary>
    /// Creates the tree canopy...a ball of leaves.
    /// </summary>
    /// <param name="blockX"></param>
    /// <param name="blockY"></param>
    /// <param name="blockZ"></param>
    /// <param name="radius"></param>
    private void CreateDiskAt(int blockX, int blockY, int blockZ, int radius)
    {
        for (int x = blockX - radius; x <= blockX + radius; x++)
        {
            for (int y = blockY - radius; y <= blockY + radius; y++)
            {
                if (Vector3.Distance(new Vector3(blockX, blockY, blockZ), new Vector3(x, y, blockZ)) <= radius)
                {
                    m_WorldData.SetBlockType(x, y, blockZ, BlockType.Leaves);
                }
            }
        }
    }

    private void CreateTrunkAt(int blockX, int blockY, int z)
    {
        m_WorldData.SetBlockType(blockX, blockY, z, BlockType.Dirt);
    }

    private void CreateLeavesAt(int blockX, int blockY, int z)
    {
        m_WorldData.SetBlockType(blockX, blockY, z, BlockType.Leaves);
    }

    private bool SpaceAboveIsEmpty(int blockX, int blockY, int blockZ, int depthAbove, int widthAround, int heightAround)
    {
        for (int z = blockZ + 1; z <= blockZ + depthAbove; z++)
        {
            for (int x = blockX - widthAround; x <= blockX + widthAround; x++)
            {
                for (int y = blockY - heightAround; y < blockY + heightAround; y++)
                {
                    if (m_WorldData.GetBlock(x, y, z).Type != BlockType.Air)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public override string ToString()
    {
        return "Pine Tree";
    }
}