namespace Dungeonlicious.Assets.Script
{
    using System.Collections.Generic;
    using UnityEngine;

    public class RoomPropSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> propPrefabs;
        [SerializeField, Range(1, 4)] private int minProps = 1;
        [SerializeField, Range(1, 4)] private int maxProps = 2;

        public void SpawnProps(List<RoomData> rooms, Vector3 tileSize)
        {
            for (int i = 1; i < rooms.Count - 1; i++)
            {
                RoomData room = rooms[i];
                List<Vector2Int> tiles = new List<Vector2Int>();
                for (int x = room.rect.x + 2; x < room.rect.xMax - 2; x++)
                    for (int z = room.rect.y + 2; z < room.rect.yMax - 2; z++)
                        tiles.Add(new Vector2Int(x, z));

                int count = Mathf.Min(Random.Range(minProps, maxProps + 1), tiles.Count);
                for (int p = 0; p < count; p++)
                {
                    int idx = Random.Range(0, tiles.Count);
                    Vector2Int tile = tiles[idx];
                    tiles.RemoveAt(idx);
                    Vector3 pos = new Vector3(tile.x * tileSize.x, tileSize.y, tile.y * tileSize.z);
                    GameObject prefab = propPrefabs[Random.Range(0, propPrefabs.Count)];
                    Instantiate(prefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), room.root);
                }
            }
        }
    }
}