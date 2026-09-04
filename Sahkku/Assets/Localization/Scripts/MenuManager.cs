using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuPanel;
    [SerializeField]
    private GameObject gameOptionsPanel;

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

    public void ToggleOdds(bool value)
    {
        GameSettings.evenOdds = value;
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
        GameSettings.muteSounds = value;
    }

    public void ToggleMusic(bool value)
    {
        GameSettings.muteMusic = value;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
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