using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float fireInterval = 0.4f;
    [SerializeField] private float projectileSpeed = 12f;

    private float timer;

    private void Update()
    {
        if (SurvivorGameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= fireInterval)
        {
            timer = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        EnemyController target = FindClosestEnemy();

        if (target == null)
        {
            return;
        }

        Projectile p = Instantiate(projectilePrefab, transform.position + (Vector3.up * 1.2f), Quaternion.identity);

        p.Initialize(target.transform, projectileSpeed);
    }

    private EnemyController FindClosestEnemy()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        EnemyController best = null;
        float bestDist = float.MaxValue;

        foreach (var e in enemies)
        {
            float d = (e.transform.position - transform.position).sqrMagnitude;

            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        return best;
    }
}
