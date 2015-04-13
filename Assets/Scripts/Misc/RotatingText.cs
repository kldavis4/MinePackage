using System;
using UnityEngine;

public class RotatingText : MonoBehaviour
{
    private DateTime m_StartTime;
    // Use this for initialization
    private void Start()
    {
        m_StartTime = DateTime.Now;
    }

    // Update is called once per frame
    private void Update()
    {
        //gameObject.transform.Rotate(10, 0, 0);
        //if ((DateTime.Now - m_StartTime).Seconds < 3)
        //{
        //    return;
        //}
        //if (ChunkProcessor.NumberOfChunksFinished >= WorldGameObject.WidthInChunks*WorldGameObject.HeightInChunks*WorldGameObject.DepthInChunks)
        //{
        //    Destroy(this);
        //}
    }
}