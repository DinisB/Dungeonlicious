using Dungeonlicious.Assets.Script;
using UnityEngine;

public class AttackChecker : MonoBehaviour
{
    private EnemyController _enemyController;
    [SerializeField] private GameObject player;
    private IDamageable _damageable;
    private void Start()
    {
        //_enemyController = GetComponent<EnemyController>();
        _damageable = GetComponent<IDamageable>();
        player = FindAnyObjectByType<PlayerHealth>().gameObject;
    }

    private void OnTriggerEnter(Collider collision)
    {
        /*
        if (collision.gameObject.layer == 6)
        {
            _enemyController.Damage(collision.gameObject.GetComponentInParent<PlayerHealth>().GetAttack(), player);
        }
        if (collision.gameObject.layer == 9)
        {
            _enemyController.Damage(collision.gameObject.GetComponent<Knife>().GetOwner().GetComponent<PlayerHealth>().GetAttack(),player);
        }
        */
         if (collision.gameObject.layer == 6)
        {
            _damageable.Damage(
                collision.gameObject
                    .GetComponentInParent<PlayerHealth>()
                    .GetAttack(),
                player);
        }

        if (collision.gameObject.layer == 9)
        {
            _damageable.Damage(
                collision.gameObject
                    .GetComponent<Knife>()
                    .GetOwner()
                    .GetComponent<PlayerHealth>()
                    .GetAttack(),
                player);
        }
    }
}
