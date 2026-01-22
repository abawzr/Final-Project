using UnityEngine;

public class BadPath : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemyRespawnPoint;

    private bool _isActive = false;

    private void Awake()
    {
        _isActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isActive) return;

        _isActive = true;
        enemy.SetActive(true);
        enemy.GetComponent<Enemy>().Respawn(enemyRespawnPoint.position);
    }
}
