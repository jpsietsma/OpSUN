using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called when Start Game button is pressed
    public void OnStartGame()
    {
        // Make sure the scene name matches your gameplay scene
        SceneManager.LoadScene("SinglePlayerGameScene");
    }

    // Called when Start Game button is pressed
    public void OnLoadGame()
    {
        Debug.Log("Load Game clicked - TODO: do load/save integration");
    }

    // Called when Host Game button is pressed
    public void OnHostGame()
    {
        Debug.Log("Host Game clicked - TODO: do multiplayer integration");
    }

    // Called when Join Game button is pressed
    public void OnJoinGame()
    {
        Debug.Log("Join Game clicked - TODO: do multiplayer integration");
    }

    // Called when Options button is pressed
    public void OnOptions()
    {
        // For now, just log or later open an options panel
        Debug.Log("Options clicked - TODO: show options menu");
    }

    // Called when Quit button is pressed
    public void OnQuitGame()
    {
        Debug.Log("Quit pressed");
        Application.Quit();

#if UNITY_EDITOR
        // So it also works in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}