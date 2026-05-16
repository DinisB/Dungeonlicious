using UnityEngine;
using System.Collections;

public class Tomato : MonoBehaviour
{
    [SerializeField] private int attackAmmount;
    void OnTriggerEnter(Collider other)
    {
        CheckIfPickable(other);
    }

    private void CheckIfPickable(Collider other)
    {
        IAttackUpgrade attackable = other.GetComponent<IAttackUpgrade>();

        if(attackable != null && attackable.CanUpgradeAttack())
        {
            StartCoroutine(IncreaseAttack(attackable));

            gameObject.SetActive(false);
        }
    }

    private IEnumerator IncreaseAttack(IAttackUpgrade attackable)
    {
        attackable.IncreaseAttack(attackAmmount);

        yield return new WaitForSeconds(10f);

        attackable.IncreaseAttack(-attackAmmount);
    }
}
