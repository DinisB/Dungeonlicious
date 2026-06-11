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
        [SerializeField] private GameObject doorPrefab;

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

        private readonly List<(Vector3 pos, Vector3 forward)> corridorEntrances
            = new List<(Vector3, Vector3)>();

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
            corridorEntrances.Clear();
            wallRightIdx = wallUpIdx = wallLeftIdx = wallDownIdx = 0;
            cornerRightUpIdx = cornerRightDownIdx = cornerLeftUpIdx = cornerLeftDownIdx = 0;

            int resolvedRoomCount = Random.Range(5, 6 + clampedLevel);

            RectInt firstRect = RandomRect(Vector2Int.zero);
            SpawnRoom("Room_Start", firstRect);
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
                    string roomName = (i == resolvedRoomCount - 1) ? "Room_End" : $"Room_{i + 1}";
                    SpawnRoom(roomName, rect);
                    SpawnCorridor(baseRect, rect);
                    break;
                }
            }

            PlaceWalls();
            FillDiagonalGaps();
            PlaceDoors();
        }

        private void SpawnCorridor(RectInt from, RectInt to)
        {
            Vector2Int cur = RectCenter(from);
            Vector2Int end = RectCenter(to);

            if (cur.x != end.x)
            {
                int sx = end.x >= cur.x ? 1 : -1;

                int exitX  = sx > 0 ? from.xMax     : from.xMin - 1;
                int entryX = sx > 0 ? to.xMin - 1   : to.xMax;

                Vector2Int exitSecond = new Vector2Int(exitX + sx, cur.y);
                
                Vector3 forwardExit = new Vector3(sx, 0, 0);
                RecordCorridorEntrance(exitSecond, forwardExit);

                Vector2Int entrySecond = new Vector2Int(entryX - sx, cur.y);
                Vector3 forwardEntry = new Vector3(-sx, 0, 0);
                RecordCorridorEntrance(entrySecond, forwardEntry);

                while (cur.x != end.x)
                {
                    cur.x += sx;
                    PlaceFloorTile(cur);
                    PlaceFloorTile(new Vector2Int(cur.x, cur.y + 1));
                    PlaceFloorTile(new Vector2Int(cur.x, cur.y - 1));
                }
            }

            if (cur.y != end.y)
            {
                int sz = end.y >= cur.y ? 1 : -1;

                int exitZ  = sz > 0 ? from.yMax     : from.yMin - 1;
                int entryZ = sz > 0 ? to.yMin - 1   : to.yMax;

                Vector2Int exitSecond = new Vector2Int(cur.x, exitZ + sz);
                Vector3 forwardExit = new Vector3(0, 0, sz);
                RecordCorridorEntrance(exitSecond, forwardExit);

                Vector2Int entrySecond = new Vector2Int(cur.x, entryZ - sz);
                Vector3 forwardEntry = new Vector3(0, 0, -sz);
                RecordCorridorEntrance(entrySecond, forwardEntry);

                while (cur.y != end.y)
                {
                    cur.y += sz;
                    PlaceFloorTile(cur);
                    PlaceFloorTile(new Vector2Int(cur.x + 1, cur.y));
                    PlaceFloorTile(new Vector2Int(cur.x - 1, cur.y));
                }
            }
        }

        private void RecordCorridorEntrance(Vector2Int midTile, Vector3 forward)
        {
            Vector3 worldPos = GridToWorld(midTile);
            corridorEntrances.Add((worldPos, forward));
        }

        private void PlaceDoors()
        {
            if (doorPrefab == null) return;

            HashSet<Vector3> placed = new HashSet<Vector3>();

            foreach ((Vector3 pos, Vector3 forward) in corridorEntrances)
            {
                if (placed.Contains(pos)) continue;
                placed.Add(pos);

                Quaternion rot = Quaternion.LookRotation(forward);
                Instantiate(doorPrefab, pos, rot, transform);
            }
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
                bool needUp    = !floorTiles.Contains(new Vector2Int(t.x, t.y + 1));
                bool needLeft  = !floorTiles.Contains(new Vector2Int(t.x - 1, t.y));
                bool needDown  = !floorTiles.Contains(new Vector2Int(t.x, t.y - 1));

                bool skipRight = false, skipUp = false, skipLeft = false, skipDown = false;

                if (needRight && needUp)
                {
                    SpawnCorner(cornerPrefabsRightUp, ref cornerRightUpIdx, pos + new Vector3(.5f, 0f, 1f));
                    skipRight = true; skipUp = true;
                }
                if (needRight && needDown)
                {
                    SpawnCorner(cornerPrefabsRightDown, ref cornerRightDownIdx, pos + new Vector3(1f, 0f, -.5f));
                    skipRight = true; skipDown = true;
                }
                if (needLeft && needUp)
                {
                    SpawnCorner(cornerPrefabsLeftUp, ref cornerLeftUpIdx, pos + new Vector3(-1f, 0f, .5f));
                    skipLeft = true; skipUp = true;
                }
                if (needLeft && needDown)
                {
                    SpawnCorner(cornerPrefabsLeftDown, ref cornerLeftDownIdx, pos + new Vector3(-.5f, 0f, -1f));
                    skipLeft = true; skipDown = true;
                }

                if (needRight && !skipRight) SpawnWall(wallPrefabsRight, ref wallRightIdx, pos);
                if (needUp    && !skipUp)    SpawnWall(wallPrefabsUp,    ref wallUpIdx,    pos);
                if (needLeft  && !skipLeft)  SpawnWall(wallPrefabsLeft,  ref wallLeftIdx,  pos);
                if (needDown  && !skipDown)  SpawnWall(wallPrefabsDown,  ref wallDownIdx,  pos);
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
                bool n = floorTiles.Contains(new Vector2Int(tile.x,     tile.y + 1));
                bool s = floorTiles.Contains(new Vector2Int(tile.x,     tile.y - 1));
                bool e = floorTiles.Contains(new Vector2Int(tile.x + 1, tile.y));
                bool w = floorTiles.Contains(new Vector2Int(tile.x - 1, tile.y));

                if (n && e && !floorTiles.Contains(new Vector2Int(tile.x + 1, tile.y + 1)))
                    SpawnRotatedCorner(cornerPrefabsRightUp, ref cornerRightUpIdx,
                        tile, Quaternion.Euler(0, 0, 0), new Vector3(0.5f, 0f, 0f));

                if (n && w && !floorTiles.Contains(new Vector2Int(tile.x - 1, tile.y + 1)))
                    SpawnRotatedCorner(cornerPrefabsLeftUp, ref cornerLeftUpIdx,
                        tile, Quaternion.Euler(0, -90, 0), new Vector3(0f, 0f, 0.5f));

                if (s && e && !floorTiles.Contains(new Vector2Int(tile.x + 1, tile.y - 1)))
                    SpawnRotatedCorner(cornerPrefabsRightDown, ref cornerRightDownIdx,
                        tile, Quaternion.Euler(0, 90, 0), new Vector3(0f, 0f, -0.5f));

                if (s && w && !floorTiles.Contains(new Vector2Int(tile.x - 1, tile.y - 1)))
                    SpawnRotatedCorner(cornerPrefabsLeftDown, ref cornerLeftDownIdx,
                        tile, Quaternion.Euler(0, 180, 0), new Vector3(-0.5f, 0f, 0f));
            }
        }

        private void SpawnRotatedCorner(List<GameObject> prefabs, ref int idx,
            Vector2Int tilePos, Quaternion rotation, Vector3 offset)
        {
            if (prefabs == null || prefabs.Count == 0) return;
            GameObject p = prefabs[idx % prefabs.Count];
            idx++;
            Instantiate(p, GridToWorld(tilePos) + offset, rotation, transform);
        }

        private void SpawnCorner(List<GameObject> prefabs, ref int idx, Vector3 pos)
        {
            if (prefabs == null || prefabs.Count == 0) return;
            GameObject p = prefabs[idx % prefabs.Count];
            idx++;
            Instantiate(p, pos, p.transform.rotation, transform);
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
                origin.x, origin.y,
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