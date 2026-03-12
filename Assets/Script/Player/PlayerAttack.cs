using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject _attackBox;
    [SerializeField] private Animator _spoonAnim;

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
        _attackBox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        _attackBox.SetActive(false);
        _spoonAnim.SetBool("Attack", false);
    }
}
