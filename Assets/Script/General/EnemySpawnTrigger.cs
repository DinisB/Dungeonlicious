using Dungeonlicious.Assets.Script;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    private CombatChecker combatChecker;

    private bool hasSpawned = false;

    private void Awake()
    {
        combatChecker = GetComponent<CombatChecker>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasSpawned) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            SpawnEnemies();
            hasSpawned = true;
        }
    }

    void SpawnEnemies()
    {
        foreach (Transform point in spawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, point.position, point.rotation);

            EnemyController controller = enemy.GetComponentInChildren<EnemyController>();

            if (controller != null)
            {
                controller.Initialize(combatChecker);
            }
        }
    }
}
