using UnityEngine;

/// <summary>
/// Contains Unity GameObject creation data
/// </summary>
public class GameObjectCreationData
{
    private readonly string m_Name;
    private readonly Vector3 m_Position;
    private readonly Vector3 m_Rotation;
    private readonly WorldData m_WorldData;

    public GameObjectCreationData(string name, Vector3 position, Vector3 rotation, WorldData worldData )
    {
        m_Name = name;
        m_Position = position;
        m_Rotation = rotation;
        m_WorldData = worldData;
        GlobalUnityPosition = new Vector3(position.x + worldData.MapBlockOffset.X,position.z + worldData.MapBlockOffset.Z, position.y + worldData.MapBlockOffset.Y);
    }

    public string Name
    {
        get { return m_Name; }
    }

    public Vector3 Position
    {
        get { return m_Position; }
    }

    public Vector3 Rotation
    {
        get { return m_Rotation; }
    }

    public WorldData WorldData
    {
        get { return m_WorldData; }
    }

    public Vector3 GlobalUnityPosition { get; set; }
}