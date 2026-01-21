using UnityEngine;

public class BadPath : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemyRespawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        enemy.SetActive(true);
        enemy.GetComponent<Enemy>().Respawn(enemyRespawnPoint.position);
    }
}
