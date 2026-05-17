using Dungeonlicious.Assets.Script;
using UnityEngine;

public class AttackChecker : MonoBehaviour
{
    private EnemyController _enemyController;
    private void Start()
    {
        _enemyController = GetComponent<EnemyController>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 6)
        {
            _enemyController.TakeDamage(collision.gameObject.GetComponentInParent<PlayerHealth>().GetAttack());
        }
        if (collision.gameObject.layer == 9)
        {
            _enemyController.TakeDamage(collision.gameObject.GetComponentInParent<PlayerHealth>().GetKnifesAttack());
        }
    }
}
