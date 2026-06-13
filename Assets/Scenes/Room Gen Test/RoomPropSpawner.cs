namespace Dungeonlicious.Assets.Script
{
    using System.Collections.Generic;
    using UnityEngine;

    public class RoomPropSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> propPrefabs;
        [SerializeField, Range(1, 4)] private int minProps = 1;
        [SerializeField, Range(1, 4)] private int maxProps = 2;

        public Dictionary<int, HashSet<Vector2Int>> SpawnProps(List<RoomData> rooms, Vector3 tileSize)
        {
            Dictionary<int, HashSet<Vector2Int>> usedTiles = new Dictionary<int, HashSet<Vector2Int>>();

            for (int i = 1; i < rooms.Count - 1; i++)
            {
                RoomData room = rooms[i];
                List<Vector2Int> tiles = new List<Vector2Int>();
                for (int x = room.rect.x + 3; x < room.rect.xMax - 3; x++)
                    for (int z = room.rect.y + 3; z < room.rect.yMax - 3; z++)
                        tiles.Add(new Vector2Int(x, z));

                HashSet<Vector2Int> roomUsed = new HashSet<Vector2Int>();
                int count = Mathf.Min(Random.Range(minProps, maxProps + 1), tiles.Count);
                for (int p = 0; p < count; p++)
                {
                    int idx = Random.Range(0, tiles.Count);
                    Vector2Int tile = tiles[idx];
                    tiles.RemoveAt(idx);
                    roomUsed.Add(tile);
                    Vector3 pos = new Vector3(tile.x * tileSize.x, tileSize.y, tile.y * tileSize.z);
                    GameObject prefab = propPrefabs[Random.Range(0, propPrefabs.Count)];
                    Instantiate(prefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), room.root);
                }

                usedTiles[i] = roomUsed;
            }

            return usedTiles;
        }
    }
}
