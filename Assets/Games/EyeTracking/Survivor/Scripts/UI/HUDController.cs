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
            if (!SurvivorGameManager.Instance.IsPlaying)
                return;

            healthText.text = $"HP: {playerHealth.Current}";
            timeText.text = $"Time: {ScoreManager.Instance.SurvivalTime:0.0}";
            killsText.text = $"Kills: {ScoreManager.Instance.EnemiesKilled}";
        }
    }
}
