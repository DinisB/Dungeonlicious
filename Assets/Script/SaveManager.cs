namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using System.IO;
    using System.Linq;

    public class SaveManager : MonoBehaviour
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        private static SaveManager _instance;
        public static SaveManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Save()
        {
            SaveData data = new SaveData
            {
                dungeonLevel = TileDungeonGenerator.Instance.Level,
                seed = TileDungeonGenerator.Instance.CurrentSeed,
                upgrades = UpgradeManager.Instance
                    .GetUpgrades()
                    .Select(upgrade => new UpgradeSaveEntry
                    {
                        upgradeType = upgrade.upgradeType,
                        upgradeValue = upgrade.upgradeValue,
                        upgradeDesc = upgrade.upgradeDesc,
                        upgradeName = upgrade.upgradeName
                    })
                    .ToArray()
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }

        public void ApplyLoad()
        {
            if (!HasSave()) return;

            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            if (data == null) return;

            TileDungeonGenerator.Instance.PrepareForLoad(data.dungeonLevel, data.seed);

            if (data.upgrades != null)
            {
                foreach (UpgradeSaveEntry entry in data.upgrades)
                {
                    UpgradeManager.Instance.AddUpgrade(new Upgrade(
                        entry.upgradeType,
                        entry.upgradeValue,
                        entry.upgradeDesc,
                        entry.upgradeName
                    ));
                }
            }
        }

        public static bool HasSaveFile() => File.Exists(SavePath);
        public static void DeleteSaveFile()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        public bool HasSave() => HasSaveFile();
        public void DeleteSave() => DeleteSaveFile();
    }
}