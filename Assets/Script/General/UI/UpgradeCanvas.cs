namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;

    public class UpgradeCanvas : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI upgradeText;
        [SerializeField]
        private Image banner;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangeUpgradeInfo(IUpgrade upgrade, Sprite bannerSprite)
        {
            upgradeText.text = $"{upgrade.upgradeType} + {upgrade.upgradeValue}";
            banner.sprite = bannerSprite;
        }
    }
}