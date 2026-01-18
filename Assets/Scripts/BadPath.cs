using UnityEngine;

public class BadPath : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Transform enemyRespawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        enemy.Respawn(enemyRespawnPoint.position);
    }
}
