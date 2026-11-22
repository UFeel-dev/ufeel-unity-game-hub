using UnityEngine;

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
        if (SurvivorGameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += speed * Time.deltaTime * dir;
    }

    public void Die()
    {
        ScoreManager.Instance.RegisterKill();
        Destroy(gameObject);
    }
}
