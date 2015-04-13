public interface ITerrainGenerationMethod
{
    /// <summary>
    /// Generate terrain data for a chunk.
    /// </summary>
    void GenerateTerrain(WorldData worldData, Chunk chunk);
}