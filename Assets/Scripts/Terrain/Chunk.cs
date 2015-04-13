using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

    private Block[,,] m_Blocks;

    // Chunk location in the chunk array
    private int m_ArrayX;
    private int m_ArrayY;
    private int m_ArrayZ;
    private readonly WorldData m_WorldData;

    private List<int> m_Indices = new List<int>();
    private List<Vector2> m_Uvs = new List<Vector2>();
    private List<Vector3> m_Vertices = new List<Vector3>();
    private List<Color> m_Colors = new List<Color>();
    private static int m_Id;

    // Location in Unity space
    private Vector3i m_Position;
    private System.Object m_ChunkTransform;
    private TQueue<GameObjectCreationData> m_GameObjectCreationQueue = new TQueue<GameObjectCreationData>();
    /// <summary>
    /// Some decorations only consider topsoil. Let's cache these, for quicker evaluation.
    /// </summary>
    public readonly List<Vector3i> TopSoilBlocks = new List<Vector3i>();



    public Chunk(int arrayX, int arrayY, int arrayZ, WorldData worldData)
    {
        m_WorldData = worldData;
        ArrayX = arrayX;
        ArrayY = arrayY;
        ArrayZ = arrayZ;
        m_Id++;
        Id = m_Id;
    }

    public int Id { get; set; }
    public void InitializeBlocks()
    {
        m_Blocks = new Block[WorldData.ChunkBlockWidth, WorldData.ChunkBlockHeight, WorldData.ChunkBlockDepth];
    }

    static Chunk()
    {
        WorldChunkYOffset = 0;
    }

    public static Chunk CreateChunk(Vector3i location, WorldData worldData)
    {
        Chunk chunk = new Chunk(location.X, location.Y, location.Z, worldData);
        worldData.Chunks[location.X, location.Y, location.Z] = chunk;
        chunk.InitializeBlocks();
        return chunk;
    }

    public void ReInitialize(Vector3i location)
    {
        ArrayX = location.X;
        ArrayY = location.Y;
        ArrayZ = location.Z;
        Id++;
        WorldData.Chunks[location.X, location.Y, location.Z] = this;
        InitializeBlocks();
    }

    public int ArrayX
    {
        get { return m_ArrayX; }
        set
        {
            m_ArrayX = value;
            m_Position.X = value * WorldData.ChunkBlockWidth + WorldData.MapChunkOffset.X * WorldData.ChunkBlockWidth;
        }
    }

    public int ArrayY
    {
        get { return m_ArrayY; }
        set
        {
            m_ArrayY = value;
            m_Position.Y = value * WorldData.ChunkBlockHeight + WorldData.MapChunkOffset.Y * WorldData.ChunkBlockHeight;
        }
    }

    public int ArrayZ
    {
        get { return m_ArrayZ; }
        set
        {
            m_ArrayZ = value;
            m_Position.Z = value + (WorldData.MapChunkOffset.Z * WorldData.ChunkBlockDepth);
        }
    }

    public Vector3i WorldPosition
    {
        get { return new Vector3i(m_ArrayX + WorldData.MapChunkOffset.X, m_ArrayY + WorldData.MapChunkOffset.Y, m_ArrayZ); }
    }

    public Block[,,] Blocks
    {
        get { return m_Blocks; }
        set { m_Blocks = value; }
    }

    public List<int> Indices
    {
        get { return m_Indices; }
        set { m_Indices = value; }
    }

    public List<Vector2> Uvs
    {
        get { return m_Uvs; }
        set { m_Uvs = value; }
    }

    public List<Vector3> Vertices
    {
        get { return m_Vertices; }
        set { m_Vertices = value; }
    }

    public List<Color> Colors
    {
        get { return m_Colors; }
        set { m_Colors = value; }
    }


    public static int WorldChunkYOffset { get; set; }

    public System.Object ChunkTransform
    {
        get { return m_ChunkTransform; }
        set { m_ChunkTransform = value; }
    }

    public override string ToString()
    {
        return String.Format("Chunk_{3}_{0},{1},{2}_{4}", m_ArrayX, m_ArrayY, m_ArrayZ, Id, WorldPosition);
    }

    /// <summary>
    /// Marks a chunk as needing to be relit and redrawn.
    /// </summary>
    public bool NeedsRegeneration { get; set; }

    public Vector3i Position
    {
        get { return m_Position; }
        set { m_Position = value; }
    }

    // TODO: When we make the world multiple chunks deep, this will have to handle ArrayZ values. 
    public bool IsOnTheBorder
    {
        get
        {
            return m_ArrayX == 0 || m_ArrayY == 0 || m_ArrayX == WorldData.RightChunkBorderColumn ||
                   m_ArrayY == WorldData.TopChunkBorderRow ;
        }
    }

    public bool IsInsideTheBorder
    {
        get
        {
            return m_ArrayX > 0 && m_ArrayY > 0 && m_ArrayX < WorldData.RightChunkBorderColumn &&
                   m_ArrayY < WorldData.TopChunkBorderRow;
        }
    }

    public TQueue<GameObjectCreationData> GameObjectCreationQueue
    {
        get { return m_GameObjectCreationQueue; }
        set { m_GameObjectCreationQueue = value; }
    }

    public WorldData WorldData
    {
        get { return m_WorldData; }
    }

    public void AddGameObjectCreationData(GameObjectCreationData gameObjectCreationData)
    {
        m_GameObjectCreationQueue.Enqueue(gameObjectCreationData);
    }
}