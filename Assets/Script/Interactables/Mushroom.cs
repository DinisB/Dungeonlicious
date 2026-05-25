using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private int healAmount;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private GameObject _indicator;

    void OnTriggerStay(Collider other)
    {
        CheckIfPickable(other);
    }

    void OnTriggerExit(Collider other)
    {
        IHealable healable = other.GetComponent<IHealable>();

        if (healable != null)
        {
            _indicator.SetActive(false);
        }
    }

    private void CheckIfPickable(Collider other)
    {
        IHealable healable = other.GetComponent<IHealable>();

        if (healable != null && healable.CanBeHealed())
        {
            _indicator.SetActive(true);
        }

        if(healable != null && healable.CanBeHealed() && interactAction.action.IsPressed())
        {
            healable.Heal(healAmount);

            _indicator.SetActive(false);

            gameObject.SetActive(false);
        }
    }
}
