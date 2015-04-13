using UnityEngine;

public class TikiStatueDecoration : Decoration
{
    public TikiStatueDecoration(WorldData worldData) : base(worldData)
    {
    }

    public override bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random)
    {
        if (IsAValidLocationforDecoration(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random))
        {
            CreateGameObjectDecorationAt(chunk, localBlockPosition.X, localBlockPosition.Y - 1, localBlockPosition.Z,
                                         random);
            return true;
        }

        return false;
    }


    private void CreateGameObjectDecorationAt(Chunk chunk, int blockX, int blockY, int blockZ, IRandom random)
    {
        AddGameObjectDecorationToWorld(chunk, new Vector3(blockX + 0.5f, blockY + 0.5f, blockZ), Vector3.zero);
    }

    private bool IsAValidLocationforDecoration(int blockX, int blockY, int blockZ, IRandom random)
    {
        if (random.RandomRange(0, 10000) < 9995)
        {
            return false;
        }

        if (!TheSpaceHereIsEmpty(blockX, blockY, blockZ - 1))
        {
            return false;
        }

        return true;
    }

    public override Vector3i BlockSize
    {
        get { return new Vector3i(1, 1, 5); }
    }

    public override string ToString()
    {
        return "TikiStatue";
    }
}