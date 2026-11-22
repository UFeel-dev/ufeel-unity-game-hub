using UnityEngine;

namespace GameHub.Survivor
{
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
            if (!SurvivorGameManager.Instance.IsPlaying)
                return;

            Current = Mathf.Max(Current - amount, 0);

            if (Current == 0)
                SurvivorGameManager.Instance.GameOver();
        }

        public void Heal(int amount)
        {
            if (SurvivorGameManager.Instance.IsPlaying)
                Current = Mathf.Min(Current + amount, maxHealth);
        }
    }
}
