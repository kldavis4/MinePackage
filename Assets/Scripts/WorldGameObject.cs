using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class WorldGameObject : MonoBehaviour
{
    private WorldData m_WorldData;
    private World m_World;
    private List<IDecoration> m_WorldDecorations = new List<IDecoration>();
    private Vector3i m_PlayerChunkPosition;


    // our blueprint for terrain chunks, in the Unity Hierarchy
    public Transform Chunk_Prefab;

    // All possible block textures
    public Texture2D[] World_Textures;
    // Our texture atlas
    public Texture2D WorldTextureAtlas;

    /// <summary>
    /// Our Unity prefabs
    /// </summary>
    public List<Transform> DecoratorPrefabs = new List<Transform>();


    public Transform Player;
    public Transform Sparks;

    private Transform m_ChunksParent;
    private ChunkProcessor m_ChunkProcessor;
    private ChunkMover m_ChunkMover;
    private readonly TQueue<GameObjectCreationData> m_GameObjectCreationQueue = new TQueue<GameObjectCreationData>();

    /// <summary>
    /// Our unity prefabs, by their name
    /// </summary>
    private readonly Dictionary<string, Transform> m_PrefabsByName = new Dictionary<string, Transform>();

    public WorldData WorldData
    {
        get { return m_WorldData; }
    }

    private void Start()
    {
        InitializeDecoratorPrefabs();
        m_ChunkProcessor = new ChunkProcessor();
        m_WorldData = new WorldData(m_ChunkProcessor);
        m_ChunksParent = transform.FindChild("Chunks");
        m_ChunkMover = new ChunkMover(m_WorldData, m_ChunkProcessor);

        // Only the dual layer terrain w/ medium valleys and standard terrain medium caves
        // currently work, I haven't updated the others to return sunlit blocks.
        BatchPoolProcessor<Chunk> batchProcessor = new BatchPoolProcessor<Chunk>();
        WorldDecorator worldDecorator = new WorldDecorator(WorldData, batchProcessor,
                                                           m_ChunkProcessor );
        m_World = new World(WorldData,
                            new TerrainGenerator(WorldData, m_ChunkProcessor, batchProcessor,
                                                 new DualLayerTerrainWithMediumValleys()),
                            new LightProcessor(batchProcessor, WorldData, m_ChunkProcessor),
                            new MeshDataGenerator(batchProcessor, WorldData, m_ChunkProcessor),
                            worldDecorator, m_ChunkProcessor);
        

        m_World.InitializeGridChunks();

        InitializeTextures();
        Player.transform.position = new Vector3(WorldData.WidthInBlocks / 2, 120, WorldData.HeightInBlocks / 2);
        CreateWorldChunkPrefabs();
        m_World.StartProcessingThread();
        m_PlayerChunkPosition = CurrentPlayerChunkPosition();
    }

    private void InitializeDecoratorPrefabs()
    {
        foreach (Transform decoratorPrefab in DecoratorPrefabs)
        {
            m_PrefabsByName.Add(decoratorPrefab.name, decoratorPrefab);
        }
    }

    private DateTime startTime;

    private void Update()
    {
        ProcessPlayerInput();
        DisplayDiggings();
        CheckForWorldMove();
        CreatePrefabsFromFinishedChunks();
        RemoveAnyChunksThatAreOffTheMap();
    }

    /// <summary>
    /// If the player has moved into a different chunk, we need to generate
    /// new world terrain
    /// </summary>
    private void CheckForWorldMove()
    {
        Vector3i newPlayerChunkPosition = CurrentPlayerChunkPosition();
        if (newPlayerChunkPosition != m_PlayerChunkPosition)
        {
            Vector3i direction = (m_PlayerChunkPosition - newPlayerChunkPosition);
            m_PlayerChunkPosition = newPlayerChunkPosition;
            m_ChunkMover.ShiftWorldChunks(direction);
        }
    }

    /// <summary>
    /// The current chunk that the player is in.
    /// </summary>
    /// <returns></returns>
    private Vector3i CurrentPlayerChunkPosition()
    {
        return new Vector3i((int)Player.position.x/m_WorldData.ChunkBlockWidth, (int)Player.position.z/m_WorldData.ChunkBlockHeight, 0);
    }


    private void RemoveAnyChunksThatAreOffTheMap()
    {
        //if (m_ChunkProcessor.PrefabRemovalQueue.Count > 0) 
        //{
        //    Chunk chunkToRemove = m_ChunkProcessor.PrefabRemovalQueue.Dequeue();
        //    Transform chunkTransform = (Transform)chunkToRemove.ChunkTransform;
        //    ChunkGameObjectScript chunkGameObjectScriptScript = chunkTransform.GetComponent<ChunkGameObjectScript>();
        //    chunkGameObjectScriptScript.Destroy();
        //}
    }


    private void InitializeTextures()
    {
        WorldTextureAtlas = new Texture2D(2048, 2048, TextureFormat.ARGB32, false );
        WorldData.WorldTextureAtlasUvs = WorldTextureAtlas.PackTextures(World_Textures, 0);
        WorldTextureAtlas.filterMode = FilterMode.Point;
        WorldTextureAtlas.anisoLevel = 9;
        WorldTextureAtlas.Apply();

        WorldData.GenerateUVCoordinatesForAllBlocks();
    }

    private void ProcessPlayerInput()
    {
        // Exit if we haven't received any input
        if (!Input.anyKey)
        {
            return;
        }

        if (Input.inputString.Contains("t"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 4.0f))
            {
                WorldData.SetBlockLightWithRegeneration((int) hit.point.x, (int) hit.point.z, (int) hit.point.y, 255);
                m_World.RegenerateChunks();
            }
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            //m_World.RemoveBlockAt(blockHitPoint);
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
            if (Physics.Raycast(ray, out hit, 4.0f))
            {
                // Get the hit position...plus a little more. 
                Vector3 worldHitPosition = hit.point + (ray.direction.normalized * 0.01f);
                // We have the 'global' block position, we need to convert that to the local map position.
                Vector3i blockMapPosition = m_World.GlobalToLocalMapBlockPosition(new Vector3i((int) worldHitPosition.x, (int) worldHitPosition.z, (int) worldHitPosition.y));
                m_World.Dig(blockMapPosition, worldHitPosition);
            }
        }

    }


    private bool m_ProcessAllChunksAtOnce;

    private DateTime m_LastChunkGameObjectCreationTime = DateTime.Now;
    private void CreatePrefabsFromFinishedChunks()
    {
        while (m_ChunkProcessor.MeshCreationQueue.Count > 0)
        {
            if (!m_PlayerIsActivated)
            {
                ActivateThePlayer();
            }

            // Don't freeze everything by drawing them every frame
            if (m_LastChunkGameObjectCreationTime + TimeSpan.FromSeconds(0.001) > DateTime.Now)
            {
                return;
            }
            m_LastChunkGameObjectCreationTime = DateTime.Now;
            Chunk chunk = m_ChunkProcessor.MeshCreationQueue.Dequeue();

            if (chunk == null)
            {
                return;
            }

            if (chunk.ChunkTransform == null)
            {
                CreatePrefabForChunk(chunk);
            }

            Transform chunkTransform = (Transform) chunk.ChunkTransform;
            ChunkGameObjectScript chunkGameObjectScriptScript = chunkTransform.GetComponent<ChunkGameObjectScript>();
            ((IPrefab) chunkGameObjectScriptScript).World = m_World;

            chunkGameObjectScriptScript.CreateFromChunk(chunk, m_PrefabsByName);

            if (!m_ProcessAllChunksAtOnce)
            {
                return;
            }
        }
    }

    private void ActivateThePlayer()
    {
        // If the world is not ready...no playing yet.
        if (!m_WorldData.WorldIsReady)
        {
            return;
        }

        m_PlayerIsActivated = true;
        Player.GetComponentInChildren<CharacterMotor>().enabled = true;
        m_PlayerChunkPosition = CurrentPlayerChunkPosition();
        GameObject.Find("GeneratingWorld").active = false;
    }


    private bool m_PlayerIsActivated;


    /// <summary>
    /// When we quit the app, be sure to shut down the threads and
    /// destory our chunks
    /// </summary>
    private void OnApplicationQuit()
    {
        Debug.Log("Exiting!");
        m_World.ContinueProcessingChunks = false;
        for (int x = 0; x < WorldData.ChunksWide; x++)
        {
            for (int y = 0; y < WorldData.ChunksHigh; y++)
            {
                World.DestroyChunk(WorldData.Chunks[x, y, 0]);
            }
        }
        m_World = null;
    }

    private void DisplayDiggings()
    {
        if (m_World.Diggings.Count == 0)
        {
            return;
        }
        Vector3 diggingsLocation = m_World.Diggings.Dequeue();
        Debug.Log("Diggings at " + diggingsLocation);
        Instantiate(Sparks, diggingsLocation, Quaternion.identity);
    }

    private void CreateWorldChunkPrefabs()
    {
        for (int x = 0; x < WorldData.ChunksWide; x++)
        {
            for (int y = 0; y < WorldData.ChunksHigh; y++)
            {
                for (int z = 0; z < WorldData.ChunksDeep; z++)
                {
                    Chunk chunk = WorldData.Chunks[x, y, z];
                    CreatePrefabForChunk(chunk);
                }
            }
        }
    }

    private void CreatePrefabForChunk(Chunk chunk)
    {
        Vector3 chunkGameObjectPosition =
            new Vector3(chunk.Position.X, chunk.Position.Z, chunk.Position.Y);
        Transform chunkGameObject =
            Instantiate(Chunk_Prefab, chunkGameObjectPosition, Quaternion.identity) as Transform;
        chunkGameObject.parent = m_ChunksParent;
        chunkGameObject.name = chunk.ToString();
        //m_ChunkGameObjects[x, y, z] = chunkGameObject;
        ChunkGameObjectScript chunkGameObjectScriptScript = chunkGameObject.GetComponent<ChunkGameObjectScript>();
        chunkGameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = WorldTextureAtlas;

        //chunkGameObjectScriptScript.Texture = WorldTextureAtlas;
        chunk.ChunkTransform = chunkGameObject;
    }
}