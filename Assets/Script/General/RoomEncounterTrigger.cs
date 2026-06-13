namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class RoomEncounterTrigger : MonoBehaviour
    {
        private RoomEncounterManager roomManager;
        private bool triggered = false;

        public void Init(RoomEncounterManager manager)
        {
            roomManager = manager;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;

            triggered = true;
            roomManager.StartEncounter();
        }
    }
}
