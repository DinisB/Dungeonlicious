using Dungeonlicious.Assets.Script;
using UnityEngine;

public class AttackChecker : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private IDamageable _damageable;
    private SlimeAI slimeAI;
    private BakonAI bakonAI;
    private AudioSource audioSource;
    [SerializeField] private AudioClip spoonOnSlime;
    [SerializeField] private AudioClip knifeOnSlime;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        _damageable = GetComponent<IDamageable>();
        player = FindAnyObjectByType<PlayerHealth>().gameObject;
    }

    private void OnTriggerEnter(Collider collision)
    {
         if (collision.gameObject.layer == 6)
        {
            if (gameObject.GetComponent<SlimeAI>() != null)
            {
                audioSource.PlayOneShot(spoonOnSlime);
            }
            _damageable.Damage(
                collision.gameObject
                    .GetComponentInParent<PlayerHealth>()
                    .GetAttack(),
                player);
        }

        if (collision.gameObject.layer == 9)
        {
            if (gameObject.GetComponent<SlimeAI>() != null)
            {
                audioSource.PlayOneShot(spoonOnSlime);
            }
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
