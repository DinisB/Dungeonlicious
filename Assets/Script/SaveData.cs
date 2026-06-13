namespace Dungeonlicious.Assets.Script
{
    using System;

    [Serializable]
    public class SaveData
    {
        public int dungeonLevel;
        public int seed;
        public UpgradeSaveEntry[] upgrades;
    }

    [Serializable]
    public class UpgradeSaveEntry
    {
        public UpgradeType upgradeType;
        public float upgradeValue;
        public string upgradeDesc;
        public string upgradeName;
    }
}