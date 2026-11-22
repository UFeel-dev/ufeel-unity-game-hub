using UnityEngine;

namespace GameHub.Survivor
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private float speed = 2.5f;

        private Transform target;

        private void Start()
        {
            target = FindFirstObjectByType<PlayerController>().transform;
        }

        private void Update()
        {
            if (SurvivorGameManager.Instance == null || !SurvivorGameManager.Instance.IsPlaying || target == null)
                return;

            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += speed * Time.deltaTime * direction;
        }

        public void Die()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.RegisterKill();
            Destroy(gameObject);
        }
    }
}
