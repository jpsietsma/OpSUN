using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Max Values")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;

    [Header("Current Values")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float hunger = 100f;
    [SerializeField] private float thirst = 100f;

    [Header("Combat")]
    [Min(0)] public int punchDamage = 10;
    [Tooltip("How much stamina is spent per punch.")]
    [Min(0)] public int punchStaminaCost = 10;

    public float Health => health;
    public float Stamina => stamina;
    public float Hunger => hunger;
    public float Thirst => thirst;

    public event Action OnChanged;

    private void Start()
    {
        // Ensure valid starting values
        health = Mathf.Clamp(health, 0, maxHealth);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        thirst = Mathf.Clamp(thirst, 0, maxThirst);
        OnChanged?.Invoke();
    }

    private void Update()
    {
        // Example drain over time (tweak or remove as needed)
        hunger = Mathf.Clamp(hunger - 0.4f * Time.deltaTime, 0, maxHunger);
        thirst = Mathf.Clamp(thirst - 0.7f * Time.deltaTime, 0, maxThirst);

        // Example stamina regen
        stamina = Mathf.Clamp(stamina + 10f * Time.deltaTime, 0, maxStamina);

        OnChanged?.Invoke();
    }

    // Public methods you can call later from damage/sprinting/eating/drinking
    public void TakeDamage(int amount)
    {
        health = Mathf.Clamp(health - amount, 0, maxHealth);
        OnChanged?.Invoke();
    }

    public void UseStamina(float amount)
    {
        stamina = Mathf.Clamp(stamina - amount, 0, maxStamina);
        OnChanged?.Invoke();
    }

    public bool TrySpendStamina(int cost)
    {
        cost = Mathf.Max(0, cost);
        if (stamina < cost) return false;
        stamina -= cost;
        return true;
    }

    public void Eat(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0, maxHunger);
        OnChanged?.Invoke();
    }

    public void Drink(float amount)
    {
        thirst = Mathf.Clamp(thirst + amount, 0, maxThirst);
        OnChanged?.Invoke();
    }

    public float Health01 => maxHealth <= 0 ? 0 : health / maxHealth;
    public float Stamina01 => maxStamina <= 0 ? 0 : stamina / maxStamina;
    public float Hunger01 => maxHunger <= 0 ? 0 : hunger / maxHunger;
    public float Thirst01 => maxThirst <= 0 ? 0 : thirst / maxThirst;

    public void ApplyConsumable(ItemDefinition item)
    {
        if (item == null) return;

        float healthDelta = item.healthBuff - item.healthDebuff;
        float hungerDelta = item.hungerBuff - item.hungerDebuff;
        float thirstDelta = item.thirstBuff - item.thirstDebuff;
        float staminaDelta = item.staminaBuff - item.staminaDebuff;

        // Replace these with your actual properties/methods:
        health = Mathf.Clamp(health + healthDelta, 0, maxHealth);
        hunger = Mathf.Clamp(hunger + hungerDelta, 0, maxHunger);
        thirst = Mathf.Clamp(thirst + thirstDelta, 0, maxThirst);
        stamina = Mathf.Clamp(stamina + staminaDelta, 0, maxStamina);

        OnChanged?.Invoke();
    }
}
