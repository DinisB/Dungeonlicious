namespace Dungeonlicious.Assets.Script
{
    using System.Collections.Generic;
    using UnityEngine;

    public class RoomEncounterManager : MonoBehaviour
    {
        private readonly List<GameObject> doors = new List<GameObject>();
        private readonly List<RoomEncounterTrigger> triggers = new List<RoomEncounterTrigger>();
        private readonly List<GameObject> enemies = new List<GameObject>();

        private bool encounterActive = false;
        private int enemiesAlive = 0;

        public void RegisterDoor(GameObject door)
        {
            doors.Add(door);

            RoomEncounterTrigger trigger = door.GetComponent<RoomEncounterTrigger>();

                trigger.Init(this);
                triggers.Add(trigger);
        }

        public void RegisterEnemies(List<GameObject> spawnedEnemies)
        {
            enemies.AddRange(spawnedEnemies);
        }

        public void StartEncounter()
        {
            if (encounterActive) return;

            if (enemies.Count == 0) return;

            encounterActive = true;

            foreach (GameObject door in doors)
                foreach (Animator anim in door.GetComponentsInChildren<Animator>())
                    anim.SetBool("Close", true);

            // Subscreve o evento de morte de cada inimigo
            enemiesAlive = 0;
            foreach (GameObject enemy in enemies)
            {
                enemiesAlive++;
                EnemyDeathNotifier notifier = enemy.GetComponent<EnemyDeathNotifier>();
                if (notifier == null)
                    notifier = enemy.AddComponent<EnemyDeathNotifier>();

                notifier.OnDeath += OnEnemyDied;
            }

            if (enemiesAlive == 0) UnlockRoom();
        }

        private void OnEnemyDied()
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
            if (enemiesAlive <= 0)
                UnlockRoom();
        }

        private void UnlockRoom()
        {
            encounterActive = false;

            foreach (GameObject door in doors)
                foreach (Animator anim in door.GetComponentsInChildren<Animator>())
                    anim.SetBool("Close", false);
        }

        private void Update()
        {
            if (!encounterActive) return;

            int alive = 0;
            foreach (GameObject enemy in enemies)
                if (enemy != null) alive++;

            if (alive == 0) UnlockRoom();
        }
    }
}
