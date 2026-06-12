namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using UnityEngine.Events;

    public class UpgradeCanvas : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TextMeshProUGUI upgradeText;
        [SerializeField]
        private TextMeshProUGUI upgradeName;
        [SerializeField]
        private Image banner;
        private IUpgrade upgrade;
        public UnityEvent onUpgradeSelected;

        private UpgradeManager upgradeManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            upgradeManager = UpgradeManager.Instance;
            if (upgradeManager == null) return;

            upgradeManager.AddUpgrade(upgrade);
            onUpgradeSelected.Invoke();
        }

        public void ChangeUpgradeInfo(IUpgrade upgrade, Sprite bannerSprite)
        {
            upgradeText.text = $"{upgrade.upgradeType} + {upgrade.upgradeValue} \n{upgrade.upgradeDesc}";
            banner.sprite = bannerSprite;
            this.upgrade = upgrade;
            upgradeName.text = upgrade.upgradeName;
        }
    }
}