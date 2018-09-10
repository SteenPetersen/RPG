using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour {

    [SerializeField] AudioMixer mixer;

    public void SetVolume(float volume)
    {
        mixer.SetFloat("Volume", volume);
        
    }

    public void PlayGame()
    {
        StartCoroutine(GameDetails.instance.FadeOutAndLoadScene("TutorialArea_indoor"));
    }

    public void QuitGame()
    {
        Scene s = SceneManager.GetActiveScene();

        GameDetails.instance.Save();

        Application.Quit();
    }

    public void GoToTwitter()
    {
        Application.OpenURL("https://twitter.com/Pixlrelm");
    }

    public void GoToDiscord()
    {
        Application.OpenURL("https://discord.gg/SEMuw6x");
    }

    public void GoToPatreon()
    {
        Application.OpenURL("https://www.patreon.com/pixlrelm");
    }

    public void LoadGame()
    {
        StartCoroutine(GameDetails.instance.FadeOutAndLoadGame());
    }
}
