using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public float SurvivalTime { get; private set; }

    public int EnemiesKilled { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    private void Update()
    {
        if (SurvivorGameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        SurvivalTime += Time.deltaTime;
    }

    public void RegisterKill()
    {
        EnemiesKilled++;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing)
        {
            SurvivalTime = 0f;
            EnemiesKilled = 0;
        }
    }
}
