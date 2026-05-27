

namespace Dungeonlicious.Assets.Script
{
    public interface IUpgrade
    {
        UpgradeType upgradeType { get; set; }
        float upgradeValue { get; set; }
    }
}