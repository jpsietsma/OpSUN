using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;

    [Header("Bars (Image Type = Filled)")]
    public Image healthFill;
    public Image staminaFill;
    public Image hungerFill;
    public Image thirstFill;

    private void Awake()
    {
        if (stats == null)
            stats = FindFirstObjectByType<PlayerStats>();
    }

    private void OnEnable()
    {
        if (stats != null)
            stats.OnChanged += Refresh;
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.OnChanged -= Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (stats == null) return;

        if (healthFill != null) healthFill.fillAmount = stats.Health01;
        if (staminaFill != null) staminaFill.fillAmount = stats.Stamina01;
        if (hungerFill != null) hungerFill.fillAmount = stats.Hunger01;
        if (thirstFill != null) thirstFill.fillAmount = stats.Thirst01;
    }
}
