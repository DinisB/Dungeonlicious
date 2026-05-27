

namespace Dungeonlicious.Assets.Script
{
    public interface IUpgrade
    {
        UpgradeType UpgradeType { get; set; }
        float UpgradeValue { get; set; }
    }
}