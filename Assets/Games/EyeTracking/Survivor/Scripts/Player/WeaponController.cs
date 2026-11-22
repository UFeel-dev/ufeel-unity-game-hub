using UnityEngine;

namespace GameHub.Survivor
{
    public class WeaponController : MonoBehaviour
    {
        private static readonly Vector3 MuzzleOffset = Vector3.up * 1.2f;

        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float fireInterval = 0.4f;
        [SerializeField] private float projectileSpeed = 12f;

        private float timer;

        private void Update()
        {
            if (!SurvivorGameManager.Instance.IsPlaying)
                return;

            timer += Time.deltaTime;
            if (timer < fireInterval)
                return;

            timer = 0f;
            Fire();
        }

        private void Fire()
        {
            EnemyController target = FindClosestEnemy();
            if (target == null)
                return;

            Projectile projectile = Instantiate(projectilePrefab, transform.position + MuzzleOffset, Quaternion.identity);
            projectile.Initialize(target.transform, projectileSpeed);
        }

        private EnemyController FindClosestEnemy()
        {
            EnemyController closest = null;
            float closestDistance = float.MaxValue;

            foreach (EnemyController enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            {
                float distance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }

            return closest;
        }
    }
}
