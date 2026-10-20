using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Queued inner-thought / subtitle lines. Unscaled fade. Built at runtime.
/// </summary>
public class SubtitleDirector : MonoBehaviour
{
    struct Playing
    {
        public string Speaker;
        public string Text;
        public float Duration;
    }

    static SubtitleDirector instance;
    readonly Queue<Playing> queue = new Queue<Playing>();
    CanvasGroup group;
    TextMeshProUGUI speakerLabel;
    TextMeshProUGUI bodyLabel;
    bool showing;
    float timer;
    float duration;

    public static SubtitleDirector Instance => instance;

    void Awake()
    {
        instance = this;
        Build();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Play(DialogueLine line)
    {
        if (line == null)
        {
            return;
        }

        Ensure().Enqueue(line.Speaker, line.Text, line.Duration);
    }

    public static void Play(string speaker, string text, float seconds = 3.5f)
    {
        Ensure().Enqueue(speaker, text, seconds);
    }

    public void Enqueue(string speaker, string text, float seconds)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        queue.Enqueue(new Playing
        {
            Speaker = speaker,
            Text = text,
            Duration = seconds > 0.1f ? seconds : 3f
        });
    }

    void Update()
    {
        if (GameMenu.Instance != null && GameMenu.Instance.IsPaused)
        {
            return;
        }

        if (!showing)
        {
            if (queue.Count == 0)
            {
                if (group != null)
                {
                    group.alpha = 0f;
                }

                return;
            }

            Playing next = queue.Dequeue();
            speakerLabel.text = string.IsNullOrEmpty(next.Speaker) ? string.Empty : next.Speaker;
            speakerLabel.gameObject.SetActive(!string.IsNullOrEmpty(next.Speaker));
            bodyLabel.text = next.Text;
            duration = next.Duration;
            timer = 0f;
            showing = true;
        }

        timer += Time.unscaledDeltaTime;
        float fade = 0.25f;
        float inA = Mathf.Clamp01(timer / fade);
        float outA = Mathf.Clamp01((duration - timer) / fade);
        group.alpha = Mathf.Min(inA, outA);
        if (timer >= duration)
        {
            showing = false;
            group.alpha = 0f;
        }
    }

    void Build()
    {
        GameObject canvasObject = new GameObject("SubtitleCanvas", typeof(RectTransform));
        canvasObject.layer = 5;
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Image panel = UiKit.CreateImage("Subtitle", canvasObject.transform, UiKit.PanelSoft, true);
        UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 0f));
        UiKit.AddVerticalLayout(panel.gameObject, 2f, new RectOffset(24, 24, 12, 12));
        ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        group = panel.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        speakerLabel = UiKit.CreateText("Speaker", panel.transform, string.Empty, 16f, UiKit.Accent, TextAlignmentOptions.Center);
        UiKit.SetLayoutHeight(speakerLabel, 22f);
        bodyLabel = UiKit.CreateText("Body", panel.transform, string.Empty, 24f, UiKit.Text, TextAlignmentOptions.Center, wrap: true);
    }

    static SubtitleDirector Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject go = new GameObject("SubtitleDirector");
        return go.AddComponent<SubtitleDirector>();
    }
}
