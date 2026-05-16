using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private int healAmount;
    void OnTriggerEnter(Collider other)
    {
        CheckIfPickable(other);
    }

    private void CheckIfPickable(Collider other)
    {
        IHealable healable = other.GetComponent<IHealable>();

        if(healable != null && healable.CanBeHealed())
        {
            healable.Heal(healAmount);

            gameObject.SetActive(false);
        }
    }
}
