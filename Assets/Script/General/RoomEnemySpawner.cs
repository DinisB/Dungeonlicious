namespace Dungeonlicious.Assets.Script
{
    using System.Collections.Generic;
    using UnityEngine;

    public class RoomEnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject commonEnemy;
        [SerializeField] private GameObject rareEnemy;
        [SerializeField, Range(1, 4)] private int minEnemies = 1;
        [SerializeField, Range(1, 4)] private int maxEnemies = 3;

        public void SpawnEnemies(
            List<RoomData> rooms,
            Vector3 tileSize,
            Dictionary<int, HashSet<Vector2Int>> usedTiles,
            int level)
        {
            float rareChance = Mathf.Min(level * 0.03f, 0.30f);

            for (int i = 1; i < rooms.Count - 1; i++)
            {
                RoomData room = rooms[i];
                HashSet<Vector2Int> blocked = usedTiles.ContainsKey(i)
                    ? usedTiles[i]
                    : new HashSet<Vector2Int>();
                List<Vector2Int> tiles = new List<Vector2Int>();
                for (int x = room.rect.x + 2; x < room.rect.xMax - 2; x++)
                    for (int z = room.rect.y + 2; z < room.rect.yMax - 2; z++)
                    {
                        Vector2Int t = new Vector2Int(x, z);
                        if (!blocked.Contains(t)) tiles.Add(t);
                    }

                List<GameObject> spawnedEnemies = new List<GameObject>();

                int count = Mathf.Min(Random.Range(minEnemies, maxEnemies + 1), tiles.Count);
                for (int p = 0; p < count; p++)
                {
                    int idx = Random.Range(0, tiles.Count);
                    Vector2Int tile = tiles[idx];
                    tiles.RemoveAt(idx);

                    Vector3 pos = new Vector3(
                        tile.x * tileSize.x,
                        tileSize.y,
                        tile.y * tileSize.z);

                    GameObject prefab = Random.value < rareChance ? rareEnemy : commonEnemy;
                    GameObject enemy = Instantiate(
                        prefab, pos,
                        Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                        room.root);

                    spawnedEnemies.Add(enemy);
                }

                RoomEncounterManager manager = room.root.GetComponent<RoomEncounterManager>();
                if (manager == null)
                    manager = room.root.gameObject.AddComponent<RoomEncounterManager>();

                manager.RegisterEnemies(spawnedEnemies);
            }
        }
    }
}