using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Unscaled black fade between scenes. Survives LoadScene. Create once (SystemsBootstrap / MainMenu).
/// </summary>
public class SceneFader : MonoBehaviour
{
    [SerializeField] float fadeSeconds = 0.45f;

    static SceneFader instance;
    CanvasGroup group;
    bool busy;

    public static SceneFader Instance => instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Build();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void Load(string sceneName)
    {
        if (busy)
        {
            return;
        }

        if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning("[SceneFader] Cannot load '" + sceneName + "'. Add it in File > Build Settings.", this);
            return;
        }

        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        busy = true;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        yield return Fade(1f);
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            busy = false;
            yield return Fade(0f);
            yield break;
        }

        while (!op.isDone)
        {
            yield return null;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
        yield return Fade(0f);
        busy = false;
    }

    IEnumerator Fade(float target)
    {
        if (group == null)
        {
            yield break;
        }

        group.blocksRaycasts = target > 0.5f;
        float start = group.alpha;
        float t = 0f;
        float duration = Mathf.Max(0.05f, fadeSeconds);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        group.alpha = target;
    }

    void Build()
    {
        GameObject canvasObject = new GameObject("FadeCanvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.layer = 5;
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        Image image = canvasObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;
        group = canvasObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        UiKit.Stretch(canvasObject.GetComponent<RectTransform>());
    }

    public static SceneFader Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject go = new GameObject("SceneFader");
        return go.AddComponent<SceneFader>();
    }
}
