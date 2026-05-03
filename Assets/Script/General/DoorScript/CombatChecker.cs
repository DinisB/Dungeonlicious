using System.Collections.Generic;
using UnityEngine;

public class CombatChecker : MonoBehaviour
{
    [SerializeField] private GameObject[] doors;

    private List<GameObject> enemiesInCombat = new List<GameObject>();
    private bool playerInside = false;

    void Start()
    {
        foreach (GameObject door in doors)
        {
            door.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!enemiesInCombat.Contains(other.gameObject))
            {
                enemiesInCombat.Add(other.gameObject);
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = true;
        }

        UpdateDoors();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (enemiesInCombat.Contains(other.gameObject))
            {
                enemiesInCombat.Remove(other.gameObject);
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = false;
        }

        UpdateDoors();
    }

    void UpdateDoors()
    {
        bool shouldClose = playerInside && enemiesInCombat.Count > 0;

        foreach (GameObject door in doors)
        {
            door.SetActive(shouldClose);
        }
    }
}