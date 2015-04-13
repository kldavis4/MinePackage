using System.Collections.Generic;

public enum BatchType
{
    TerrainGeneration,
    Decoration,
    Lighting,
    MeshDataCreation
}
public class ChunkBatch
{
    private readonly List<Chunk> m_Chunks;
    private readonly BatchType m_BatchType;
    private bool m_ProcessingComplete;

    public ChunkBatch(List<Chunk> chunks, BatchType batchType )
    {
        m_Chunks = chunks;
        m_BatchType = batchType;
    }

    public List<Chunk> Chunks
    {
        get { return m_Chunks; }
    }

    public BatchType BatchType
    {
        get { return m_BatchType; }
    }

    public bool ProcessingComplete
    {
        get { return m_ProcessingComplete; }
        set { m_ProcessingComplete = value; }
    }
}