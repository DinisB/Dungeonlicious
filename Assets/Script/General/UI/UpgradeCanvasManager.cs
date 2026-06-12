namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

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
                UpgradeType type = (UpgradeType)values.GetValue(UnityEngine.Random.Range(0, values.Length));

                IUpgrade upgrade = new Upgrade(
                    type,
                    0,
                    GetUpgradeDesc(type),
                    GetUpgradeName(type)
                );

                upgrade.upgradeValue = UnityEngine.Random.Range(1, GetUpgradeValue(type) + 1);

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
                return 10;
            else if (upgradeType == UpgradeType.Speed)
                return 1;
            else if (upgradeType == UpgradeType.Strength)
                return 2;
            else if (upgradeType == UpgradeType.Knife)
                return 1;
            else
                return 0;
        }

        public string GetUpgradeDesc(UpgradeType upgradeType)
        {
            if (upgradeType == UpgradeType.Health)
                return "Betters your health";
            else if (upgradeType == UpgradeType.Speed)
                return "Makes you faster";
            else if (upgradeType == UpgradeType.Strength)
                return "Makes you beefier";
            else if (upgradeType == UpgradeType.Knife)
                return "Adds additional knives";
            else
                return "Idk man";
        }

        public string GetUpgradeName(UpgradeType upgradeType)
        {
            if (upgradeType == UpgradeType.Health)
                return "Carrot soup";
            else if (upgradeType == UpgradeType.Speed)
                return "Tomato soup";
            else if (upgradeType == UpgradeType.Strength)
                return "Beef Wellington";
            else if (upgradeType == UpgradeType.Knife)
                return "Knizza slice";
            else
                return "Idk man";
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            gameObject.SetActive(false);
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
                Time.timeScale = 1f;
                UpgradeCanvas[i].SetActive(false);
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                gameManager.NextLevel(SceneManager.GetActiveScene().name);
            }
        }
    }
}