using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Tomato : MonoBehaviour
{
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private int attackAmmount;

    private void OnTriggerStay(Collider other)
    {
        CheckIfPickable(other);
    }

    private void CheckIfPickable(Collider other)
    {
        IAttackUpgrade attackable = other.GetComponent<IAttackUpgrade>();

        if (attackable != null && interactAction.action.WasPressedThisFrame())
        {
            StartCoroutine(IncreaseAttack(attackable));
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private IEnumerator IncreaseAttack(IAttackUpgrade attackable)
    {
        attackable.IncreaseAttack(attackAmmount);

        yield return new WaitForSeconds(10f);

        attackable.IncreaseAttack(-attackAmmount);
    }
}