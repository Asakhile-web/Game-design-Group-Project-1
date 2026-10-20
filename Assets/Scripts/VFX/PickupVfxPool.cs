using UnityEngine;

/// <summary>
/// Pooled sparkle bursts for item pickups. No Instantiate per pickup after warm-up.
/// </summary>
public class PickupVfxPool : MonoBehaviour
{
    [SerializeField] int poolSize = 8;

    static PickupVfxPool instance;
    ParticleSystem[] pool;
    int index;

    public static PickupVfxPool Instance => instance;

    void Awake()
    {
        instance = this;
        pool = new ParticleSystem[Mathf.Max(2, poolSize)];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = CreateBurst("Sparkle" + i);
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Play(Vector3 worldPosition)
    {
        if (instance == null)
        {
            return;
        }

        instance.PlayAt(worldPosition);
    }

    void PlayAt(Vector3 worldPosition)
    {
        ParticleSystem ps = pool[index];
        index = (index + 1) % pool.Length;
        ps.transform.position = worldPosition;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }

    ParticleSystem CreateBurst(string objectName)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(transform, false);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = 0.45f;
        main.startSpeed = 1.4f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.98f, 0.86f, 0.45f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 24;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 16) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader != null)
        {
            renderer.material = new Material(shader);
            if (renderer.material.HasProperty("_BaseColor"))
            {
                renderer.material.SetColor("_BaseColor", new Color(0.98f, 0.86f, 0.45f, 1f));
            }
        }

        return ps;
    }
}
