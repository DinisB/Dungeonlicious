namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using System.Collections.Generic;

    public class UpgradeManager : MonoBehaviour
    {
        private static UpgradeManager instance;
        public static UpgradeManager Instance { get { return instance; } }
        private List<IUpgrade> upgrades;

        private void Awake()
        {
            upgrades = new List<IUpgrade>();
            EnforceSingleton();
        }

        private void EnforceSingleton()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void AddUpgrade(IUpgrade upgrade)
        {
            upgrades.Add(upgrade);
        }

        public IEnumerable<IUpgrade> GetUpgrades()
        {
            return upgrades;
        }
    }
}