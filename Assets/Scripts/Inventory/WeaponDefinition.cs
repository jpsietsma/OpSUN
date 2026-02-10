using UnityEngine;

public enum WeaponType { Blunt, Bladed, Explosive, Ranged_Piercing, Ranged_Explosive }
public enum AmmoType { None, Bullet_9mm, Bullet_Shotgun, Arrow }

[CreateAssetMenu(menuName = "Game/Weapons/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    public WeaponType weaponType;
    public bool requireAmmo;

    [Tooltip("Allowed ammo types for the weapon")]
    public AmmoType ammoType;

    [Header("Damage Effects")]

    [Tooltip("Amount of damage done without buffs or debuffs")]
    public float baseDamage;

    [Tooltip("Max amount of ammo per clip")]
    public float maxAmmo;

    [Tooltip("Distance Weapon can hit")]
    public float weaponRange;

    [Tooltip("Distance begin damage debuff")]
    public float distanceBeginDebuff;

    [Tooltip("The % of damage per distance to lower")]
    public float distanceDebuffAmount;

}