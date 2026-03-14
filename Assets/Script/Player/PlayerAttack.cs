using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject _attackBox;
    [SerializeField] private Animator _spoonAnim;
    [SerializeField] private BoxCollider _attackCollider;

    private void Start()
    {
        _attackBox.SetActive(false);
    }

    private void Update()
    {
        if (InputSystem.actions.FindAction("Attack").WasPressedThisFrame())
        {
            _spoonAnim.SetBool("Attack", true);
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        _attackBox.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        _attackBox.SetActive(false);
        _spoonAnim.SetBool("Attack", false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (gameObject.activeInHierarchy)
            Gizmos.DrawWireCube(_attackCollider.bounds.center, _attackCollider.bounds.size);
    }
}
