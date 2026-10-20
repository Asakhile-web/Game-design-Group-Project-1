using UnityEngine;

/// <summary>
/// Randomised intensity flicker for desk lamps. Caches the Light in Awake.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField] float minIntensity = 1.6f;
    [SerializeField] float maxIntensity = 2.4f;
    [SerializeField] float speed = 12f;
    [SerializeField] float smoothness = 8f;

    Light lamp;
    float target;
    float current;

    void Awake()
    {
        lamp = GetComponent<Light>();
        if (lamp == null)
        {
            Debug.LogWarning("[LightFlicker] Missing Light on " + name + ".", this);
            enabled = false;
            return;
        }

        current = lamp.intensity;
        target = current;
    }

    void Update()
    {
        if (lamp == null)
        {
            return;
        }

        if (Random.value < speed * Time.deltaTime * 0.05f)
        {
            target = Random.Range(minIntensity, maxIntensity);
        }

        current = Mathf.MoveTowards(current, target, smoothness * Time.deltaTime);
        lamp.intensity = current;
    }
}
