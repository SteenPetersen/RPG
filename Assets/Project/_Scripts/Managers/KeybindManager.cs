using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeybindManager : MonoBehaviour {

    public static KeybindManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    public Dictionary<string, KeyCode> Keybinds { get; private set; }

    public Dictionary<string, KeyCode> ActionBinds { get; private set; }

    public Dictionary<string, KeyCode> CameraBinds { get; private set; }

    private string bindName;


    void Start ()
    {
        Keybinds = new Dictionary<string, KeyCode>();

        ActionBinds = new Dictionary<string, KeyCode>();

        CameraBinds = new Dictionary<string, KeyCode>();

        // Default Keybinds
        BindKey("UP", KeyCode.W);
        BindKey("DOWN", KeyCode.S);
        BindKey("LEFT", KeyCode.A);
        BindKey("RIGHT", KeyCode.D);

        BindKey("ACTION1", KeyCode.Alpha1);
        BindKey("ACTION2", KeyCode.Alpha2);

        BindKey("ACTION3", KeyCode.Alpha3);
        BindKey("ACTION4", KeyCode.Alpha4);
        BindKey("ACTION5", KeyCode.Alpha5);

        BindKey("CAMERACW", KeyCode.E);
        BindKey("CAMERACCW", KeyCode.Q);
    }


    /// <summary>
    /// Binds a key to a given action
    /// </summary>
    /// <param name="key">The action that is to be changed</param>
    /// <param name="keyBind">The new keycode for a given action or key</param>
    private void BindKey(string key, KeyCode keyBind)
    {
        // We guess that it is a keybinds that the player wants to swap around
        Dictionary<string, KeyCode> currentDictionary = Keybinds;

        // if the key contains the letters ACT then it is actually an action the player wants to swap
        if (key.Contains("ACT"))
        {
            // set the current dictionary to be the action dictionary instead
            currentDictionary = ActionBinds;
        }

        // if the key contains the letters CAMERA then it is actually the camera the player wants to swap
        if (key.Contains("CAMERA"))
        {
            // set the current dictionary to be the action dictionary instead
            currentDictionary = CameraBinds;
        }

        // if the dictionary is not currently using that keybind
        if (!currentDictionary.ContainsKey(key))
        {
            currentDictionary.Add(key, keyBind);
            UiManager.instance.UpdateKeyText(key, keyBind);
        }
        // if the current dictionary is already using that keybind
        else if (currentDictionary.ContainsValue(keyBind))
        {
            string myKey = currentDictionary.FirstOrDefault(x => x.Value == keyBind).Key;

            currentDictionary[myKey] = KeyCode.None;
            UiManager.instance.UpdateKeyText(key, KeyCode.None);

        }

        currentDictionary[key] = keyBind;
        UiManager.instance.UpdateKeyText(key, keyBind);
        bindName = string.Empty;
    }


    public void KeyBindOnClick(string bindName)
    {
        this.bindName = bindName;
    }

    private void OnGUI()
    {
        if (bindName != string.Empty)
        {
            Event e = Event.current;

            if (e.isKey)
            {
                BindKey(bindName, e.keyCode);
            }
        }
    }
}
