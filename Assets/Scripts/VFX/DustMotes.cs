using UnityEngine;

/// <summary>
/// Slow dust in lamp light. Call DustMotes.Spawn(parent, position) or add this component.
/// </summary>
public class DustMotes : MonoBehaviour
{
    [SerializeField] Color moteColor = new Color(1f, 0.92f, 0.75f, 0.35f);
    [SerializeField] float radius = 1.4f;

    void Awake()
    {
        Build(transform, Vector3.zero, moteColor, radius);
    }

    public static ParticleSystem Spawn(Transform parent, Vector3 localPosition)
    {
        GameObject go = new GameObject("DustMotes");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        DustMotes motes = go.AddComponent<DustMotes>();
        motes.enabled = false;
        return Build(go.transform, Vector3.zero, motes.moteColor, motes.radius);
    }

    static ParticleSystem Build(Transform host, Vector3 localPosition, Color color, float radius)
    {
        ParticleSystem existing = host.GetComponent<ParticleSystem>();
        if (existing != null)
        {
            return existing;
        }

        host.localPosition = host.localPosition + localPosition;
        ParticleSystem ps = host.gameObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.startLifetime = 8f;
        main.startSpeed = 0.04f;
        main.startSize = 0.015f;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 40;
        main.playOnAwake = true;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 4f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.05f);

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
                renderer.material.SetColor("_BaseColor", color);
            }

            if (renderer.material.HasProperty("_Surface"))
            {
                renderer.material.SetFloat("_Surface", 1f);
            }
        }

        return ps;
    }
}
