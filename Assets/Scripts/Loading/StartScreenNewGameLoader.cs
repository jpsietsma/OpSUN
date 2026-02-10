using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenNewGameLoader : MonoBehaviour
{
    [Tooltip("Name of the loading scene in Build Settings.")]
    [SerializeField] private string loadingSceneName = "LoadingScreen";

    public void OnClick_NewGame()
    {
        SceneManager.LoadScene(loadingSceneName);
    }
}
