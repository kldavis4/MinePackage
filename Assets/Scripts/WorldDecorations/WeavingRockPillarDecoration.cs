using UnityEngine;

public class WeavingRockPillarDecoration : Decoration
{
    private readonly WorldData m_WorldData;
    private static readonly Vector3i m_BlockSize = new Vector3i(9, 9, 30);

    public WeavingRockPillarDecoration(WorldData worldData)
        : base(worldData)
    {
        m_WorldData = worldData;
    }

    public override Vector3i BlockSize
    {
        get { return m_BlockSize; }
    }

    public override bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random)
    {
        if (IsAValidLocationforDecoration(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random))
        {
            CreateDecorationAt(chunk, localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random);
            return true;
        }

        return false;
    }

    private void CreateDecorationAt(Chunk chunk, int blockX, int blockY, int blockZ, IRandom random)
    {
        int offsetX = blockX;
        int offsetY = blockY;
        int numberOfVerticalSegments = BlockSize.Z / 5;
        int diskZ = blockZ;
        int radius = 5;
        BlockType blockType = BlockType.Stone;
        for (int seg = 0; seg < numberOfVerticalSegments; seg++)
        {
            for (int disc = 0; disc < 5; disc++)
            {
                CreateDiskAt(offsetX, offsetY, diskZ, radius, blockType);
                diskZ++;
            }
            if (radius > 1)
            {
                radius--;
                if (radius == 1)
                {
                    blockType = BlockType.Dirt;
                }
            }
        }

        AddGameObjectDecorationToWorld("gray diamond", chunk, new Vector3(blockX + 0.5f, blockY + 0.5f, diskZ + 0.1f),
                                       new Vector3(0, -90, 0));
    }


    private bool IsAValidLocationforDecoration(int blockX, int blockY, int blockZ, IRandom random)
    {
        if (random.RandomRange(1, 1000) < 999)
        {
            return false;
        }

        return IsLocationLowEnough(blockZ) && TheSpaceHereIsEmpty(blockX, blockY, blockZ);
    }
}