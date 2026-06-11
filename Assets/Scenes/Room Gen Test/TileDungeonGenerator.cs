namespace Dungeonlicious.Assets.Script
{
    using System.Collections.Generic;
    using UnityEngine;

    public class TileDungeonGenerator : MonoBehaviour
    {
        private static TileDungeonGenerator _instance;
        public static TileDungeonGenerator Instance => _instance;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private List<GameObject> wallPrefabsRight;
        [SerializeField] private List<GameObject> wallPrefabsUp;
        [SerializeField] private List<GameObject> wallPrefabsLeft;
        [SerializeField] private List<GameObject> wallPrefabsDown;

        [SerializeField] private List<GameObject> cornerPrefabsRightUp;
        [SerializeField] private List<GameObject> cornerPrefabsRightDown;
        [SerializeField] private List<GameObject> cornerPrefabsLeftUp;
        [SerializeField] private List<GameObject> cornerPrefabsLeftDown;

        [SerializeField] private GameObject furnacePrefab;

        [SerializeField] private bool useCustomSeed = false;
        [SerializeField] private int seed = 1337;
        [SerializeField, Range(1, 9)] private int level = 1;
        [SerializeField] private int maxLevel = 9;
        [SerializeField] private int roomCount = 5;
        [SerializeField] private Vector2Int minRoomSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(7, 7);
        [SerializeField] private int roomSpacing = 2;

        private Vector3 tileSize;

        private readonly HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
        private readonly List<RectInt> placedRoomRects = new List<RectInt>();

        private int wallRightIdx, wallUpIdx, wallLeftIdx, wallDownIdx;

        private int cornerRightUpIdx, cornerRightDownIdx, cornerLeftUpIdx, cornerLeftDownIdx;

        public int Level => level;
        public void IncreaseLevel() { level = Mathf.Min(level + 1, maxLevel); }

        private void Awake()
        {
            if (_instance == null) { _instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
        }

        private void Start()
        {
            tileSize = MeasureTileSize(tilePrefab);
            GenerateDungeon();
        }

        private void GenerateDungeon()
        {
            int clampedLevel = Mathf.Clamp(level, 1, maxLevel);
            int actualSeed = useCustomSeed
                ? seed + clampedLevel
                : System.Environment.TickCount + clampedLevel;
            Random.InitState(actualSeed);

            floorTiles.Clear();
            placedRoomRects.Clear();
            wallRightIdx = wallUpIdx = wallLeftIdx = wallDownIdx = 0;
            cornerRightUpIdx = cornerRightDownIdx = cornerLeftUpIdx = cornerLeftDownIdx = 0;

            int resolvedRoomCount = Random.Range(5, 5 + clampedLevel);

            RectInt firstRect = RandomRect(Vector2Int.zero);
            SpawnRoom("Room_1", firstRect);
            TeleportPlayerToCenter(firstRect);

            for (int a = 0; a < 20; a++)
            {
                Direction dir = Random.value > 0.5f ? Direction.East : Direction.West;
                RectInt rect = AdjacentRect(firstRect, dir);
                if (Overlaps(rect)) continue;
                SpawnRoom("Room_2", rect);
                SpawnCorridor(firstRect, rect);
                break;
            }

            for (int i = 2; i < resolvedRoomCount; i++)
            {
                for (int a = 0; a < 20; a++)
                {
                    RectInt baseRect = placedRoomRects[Random.Range(1, placedRoomRects.Count)];
                    Direction dir = (Direction)Random.Range(0, 4);
                    RectInt rect = AdjacentRect(baseRect, dir);
                    if (Overlaps(rect)) continue;
                    SpawnRoom($"Room_{i + 1}", rect);
                    SpawnCorridor(baseRect, rect);
                    break;
                }
            }

            PlaceWalls();
            FillDiagonalGaps();
            PlaceFurnace();
        }

        private void SpawnRoom(string roomName, RectInt rect)
        {
            placedRoomRects.Add(rect);

            GameObject root = new GameObject(roomName);
            root.transform.parent = transform;

            for (int x = rect.x; x < rect.xMax; x++)
                for (int z = rect.y; z < rect.yMax; z++)
                    PlaceFloorTile(new Vector2Int(x, z), root.transform);
        }

        private void SpawnCorridor(RectInt from, RectInt to)
        {
            Vector2Int cur = RectCenter(from);
            Vector2Int end = RectCenter(to);

            int sx = end.x >= cur.x ? 1 : -1;
            while (cur.x != end.x)
            {
                cur.x += sx;
                PlaceFloorTile(cur);
                PlaceFloorTile(new Vector2Int(cur.x, cur.y + 1));
                PlaceFloorTile(new Vector2Int(cur.x, cur.y - 1));
            }

            int sz = end.y >= cur.y ? 1 : -1;
            while (cur.y != end.y)
            {
                cur.y += sz;
                PlaceFloorTile(cur);
                PlaceFloorTile(new Vector2Int(cur.x + 1, cur.y));
                PlaceFloorTile(new Vector2Int(cur.x - 1, cur.y));
            }
        }

        private void PlaceFloorTile(Vector2Int coord, Transform parent = null)
        {
            if (floorTiles.Contains(coord)) return;
            floorTiles.Add(coord);

            Vector3 pos = GridToWorld(coord);
            Instantiate(tilePrefab, pos, Quaternion.identity, parent != null ? parent : transform);
        }

        private void PlaceWalls()
        {
            foreach (Vector2Int t in floorTiles)
            {
                Vector3 pos = GridToWorld(t);

                bool needRight = !floorTiles.Contains(new Vector2Int(t.x + 1, t.y));
                bool needUp = !floorTiles.Contains(new Vector2Int(t.x, t.y + 1));
                bool needLeft = !floorTiles.Contains(new Vector2Int(t.x - 1, t.y));
                bool needDown = !floorTiles.Contains(new Vector2Int(t.x, t.y - 1));

                bool skipRight = false, skipUp = false, skipLeft = false, skipDown = false;

                if (needRight && needUp)
                {
                    SpawnCorner(cornerPrefabsRightUp, ref cornerRightUpIdx, pos + new Vector3(1f,0f,1f));
                    skipRight = true;
                    skipUp = true;
                }
                if (needRight && needDown)
                {
                    SpawnCorner(cornerPrefabsRightDown, ref cornerRightDownIdx, pos + new Vector3(1f,0f,-1f));
                    skipRight = true;
                    skipDown = true;
                }
                if (needLeft && needUp)
                {
                    SpawnCorner(cornerPrefabsLeftUp, ref cornerLeftUpIdx, pos + new Vector3(-1f,0f,1f));
                    skipLeft = true;
                    skipUp = true;
                }
                if (needLeft && needDown)
                {
                    SpawnCorner(cornerPrefabsLeftDown, ref cornerLeftDownIdx, pos + new Vector3(-1f,0f,-1f));
                    skipLeft = true;
                    skipDown = true;
                }

                if (needRight && !skipRight)
                    SpawnWall(wallPrefabsRight, ref wallRightIdx, pos);
                if (needUp && !skipUp)
                    SpawnWall(wallPrefabsUp, ref wallUpIdx, pos);
                if (needLeft && !skipLeft)
                    SpawnWall(wallPrefabsLeft, ref wallLeftIdx, pos);
                if (needDown && !skipDown)
                    SpawnWall(wallPrefabsDown, ref wallDownIdx, pos);
            }
        }

        private void SpawnWall(List<GameObject> prefabs, ref int idx, Vector3 pos)
        {
            if (prefabs == null || prefabs.Count == 0) return;
            GameObject p = prefabs[idx % prefabs.Count];
            idx++;
            Instantiate(p, pos, p.transform.rotation, transform);
        }

        private void FillDiagonalGaps()
        {
            foreach (Vector2Int tile in floorTiles)
            {
                bool n = floorTiles.Contains(new Vector2Int(tile.x, tile.y + 1));
                bool s = floorTiles.Contains(new Vector2Int(tile.x, tile.y - 1));
                bool e = floorTiles.Contains(new Vector2Int(tile.x + 1, tile.y));
                bool w = floorTiles.Contains(new Vector2Int(tile.x - 1, tile.y));

                if (n && e && !floorTiles.Contains(new Vector2Int(tile.x + 1, tile.y + 1)))
                    SpawnRotatedCorner(cornerPrefabsRightUp, ref cornerRightUpIdx, tile, Quaternion.Euler(0, 0, 0));

                if (n && w && !floorTiles.Contains(new Vector2Int(tile.x - 1, tile.y + 1)))
                    SpawnRotatedCorner(cornerPrefabsLeftUp, ref cornerLeftUpIdx, tile, Quaternion.Euler(0, -90, 0));

                if (s && e && !floorTiles.Contains(new Vector2Int(tile.x + 1, tile.y - 1)))
                    SpawnRotatedCorner(cornerPrefabsRightDown, ref cornerRightDownIdx, tile, Quaternion.Euler(0, 90, 0));

                if (s && w && !floorTiles.Contains(new Vector2Int(tile.x - 1, tile.y - 1)))
                    SpawnRotatedCorner(cornerPrefabsLeftDown, ref cornerLeftDownIdx, tile, Quaternion.Euler(0, 180, 0));
            }
        }

        private void SpawnRotatedCorner(List<GameObject> prefabs, ref int idx, Vector2Int tilePos, Quaternion rotation)
        {
            if (prefabs == null || prefabs.Count == 0) return;
            GameObject p = prefabs[idx % prefabs.Count];
            idx++;
            Instantiate(p, GridToWorld(tilePos), rotation, transform);
        }

        private void SpawnCorner(List<GameObject> prefabs, ref int idx, Vector3 pos)
        {
            if (prefabs == null || prefabs.Count == 0) return;
            GameObject p = prefabs[idx % prefabs.Count];
            idx++;
            Instantiate(p, pos, p.transform.rotation, transform);
        }

        private void PlaceFurnace()
        {
            if (furnacePrefab == null || floorTiles.Count == 0) return;

            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;
            foreach (Vector2Int t in floorTiles)
            {
                if (t.x < minX) minX = t.x;
                if (t.x > maxX) maxX = t.x;
                if (t.y < minZ) minZ = t.y;
                if (t.y > maxZ) maxZ = t.y;
            }

            bool rightHasExit = false;
            foreach (Vector2Int t in floorTiles)
            {
                if (t.x == maxX && floorTiles.Contains(new Vector2Int(t.x + 1, t.y)))
                {
                    rightHasExit = true;
                    break;
                }
            }

            bool upHasExit = false;
            foreach (Vector2Int t in floorTiles)
            {
                if (t.y == maxZ && floorTiles.Contains(new Vector2Int(t.x, t.y + 1)))
                {
                    upHasExit = true;
                    break;
                }
            }

            if (!rightHasExit)
            {
                PlaceFurnaceOnRightEdge(maxX, minZ, maxZ);
                return;
            }
            if (!upHasExit)
            {
                PlaceFurnaceOnUpEdge(maxZ, minX, maxX);
                return;
            }

            int centreX = (minX + maxX) / 2;
            int centreZ = (minZ + maxZ) / 2;
            Instantiate(furnacePrefab, GridToWorld(new Vector2Int(centreX, centreZ)), Quaternion.identity, transform);
        }

        private void PlaceFurnaceOnRightEdge(int edgeX, int minZ, int maxZ)
        {
            List<Vector2Int> edgeTiles = new List<Vector2Int>();
            foreach (Vector2Int t in floorTiles)
                if (t.x == edgeX)
                    edgeTiles.Add(t);

            if (edgeTiles.Count == 0) return;

            int centreZ = (minZ + maxZ) / 2;

            Vector2Int best = edgeTiles[0];
            int bestDist = Mathf.Abs(best.y - centreZ);
            foreach (Vector2Int t in edgeTiles)
            {
                int dist = Mathf.Abs(t.y - centreZ);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }

            Instantiate(furnacePrefab, GridToWorld(best), Quaternion.identity, transform);
        }

        private void PlaceFurnaceOnUpEdge(int edgeZ, int minX, int maxX)
        {
            List<Vector2Int> edgeTiles = new List<Vector2Int>();
            foreach (Vector2Int t in floorTiles)
                if (t.y == edgeZ)
                    edgeTiles.Add(t);

            if (edgeTiles.Count == 0) return;

            int centreX = (minX + maxX) / 2;

            Vector2Int best = edgeTiles[0];
            int bestDist = Mathf.Abs(best.x - centreX);
            foreach (Vector2Int t in edgeTiles)
            {
                int dist = Mathf.Abs(t.x - centreX);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }

            Instantiate(furnacePrefab, GridToWorld(best), Quaternion.identity, transform);
        }

        private void TeleportPlayerToCenter(RectInt room)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;
            Vector3 pos = GridToWorld(RectCenter(room));
            pos.y = tileSize.y;
            player.transform.position = pos;
        }

        private Vector3 GridToWorld(Vector2Int coord)
            => new Vector3(coord.x * tileSize.x, 0f, coord.y * tileSize.z);

        private static Vector2Int RectCenter(RectInt r)
            => new Vector2Int(r.x + r.width / 2, r.y + r.height / 2);

        private RectInt RandomRect(Vector2Int origin)
            => new RectInt(
                origin.x,
                origin.y,
                Random.Range(minRoomSize.x, maxRoomSize.x + 1),
                Random.Range(minRoomSize.y, maxRoomSize.y + 1));

        private RectInt AdjacentRect(RectInt baseRect, Direction dir)
        {
            int w = Random.Range(minRoomSize.x, maxRoomSize.x + 1);
            int d = Random.Range(minRoomSize.y, maxRoomSize.y + 1);
            switch (dir)
            {
                case Direction.East:
                    return new RectInt(
                        baseRect.xMax + roomSpacing,
                        baseRect.y + Mathf.FloorToInt((baseRect.height - d) * 0.5f),
                        w, d);
                case Direction.West:
                    return new RectInt(
                        baseRect.xMin - roomSpacing - w,
                        baseRect.y + Mathf.FloorToInt((baseRect.height - d) * 0.5f),
                        w, d);
                case Direction.North:
                    return new RectInt(
                        baseRect.x + Mathf.FloorToInt((baseRect.width - w) * 0.5f),
                        baseRect.yMax + roomSpacing,
                        w, d);
                default:
                    return new RectInt(
                        baseRect.x + Mathf.FloorToInt((baseRect.width - w) * 0.5f),
                        baseRect.yMin - roomSpacing - d,
                        w, d);
            }
        }

        private bool Overlaps(RectInt rect)
        {
            foreach (RectInt r in placedRoomRects)
                if (rect.Overlaps(r)) return true;
            return false;
        }

        private Vector3 MeasureTileSize(GameObject prefab)
        {
            GameObject tmp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            tmp.hideFlags = HideFlags.HideAndDontSave;
            Bounds b = new Bounds(tmp.transform.position, Vector3.zero);
            foreach (Renderer r in tmp.GetComponentsInChildren<Renderer>())
                b.Encapsulate(r.bounds);
            if (b.size == Vector3.zero)
                foreach (Collider c in tmp.GetComponentsInChildren<Collider>())
                    b.Encapsulate(c.bounds);
            Destroy(tmp);
            return b.size == Vector3.zero ? Vector3.one : b.size;
        }
    }
}