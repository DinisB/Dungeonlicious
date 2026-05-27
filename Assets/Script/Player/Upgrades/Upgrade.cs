namespace Dungeonlicious.Assets.Script
{
    public class Upgrade : IUpgrade
    {
        public UpgradeType upgradeType { get; set; }
        public float upgradeValue { get; set; }

        public Upgrade(UpgradeType upgradeType, float upgradeValue)
        {
            this.upgradeType = upgradeType;
            this.upgradeValue = upgradeValue;
        }
    }
}