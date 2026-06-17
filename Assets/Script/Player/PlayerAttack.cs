using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject _attackBox;
    [SerializeField] private Animator _anim;
    [SerializeField] private BoxCollider _attackCollider;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spoonSwing;
    private bool _isAttacking;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _attackBox.SetActive(false);
    }

    private void Update()
    {
        if (InputSystem.actions.FindAction("Attack").WasPressedThisFrame() && !_isAttacking)
        {
            audioSource.PlayOneShot(spoonSwing);
            _isAttacking = true;
            _anim.SetBool("Attack", true);
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        _attackBox.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        _attackBox.SetActive(false);
        _anim.SetBool("Attack", false);
        _isAttacking = false;   
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (gameObject.activeInHierarchy)
            Gizmos.DrawWireCube(_attackCollider.bounds.center, _attackCollider.bounds.size);
    }

    public bool IsAttacking()
    {
        return _isAttacking;
    }
}
