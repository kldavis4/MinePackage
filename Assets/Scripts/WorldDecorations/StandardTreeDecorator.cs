public class StandardTreeDecorator : Decoration
{
    public StandardTreeDecorator(WorldData worldData)
        : base(worldData)
    {
    }

    public override bool Decorate(Chunk chunk, Vector3i localBlockPosition, IRandom random)
    {
        if (IsAValidLocationforDecoration(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random))
        {
            CreateDecorationAt(localBlockPosition.X, localBlockPosition.Y, localBlockPosition.Z, random);
            return true;
        }

        return false;
    }

    /// <summary>
    /// How much space this decoration takes up.
    /// </summary>
    public override Vector3i BlockSize
    {
        get { return new Vector3i(4, 4, 8); }
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

        if (!IsLocationLowEnough(blockZ))
        {
            return false;
        }

        // Trees like to have a minimum amount of space to grow in.
        return TheSpaceHereIsEmpty(blockX, blockY, blockZ);
    }

    private void CreateDecorationAt(int blockX, int blockY, int blockZ, IRandom random)
    {
        int trunkLength = random.RandomRange(6, 10);
        // Trunk
        CreateColumnAt(blockX, blockY, blockZ, trunkLength, BlockType.Dirt);

        // Leaves
        CreateSphereAt(blockX, blockY, blockZ + trunkLength, random.RandomRange(3, 4));
    }


    public override string ToString()
    {
        return "Standard Tree";
    }
}