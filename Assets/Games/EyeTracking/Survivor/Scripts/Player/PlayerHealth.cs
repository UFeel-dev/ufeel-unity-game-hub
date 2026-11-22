using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;

    public int Current { get; private set; }

    private void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (SurvivorGameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        Current -= amount;

        if (Current <= 0)
        {
            Current = 0;
            SurvivorGameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        if (SurvivorGameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        Current = Mathf.Min(Current + amount, maxHealth);
    }
}
