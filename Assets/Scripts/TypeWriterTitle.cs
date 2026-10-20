
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterTitle : MonoBehaviour
{
    [Header("Title")]
    public TMP_Text titleText;
    public string title = "The Writer's Study";

    [Header("Timing")]
    public float characterDelay = 0.2f;
    public float startDelay = 1f;
    public float paperFadeDuration = 1.5f;

    [Header("Paper")]
    public Image paperImage;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip keyClickSound;
    public AudioClip dingSound;

    private void Start()
    {
        StartCoroutine(WriteTitle());
    }

    private IEnumerator WriteTitle()
    {
        titleText.text = "";

        if (paperImage != null)
        {
            Color paperColor = paperImage.color;
            paperColor.a = 0f;
            paperImage.color = paperColor;

            float elapsed = 0f;

            while (elapsed < paperFadeDuration)
            {
                elapsed += Time.deltaTime;
                paperColor.a = Mathf.Clamp01(
                    elapsed / paperFadeDuration
                );
                paperImage.color = paperColor;

                yield return null;
            }

            paperColor.a = 1f;
            paperImage.color = paperColor;
        }

        yield return new WaitForSeconds(startDelay);

        foreach (char letter in title)
        {
            titleText.text += letter;

            if (audioSource != null && keyClickSound != null
                && !char.IsWhiteSpace(letter))
            {
                audioSource.PlayOneShot(keyClickSound);
            }

            yield return new WaitForSeconds(characterDelay);
        }

        if (audioSource != null && dingSound != null)
        {
            audioSource.PlayOneShot(dingSound);
        }
    }
}
