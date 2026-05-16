using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private int healAmount;
    [SerializeField] private InputActionReference interactAction;
    void OnTriggerStay(Collider other)
    {
        CheckIfPickable(other);
    }

    private void CheckIfPickable(Collider other)
    {
        IHealable healable = other.GetComponent<IHealable>();

        if(healable != null && healable.CanBeHealed() && interactAction.action.WasPressedThisFrame())
        {
            healable.Heal(healAmount);

            gameObject.SetActive(false);
        }
    }
}
