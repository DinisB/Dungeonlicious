using System.Collections.Generic;
using Dungeonlicious.Assets.Script;
using UnityEngine;

public class AttackChecker : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private IDamageable _damageable;
    private AudioSource audioSource;
    [SerializeField] private AudioClip spoonOnSlime;
    [SerializeField] private AudioClip knifeOnSlime;
    [SerializeField] private AudioClip bakonHit;

    private HashSet<GameObject> hitObjects = new();

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

            if (gameObject.GetComponent<BakonAI>() != null)
            {
                audioSource.PlayOneShot(bakonHit);
            }
            _damageable.Damage(
                collision.gameObject
                    .GetComponentInParent<PlayerHealth>()
                    .GetAttack(),
                player);
        }

        if (collision.gameObject.layer == 9)
        {
            if (hitObjects.Contains(collision.gameObject))
                return;

            hitObjects.Add(collision.gameObject);


            if (gameObject.GetComponent<SlimeAI>() != null)
            {
                audioSource.PlayOneShot(spoonOnSlime);
            }

            if (gameObject.GetComponent<BakonAI>() != null)
            {
                audioSource.PlayOneShot(bakonHit);
            }
            /*
            _damageable.Damage(
                collision.gameObject
                    .GetComponent<Knife>()
                    .GetOwner()
                    .GetComponent<PlayerHealth>()
                    .GetAttack(),
                player);
            */
        }
    }
}
