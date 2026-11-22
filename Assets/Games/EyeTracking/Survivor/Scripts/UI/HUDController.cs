using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Survivor
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Text healthText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text killsText;

        private void Update()
        {
            if (SurvivorGameManager.Instance == null ||
                !SurvivorGameManager.Instance.IsPlaying ||
                ScoreManager.Instance == null ||
                playerHealth == null ||
                healthText == null ||
                timeText == null ||
                killsText == null)
            {
                return;
            }

            healthText.text = $"HP: {playerHealth.Current}";
            timeText.text = $"Time: {ScoreManager.Instance.SurvivalTime:0.0}";
            killsText.text = $"Kills: {ScoreManager.Instance.EnemiesKilled}";
        }
    }
}
