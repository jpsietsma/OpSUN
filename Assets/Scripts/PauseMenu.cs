using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Called when Save Game button is pressed
    public void OnSaveGame()
    {
        Debug.Log("Host Game clicked - TODO: do Save Game integration");
    }

    // Called when Graphics Settings button is pressed
    public void OnGraphicsSettings()
    {
        Debug.Log("Host Game clicked - TODO: do Graphics Settings integration");
    }

    // Called when Audio Settings button is pressed
    public void OnAudioSettings()
    {
        Debug.Log("Join Game clicked - TODO: do multiplayer integration");
    }

    // Called when Quit to Main Menu button is pressed
    public void OnQuitToMenu()
    {
        SceneManager.LoadScene("StartScreenScene");
    }

    // Called when Exit to Desktop button is pressed
    public void OnExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        // So it also works in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}