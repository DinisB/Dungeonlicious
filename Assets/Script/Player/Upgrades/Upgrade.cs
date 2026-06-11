namespace Dungeonlicious.Assets.Script
{
    public class Upgrade : IUpgrade
    {
        public UpgradeType upgradeType { get; set; }
        public float upgradeValue { get; set; }
        public string upgradeDesc { get; set; }

        public Upgrade(UpgradeType upgradeType, float upgradeValue, string upgradeDesc)
        {
            this.upgradeType = upgradeType;
            this.upgradeValue = upgradeValue;
            this.upgradeDesc = upgradeDesc;
        }
    }
}