using UnityEngine;

namespace GameHub.Survivor
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public float SurvivalTime { get; private set; }

        public int EnemiesKilled { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            SurvivorGameManager.Instance.StateChanged += OnStateChanged;
        }

        private void Update()
        {
            if (SurvivorGameManager.Instance.IsPlaying)
                SurvivalTime += Time.deltaTime;
        }

        private void OnDestroy()
        {
            if (SurvivorGameManager.Instance != null)
                SurvivorGameManager.Instance.StateChanged -= OnStateChanged;

            if (Instance == this)
                Instance = null;
        }

        public void RegisterKill()
        {
            EnemiesKilled++;
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Playing)
                return;

            SurvivalTime = 0f;
            EnemiesKilled = 0;
        }
    }
}
