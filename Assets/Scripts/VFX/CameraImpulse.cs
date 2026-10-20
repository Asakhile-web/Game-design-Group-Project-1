using UnityEngine;

/// <summary>
/// Light camera shake. Runs after HeadBob ([DefaultExecutionOrder(100)]).
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraImpulse : MonoBehaviour
{
    static CameraImpulse instance;

    float trauma;

    public static void Shake(float amount = 0.15f, float durationTrauma = 0.25f)
    {
        if (instance == null)
        {
            return;
        }

        instance.trauma = Mathf.Clamp01(instance.trauma + amount);
        instance.trauma = Mathf.Max(instance.trauma, durationTrauma);
    }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void LateUpdate()
    {
        if (trauma <= 0.001f)
        {
            return;
        }

        float shake = trauma * trauma;
        transform.localPosition += new Vector3(
            (Random.value * 2f - 1f) * 0.04f * shake,
            (Random.value * 2f - 1f) * 0.04f * shake,
            0f);
        trauma = Mathf.MoveTowards(trauma, 0f, Time.deltaTime * 1.8f);
    }
}
