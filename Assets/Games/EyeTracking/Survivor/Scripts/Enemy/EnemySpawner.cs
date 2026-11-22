using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private float radius = 18f;
    [SerializeField] private float interval = 2f;

    private float timer;
    private Transform player;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerController>().transform;
    }

    private void Update()
    {
        if (SurvivorGameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        if (player == null)
        {
            return;
        }

        Vector2 circle = Random.insideUnitCircle.normalized * radius;

        Vector3 pos = player.position + new Vector3(circle.x, 0f, circle.y);

        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }
}
