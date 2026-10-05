using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 1;

    public int CurrentHealth { get; private set; }

    public bool IsDead { get; private set; }

    public static event Action OnPlayerDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        CurrentHealth -= damage;

        CurrentHealth =
            Mathf.Max(CurrentHealth, 0);

        Debug.Log(
            $"Player took {damage} damage. Health = {CurrentHealth}"
        );

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        Debug.Log("PLAYER DIED");

        OnPlayerDied?.Invoke();
    }
}