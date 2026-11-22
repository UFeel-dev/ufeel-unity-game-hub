using UnityEngine;

namespace GameHub.Survivor
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private float radius = 18f;
        [SerializeField] private float interval = 2f;

        private Transform player;
        private float timer;

        private void Start()
        {
            player = FindFirstObjectByType<PlayerController>().transform;
        }

        private void Update()
        {
            if (!SurvivorGameManager.Instance.IsPlaying)
                return;

            timer += Time.deltaTime;
            if (timer < interval)
                return;

            timer = 0f;
            Spawn();
        }

        private void Spawn()
        {
            if (player == null)
                return;

            Vector2 offset = Random.insideUnitCircle.normalized * radius;
            Instantiate(enemyPrefab, player.position + new Vector3(offset.x, 0f, offset.y), Quaternion.identity);
        }
    }
}
