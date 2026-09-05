using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuPanel;
    [SerializeField]
    private GameObject gameOptionsPanel;

    [SerializeField]
    private Toggle soundToggle;
    [SerializeField]
    private Toggle musicToggle;
    [SerializeField]
    private Toggle oddToggle;

    private void Start()
    {
        ShowMainMenu();
    }

    public void PlayVersus()
    {
        GameSettings.singlePlayer = false;
        ShowGameOptions();
    }

    public void PlaySolo()
    {
        GameSettings.singlePlayer = true;
        ShowGameOptions ();
    }

    public void ToggleOdds()
    {
        GameSettings.evenOdds = oddToggle.isOn;
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameOptionsPanel.SetActive(false);
    }

    public void ShowGameOptions()
    {
        mainMenuPanel.SetActive(false);
        gameOptionsPanel.SetActive(true);
    }

    public void ToggleAudio(bool value)
    {
        GameSettings.muteSounds = !soundToggle.isOn;
        Debug.Log("Mute sounds: " + GameSettings.muteSounds);
    }

    public void ToggleMusic(bool value)
    {
        GameSettings.muteMusic = !musicToggle.isOn;
        Debug.Log("Mute music: " + GameSettings.muteMusic);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void OpenRules()
    {
        string rulesPath = Application.dataPath + "/../Rules.pdf";
        Debug.Log("Opening " + rulesPath);
        Application.OpenURL(rulesPath);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}