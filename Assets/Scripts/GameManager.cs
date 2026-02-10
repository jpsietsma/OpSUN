using UnityEngine;
using TMPro;  // for TextMeshPro UI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int itemsCollected = 0;

    [SerializeField] private TextMeshProUGUI winText; // assign in Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectItem()
    {
        itemsCollected++;
        Debug.Log("Items collected: " + itemsCollected);

        if (itemsCollected >= 1)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        Debug.Log("You win! 🎉");
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
        }

        // Optional: pause the game
        // Time.timeScale = 0f;
    }
}