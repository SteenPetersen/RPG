using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour {

    [SerializeField] AudioMixer mixer;

    public void SetVolume(float volume)
    {
        mixer.SetFloat("Volume", volume);
        
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
        GameDetails.instance.Load();
    }
}
