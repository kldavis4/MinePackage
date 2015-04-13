using UnityEngine;

/// <summary>
/// Base class for decorations
/// </summary>
public abstract class Decoration : IDecoration
{
    private readonly WorldData m_WorldData;

    public Decoration(WorldData worldData)
    {
        m_WorldData = worldData;
    }

    public WorldData WorldData
    {
        get { return m_WorldData; }
    }

    public abstract bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random);

    /// <summary>
    /// Creates the tree canopy...a ball of leaves.
    /// </summary>
    /// <param name="blockX"></param>
    /// <param name="blockY"></param>
    /// <param name="blockZ"></param>
    /// <param name="radius"></param>
    protected void CreateSphereAt(int blockX, int blockY, int blockZ, int radius)
    {
        for (int x = blockX - radius; x <= blockX + radius; x++)
        {
            for (int y = blockY - radius; y <= blockY + radius; y++)
            {
                for (int z = blockZ - radius; z <= blockZ + radius; z++)
                {
                    if (Vector3.Distance(new Vector3(blockX, blockY, blockZ), new Vector3(x, y, z)) <= radius)
                    {
                        WorldData.SetBlockType(x, y, z, BlockType.Leaves);
                    }
                }
            }
        }
    }

    public void CreateColumnAt(int blockX, int blockY, int blockZ, int columnLength, BlockType blockType)
    {
        // Trunk
        for (int z = blockZ + 1; z <= blockZ + columnLength; z++)
        {
            CreateColumnAt(blockX, blockY, z, blockType);
        }
    }

    private void CreateColumnAt(int blockX, int blockY, int z, BlockType blockType)
    {
        WorldData.SetBlockType(blockX, blockY, z, blockType);
    }

    protected bool TheSpaceHereIsEmpty(int blockX, int blockY, int blockZ)
    {
        Vector3i blockSize = BlockSize;
        for (int z = blockZ + 1; z <= blockZ + blockSize.Z; z++)
        {
            for (int x = blockX - blockSize.X / 2; x <= blockX + blockSize.X / 2; x++)
            {
                for (int y = blockY - blockSize.Y / 2; y < blockY + blockSize.Y / 2; y++)
                {
                    if (WorldData.GetBlock(x, y, z).Type != BlockType.Air)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    protected bool IsLocationLowEnough(int blockZ)
    {
        return blockZ < m_WorldData.DepthInBlocks - BlockSize.Z;
    }

    public abstract Vector3i BlockSize { get; }

    protected void CreateDiskAt(int blockX, int blockY, int blockZ, int radius, BlockType blockType)
    {
        for (int x = blockX - radius; x <= blockX + radius; x++)
        {
            for (int y = blockY - radius; y <= blockY + radius; y++)
            {
                if (Vector3.Distance(new Vector3(blockX, blockY, blockZ), new Vector3(x, y, blockZ)) <= radius)
                {
                    m_WorldData.SetBlockType(x, y, blockZ, blockType);
                }
            }
        }
    }

    protected bool IsASolidDiskAreaAt(int blockX, int blockY, int blockZ, int radius)
    {
        for (int x = blockX - radius; x <= blockX + radius; x++)
        {
            for (int y = blockY - radius; y <= blockY + radius; y++)
            {
                if (Vector3.Distance(new Vector3(blockX, blockY, blockZ), new Vector3(x, y, blockZ)) <= radius)
                {
                    if (m_WorldData.GetBlock(x, y, blockZ).Type == BlockType.Air)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public void AddGameObjectDecorationToWorld(string name, Chunk chunk, Vector3 localMapPosition, Vector3 rotation)
    {
        chunk.AddGameObjectCreationData(new GameObjectCreationData(name,
                                                                   new Vector3(localMapPosition.x, localMapPosition.y, localMapPosition.z),
                                                                   rotation, m_WorldData));
    }

    public void AddGameObjectDecorationToWorld(Chunk chunk, Vector3 position, Vector3 rotation)
    {
        AddGameObjectDecorationToWorld(ToString(), chunk, position, rotation);
    }
}