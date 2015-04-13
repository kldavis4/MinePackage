using System;

public struct Vector3i
{
    private int m_X;
    private int m_Y;
    private int m_Z;
    public Vector3i(int x, int y, int z)
    {
        m_X = x;
        m_Y = y;
        m_Z = z;
    }

    public int X
    {
        get { return m_X; }
        set { m_X = value; }
    }

    public int Y
    {
        get { return m_Y; }
        set { m_Y = value; }
    }

    public int Z
    {
        get { return m_Z; }
        set { m_Z = value; }
    }

    public override bool Equals(object obj)
    {
        Vector3i other = (Vector3i)obj;
        return (other.X == m_X && other.Y == m_Y && other.Z == m_Z);
    }

    public static bool operator ==(Vector3i one, Vector3i other)
    {
        return (one.X == other.X) && (one.Y == other.Y) && (one.Z == other.Z);
    }

    public static bool operator !=(Vector3i one, Vector3i other)
    {
        return !(one == other);
    }

    public static Vector3i operator -(Vector3i one, Vector3i other)
    {
        return new Vector3i(one.X - other.X, one.Y - other.Y, one.Z - other.Z);
    }

    public override string ToString()
    {
        return string.Format("{0}, {1}, {2}", m_X, m_Y, m_Z);
    }
}