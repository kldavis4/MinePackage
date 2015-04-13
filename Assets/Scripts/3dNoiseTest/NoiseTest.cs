using UnityEngine;

public class NoiseTest : MonoBehaviour
{
    private ParticleEmitter m_ParticleEmitter;
    // Use this for initialization
    private void Start()
    {
        m_ParticleEmitter = gameObject.GetComponent<ParticleEmitter>();
    }

    private bool m_FirstTime = true;
    // Update is called once per frame
    private void Update()
    {
        Debug.Log(m_ParticleEmitter.particleCount);

        int count = 0;
        Particle[] particles = m_ParticleEmitter.particles;
        for (int x = 0; x < 1024; x++)
        {
            for (int z = 0; z < 1024; z++)
            {
                for (int y = 0; y < 128; y++)
                {
                    float groundHeight = PerlinSimplexNoise.noise(x*0.001f, z*0.001f, y*0.001f)*64.0f;
                    groundHeight += PerlinSimplexNoise.noise(x*0.01f, z*0.01f, y*0.01f)*32.0f;
                    groundHeight += PerlinSimplexNoise.noise(x*0.1f, z*0.1f, y*0.1f)*4.0f;
                    //groundHeight = Mathf.Clamp(groundHeight, 0, 127);

                    particles[count].position = new Vector3(x + y, groundHeight, z);
                    if (count < particles.Length -1)
                    {
                        count++;
                    }
                    else
                    {
                        UpdateDisplay(particles);
                        return;
                    }
                }
            }
        }
    }

    private void UpdateDisplay(Particle[] particles)
    {
        m_ParticleEmitter.particles = particles;
    }
}