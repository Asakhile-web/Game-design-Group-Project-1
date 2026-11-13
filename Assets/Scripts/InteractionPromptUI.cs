
using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public InteractionRaycaster raycaster;
    public TextMeshProUGUI promptText;

    void Update()
    {
        if (raycaster == null || promptText == null)
            return;

        if (raycaster.HasTarget && !string.IsNullOrEmpty(raycaster.CurrentPrompt))
        {
            promptText.text = raycaster.CurrentPrompt;
            promptText.gameObject.SetActive(true);
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }
}
