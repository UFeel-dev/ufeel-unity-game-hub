using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Text healthText;

    [SerializeField] private Text timeText;
    [SerializeField] private Text killsText;

    private void Update()
    {
        if (SurvivorGameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        healthText.text = "HP: " + playerHealth.Current;

        timeText.text = "Time: " + ScoreManager.Instance.SurvivalTime.ToString("0.0");

        killsText.text = "Kills: " + ScoreManager.Instance.EnemiesKilled;
    }

    private void OnEnable()
    {
        SurvivorGameManager.Instance.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (SurvivorGameManager.Instance != null)
        {
            SurvivorGameManager.Instance.StateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged(GameState state)
    {
        gameObject.SetActive(state == GameState.Playing);
    }
}
