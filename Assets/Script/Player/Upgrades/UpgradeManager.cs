namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using System.Collections.Generic;

    public class UpgradeManager : MonoBehaviour
    {
        private List<IUpgrade> upgrades;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
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
    }
}