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
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (upgrades == null)
                upgrades = new List<IUpgrade>();
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