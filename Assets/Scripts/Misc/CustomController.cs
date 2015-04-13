using System;
using UnityEngine;

public class CustomController : MonoBehaviour
{
    private Vector3 m_Gravity = new Vector3(0,-0.98f, 0);
    private float m_BoxWidth = 0.5f;
    private Vector3 m_Velocity;
    private Vector3 m_Forward;
    public float Speed = 0.1f;
    public WorldGameObject World;
    private WorldData m_WorldData;

    private void Start()
    {
        m_WorldData = World.WorldData;
    }
    private void Update()
    {
        Vector3 position = transform.position;
        m_Velocity = Vector3.zero;
        m_Forward = transform.forward;

        if (Input.inputString.Contains("w"))
        {
            m_Velocity = m_Forward;
        }
        else if (Input.inputString.Contains("s"))
        {
            m_Velocity = -m_Forward;
        }

        m_Velocity += m_Gravity;
        float newVelocity;
        //if (CollidesWithX(position, m_Velocity.x, out newVelocity))
        //{
        //    m_Velocity.x = newVelocity;
        //}

        if (CollidesWithY(position, m_Velocity.y, out newVelocity))
        {
            m_Velocity.y = newVelocity;
        }

        transform.position += m_Velocity;

    }

    private bool CollidesWithX(Vector3 position, float movement, out float newVelocity)
    {
        float adjustedSpeed = Speed * Math.Sign(movement);

        Vector3 newPosition = new Vector3(position.x + adjustedSpeed + m_BoxWidth, position.y, position.z);
        Block block = m_WorldData.GetBlock((int)newPosition.x, (int)newPosition.y, (int)newPosition.z);

        if (block.Type != BlockType.Air)
        {
            newVelocity = 0;
            return true;
        }

        newVelocity = movement;
        return false;
    }

    private bool CollidesWithY(Vector3 position, float movement, out float newVelocity)
    {
        float adjustedSpeed = Speed * Math.Sign(movement);

        Vector3 newPosition = new Vector3(position.x, position.y + adjustedSpeed + m_BoxWidth, position.z);
        Debug.Log("Checking " + newPosition.x + ", " + newPosition.z + ", " + newPosition.y);
        Block block = m_WorldData.GetBlock((int)newPosition.x, (int)newPosition.z, (int)newPosition.y);

        if (block.Type != BlockType.Air)
        {
            Debug.Log("Block " + block.Type + " hit at " + newPosition.x + ", " + newPosition.z + ", " + newPosition.y);
            newVelocity = 0;
            return true;
        }

        newVelocity = movement;
        return false;
    }
}