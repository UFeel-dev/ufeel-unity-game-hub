using UnityEngine;

namespace GameHub.Survivor
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 3f;

        private Transform target;
        private float speed;
        private float age;

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= lifeTime || target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += speed * Time.deltaTime * direction;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out EnemyController enemy))
                return;

            enemy.Die();
            Destroy(gameObject);
        }

        public void Initialize(Transform newTarget, float newSpeed)
        {
            target = newTarget;
            speed = newSpeed;
        }
    }
}
