using UnityEngine;

[System.Serializable]
public abstract class DamageService
{
    [SerializeField] protected DamageType _damageType;

    // Properties
    public DamageType DamageType => _damageType;
}