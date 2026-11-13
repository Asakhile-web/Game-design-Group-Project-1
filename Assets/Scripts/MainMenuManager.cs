
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Button Click Audio")]
    [SerializeField] AudioSource buttonAudioSource;
    [SerializeField] AudioClip buttonClickSound;

    void PlayButtonClick()
    {
        if (buttonAudioSource != null && buttonClickSound != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    public void NewGame()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        PlayButtonClick();

        if (buttonClickSound != null)
        {
            yield return new WaitForSeconds(buttonClickSound.length);
        }

        SceneManager.LoadScene(1);
    }

    public void OpenSettings()
    {
        PlayButtonClick();
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayButtonClick();
        settingsPanel.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void OpenCredits()
    {
        PlayButtonClick();
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        PlayButtonClick();
        creditsPanel.SetActive(false);
    }
}
