using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;

    void Start()
    {
        messageText.gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
    }
}