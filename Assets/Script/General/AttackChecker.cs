using UnityEngine;

public class AttackChecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 6)
        {
            Debug.Log("Enemy" + gameObject.name + " was hit!");
        }
    }
}
