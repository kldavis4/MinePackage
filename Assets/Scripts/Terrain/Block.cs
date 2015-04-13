
using UnityEngine;

public enum BlockType : byte
{
    TopSoil=0,
    Dirt=1,
    Light = 2,
    Lava = 3,
    Leaves=4,
    Stone = 5,
    Air = 255

}

public enum BlockFace : byte
{
    Top = 0,
    Side = 1,
    Bottom = 2
}
/// <summary>
/// The data we store for each block
/// </summary>
public struct Block
{
    public BlockType Type;
    public byte LightAmount;
}

public class BlockUVCoordinates
{
    private readonly Rect[] m_BlockFaceUvCoordinates = new Rect[3];

    public BlockUVCoordinates(Rect topUvCoordinates, Rect sideUvCoordinates, Rect bottomUvCoordinates)
    {
        BlockFaceUvCoordinates[(int)BlockFace.Top] = topUvCoordinates;
        BlockFaceUvCoordinates[(int)BlockFace.Side] = sideUvCoordinates;
        BlockFaceUvCoordinates[(int)BlockFace.Bottom] = bottomUvCoordinates;
    }


    public Rect[] BlockFaceUvCoordinates
    {
        get { return m_BlockFaceUvCoordinates; }
    }
}