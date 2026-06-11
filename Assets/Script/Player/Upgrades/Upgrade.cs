namespace Dungeonlicious.Assets.Script
{
    public class Upgrade : IUpgrade
    {
        public UpgradeType upgradeType { get; set; }
        public float upgradeValue { get; set; }
        public string upgradeDesc { get; set; }
        public string upgradeName { get; set; }

        public Upgrade(UpgradeType upgradeType, float upgradeValue, string upgradeDesc, string upgradeName)
        {
            this.upgradeType = upgradeType;
            this.upgradeValue = upgradeValue;
            this.upgradeDesc = upgradeDesc;
            this.upgradeName = upgradeName;
        }
    }
}