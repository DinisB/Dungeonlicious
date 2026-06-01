namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;
    using UnityEngine.UI;

    public class UpgradeCanvasManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] UpgradeCanvas;
        private List<IUpgrade> upgrades;
        [SerializeField] private Sprite[] banners;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Array values = Enum.GetValues(typeof(UpgradeType));
            upgrades = new List<IUpgrade>();
            for (int i = 0; i < 3; i++)
            {
                IUpgrade upgrade = new Upgrade((UpgradeType)values.GetValue(UnityEngine.Random.Range(0, values.Length)), 0);
                upgrade.upgradeValue = UnityEngine.Random.Range(1, GetUpgradeValue(upgrade.upgradeType));
                upgrades.Add(upgrade);
            }
            UpdateUpgradeCanvas();
            for (int i = 0; i < 3; i++)
            {
                UpgradeCanvas[i].GetComponent<UpgradeCanvas>().onUpgradeSelected.AddListener(NextLevel);
            }
        }

        public int GetUpgradeValue(UpgradeType upgradeType)
        {
            if (upgradeType == UpgradeType.Health)
                return 20;
            else if (upgradeType == UpgradeType.Speed)
                return 1;
            else if (upgradeType == UpgradeType.Strength)
                return 5;
            else if (upgradeType == UpgradeType.Knife)
                return 1;
            else
                return 0;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void UpdateUpgradeCanvas()
        {
            for (int i = 0; i < UpgradeCanvas.Length; i++)
            {
                UpgradeCanvas[i].GetComponent<UpgradeCanvas>().ChangeUpgradeInfo(upgrades[i], banners[(int)upgrades[i].upgradeType]);
            }
        }

        public void NextLevel()
        {
            for (int i = 0; i < UpgradeCanvas.Length; i++)
            {
                UpgradeCanvas[i].SetActive(false);
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                gameManager.NextLevel();
            }
        }
    }
}