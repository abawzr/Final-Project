using UnityEngine;

public class FirstRoomSpawnEnemy : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemyRespawnPoint;

    private bool _isTriggered = false;

    private void Awake()
    {
        _isTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isTriggered) return;

        _isTriggered = true;

        enemy.SetActive(true);
        enemy.GetComponent<Enemy>().Respawn(enemyRespawnPoint.position);
    }
}
