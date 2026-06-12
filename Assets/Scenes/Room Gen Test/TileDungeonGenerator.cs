namespace Dungeonlicious.Assets.Script
{
    using System.Collections.Generic;
    using System.Collections;
    using TMPro;
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Unity.AI.Navigation;

    public class TileDungeonGenerator : MonoBehaviour
    {
        private static TileDungeonGenerator _instance;
        public static TileDungeonGenerator Instance => _instance;

        private bool isGenerating;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private List<GameObject> wallPrefabsRight;
        [SerializeField] private List<GameObject> wallPrefabsUp;
        [SerializeField] private List<GameObject> wallPrefabsLeft;
        [SerializeField] private List<GameObject> wallPrefabsDown;

        [SerializeField] private List<GameObject> cornerPrefabsRightUp;
        [SerializeField] private List<GameObject> cornerPrefabsRightDown;
        [SerializeField] private List<GameObject> cornerPrefabsLeftUp;
        [SerializeField] private List<GameObject> cornerPrefabsLeftDown;

        [SerializeField] private NavMeshSurface navMeshSurface;

        [SerializeField] private GameObject furnace;
        [SerializeField] private GameObject doorPrefab;

        [SerializeField] private bool useCustomSeed = false;
        [SerializeField] private int seed;
        [SerializeField, Range(1, 9)] private int level = 1;
        [SerializeField] private int maxLevel = 9;
        public int MaxLevel => maxLevel;
        [SerializeField] private int roomCount = 5;
        [SerializeField] private Vector2Int minRoomSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(7, 7);
        [SerializeField] private int roomSpacing = 2;

        [SerializeField] private RoomPropSpawner propSpawner;
        [SerializeField] private RoomFoodSpawner foodSpawner;
        [SerializeField] private RoomEnemySpawner enemySpawner;
        private readonly List<RoomData> placedRooms = new List<RoomData>();

        private Vector3 tileSize;

        private readonly HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
        private readonly List<RectInt> placedRoomRects = new List<RectInt>();

        private readonly List<(Vector2Int grid, Vector3 pos, Vector3 forward)> corridorEntrances
            = new List<(Vector2Int, Vector3, Vector3)>();

        private int wallRightIdx, wallUpIdx, wallLeftIdx, wallDownIdx;
        private int cornerRightUpIdx, cornerRightDownIdx, cornerLeftUpIdx, cornerLeftDownIdx;

        public int Level => level;
        public void IncreaseLevel() { level = Mathf.Min(level + 1, maxLevel); }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Debug.Log("SEED FINAL: " + seed);
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
            if (isGenerating) return;
            isGenerating = true;

            SeedKeeper keeper = SeedKeeper.Instance;

            navMeshSurface = FindFirstObjectByType<NavMeshSurface>();

            if (keeper != null && int.TryParse(keeper.Seed, out int parsedSeed))
                seed = parsedSeed;
            else
                seed = Environment.TickCount;

            DestroyDungeon();

            tileSize = MeasureTileSize(tilePrefab);

            GenerateDungeon();

            isGenerating = false;
        }

        private void DestroyDungeon()
        {
            StopAllCoroutines();

            floorTiles.Clear();
            placedRooms.Clear();
            placedRoomRects.Clear();
            corridorEntrances.Clear();

            wallRightIdx = wallUpIdx = wallLeftIdx = wallDownIdx = 0;
            cornerRightUpIdx = cornerRightDownIdx = cornerLeftUpIdx = cornerLeftDownIdx = 0;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            if (navMeshSurface != null)
            {
                navMeshSurface.RemoveData();
            }
        }

        private void GenerateDungeon()
        {
            int clampedLevel = Mathf.Clamp(level, 1, maxLevel);
            int finalSeed = seed + level * 1000;
            UnityEngine.Random.InitState(finalSeed);

            floorTiles.Clear();
            placedRooms.Clear();
            placedRoomRects.Clear();
            corridorEntrances.Clear();
            wallRightIdx = wallUpIdx = wallLeftIdx = wallDownIdx = 0;
            cornerRightUpIdx = cornerRightDownIdx = cornerLeftUpIdx = cornerLeftDownIdx = 0;

            int resolvedRoomCount = UnityEngine.Random.Range(5, 6 + clampedLevel);

            RectInt firstRect = RandomRect(Vector2Int.zero);
            SpawnRoom("Room_Start", firstRect);
            TeleportPlayerToCenter(firstRect);

            for (int a = 0; a < 20; a++)
            {
                Direction dir = UnityEngine.Random.value > 0.5f ? Direction.East : Direction.West;
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
                    RectInt baseRect = placedRoomRects[UnityEngine.Random.Range(1, placedRoomRects.Count)];
                    Direction dir = (Direction)UnityEngine.Random.Range(0, 4);
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

            Dictionary<int, HashSet<Vector2Int>> usedTiles =
            propSpawner.SpawnProps(placedRooms, tileSize);

            foodSpawner.SpawnFood(placedRooms, tileSize, usedTiles);
            enemySpawner.SpawnEnemies(placedRooms, tileSize, usedTiles, level);

            StartCoroutine(BakeNavMesh());
        }

        private IEnumerator BakeNavMesh()
        {
            yield return null;

            if (navMeshSurface == null)
                navMeshSurface = FindFirstObjectByType<NavMeshSurface>();

            if (navMeshSurface == null || navMeshSurface.gameObject == null)
                yield break;

            yield return new WaitForEndOfFrame();
            navMeshSurface.RemoveData();
            navMeshSurface.BuildNavMesh();
        }

        private void SpawnCorridor(RectInt from, RectInt to)
        {
            Vector2Int fromCenter = RectCenter(from);
            Vector2Int toCenter = RectCenter(to);
            Vector2Int cur = fromCenter;

            Vector2Int? exitDoorPos = null;
            Vector2Int? entryDoorPos = null;

            if (cur.x != toCenter.x)
            {
                int sx = toCenter.x > cur.x ? 1 : -1;
                while (cur.x != toCenter.x)
                {
                    cur.x += sx;
                    GameObject t1 = PlaceFloorTile(cur);
                    if (t1 != null) t1.layer = LayerMask.NameToLayer("Corridor");

                    GameObject t2 = PlaceFloorTile(new Vector2Int(cur.x, cur.y + 1));
                    if (t2 != null) t2.layer = LayerMask.NameToLayer("Corridor");

                    GameObject t3 = PlaceFloorTile(new Vector2Int(cur.x, cur.y - 1));
                    if (t3 != null) t3.layer = LayerMask.NameToLayer("Corridor");

                    if (exitDoorPos == null && !from.Contains(cur))
                        exitDoorPos = cur;

                    if (entryDoorPos == null && to.Contains(cur))
                        entryDoorPos = new Vector2Int(cur.x - sx, cur.y);
                }
            }

            if (cur.y != toCenter.y)
            {
                int sz = toCenter.y > cur.y ? 1 : -1;
                while (cur.y != toCenter.y)
                {
                    cur.y += sz;
                    GameObject t1 = PlaceFloorTile(cur);
                    if (t1 != null) t1.layer = LayerMask.NameToLayer("Corridor");

                    GameObject t2 = PlaceFloorTile(new Vector2Int(cur.x + 1, cur.y));
                    if (t2 != null) t2.layer = LayerMask.NameToLayer("Corridor");

                    GameObject t3 = PlaceFloorTile(new Vector2Int(cur.x - 1, cur.y));
                    if (t3 != null) t3.layer = LayerMask.NameToLayer("Corridor");

                    if (exitDoorPos == null && !from.Contains(cur))
                        exitDoorPos = cur;

                    if (entryDoorPos == null && to.Contains(cur))
                        entryDoorPos = new Vector2Int(cur.x, cur.y - sz);
                }
            }

            int dx = toCenter.x - fromCenter.x;
            int dy = toCenter.y - fromCenter.y;
            Vector3 exitFwd, entryFwd;

            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                int sx = dx > 0 ? 1 : -1;
                exitFwd = new Vector3(sx, 0, 0);
                entryFwd = new Vector3(-sx, 0, 0);
                if (exitDoorPos == null) exitDoorPos = sx > 0
                    ? new Vector2Int(from.xMax, fromCenter.y)
                    : new Vector2Int(from.xMin - 1, fromCenter.y);
                if (entryDoorPos == null) entryDoorPos = sx > 0
                    ? new Vector2Int(to.xMin - 1, toCenter.y)
                    : new Vector2Int(to.xMax, toCenter.y);
            }
            else
            {
                int sz = dy > 0 ? 1 : -1;
                exitFwd = new Vector3(0, 0, sz);
                entryFwd = new Vector3(0, 0, -sz);
                if (exitDoorPos == null) exitDoorPos = sz > 0
                    ? new Vector2Int(fromCenter.x, from.yMax)
                    : new Vector2Int(fromCenter.x, from.yMin - 1);
                if (entryDoorPos == null) entryDoorPos = sz > 0
                    ? new Vector2Int(toCenter.x, to.yMin - 1)
                    : new Vector2Int(toCenter.x, to.yMax);
            }


            PlaceFloorTile(exitDoorPos.Value);
            PlaceFloorTile(entryDoorPos.Value);
            RecordCorridorEntrance(exitDoorPos.Value, exitFwd);
            RecordCorridorEntrance(entryDoorPos.Value, entryFwd);
        }

        private void RecordCorridorEntrance(Vector2Int midTile, Vector3 forward)
        {
            Vector3 worldPos = GridToWorld(midTile);
            corridorEntrances.Add((midTile, worldPos, forward));
        }

        private void PlaceDoors()
        {
            HashSet<Vector2Int> placed = new HashSet<Vector2Int>();

            foreach ((Vector2Int grid, Vector3 pos, Vector3 forward) in corridorEntrances)
            {
                if (!placed.Add(grid))
                    continue;

                Instantiate(doorPrefab, pos, Quaternion.LookRotation(forward), transform);
            }
        }

        private void SpawnRoom(string roomName, RectInt rect)
        {
            placedRoomRects.Add(rect);

            GameObject root = new GameObject(roomName);
            root.transform.parent = transform;

            placedRooms.Add(new RoomData(rect, GridToWorld(RectCenter(rect)), root.transform));

            for (int x = rect.x; x < rect.xMax; x++)
                for (int z = rect.y; z < rect.yMax; z++)
                    PlaceFloorTile(new Vector2Int(x, z), root.transform);

            if (roomName == "Room_End")
            {
                Vector3 centerPos = GridToWorld(RectCenter(rect));
                centerPos.y = tileSize.y;
                if (furnace == null) furnace = GameObject.Find("FurnaceGen");
                furnace.transform.position = centerPos;
            }
        }

        private GameObject PlaceFloorTile(Vector2Int coord, Transform parent = null)
        {
            if (floorTiles.Contains(coord)) return null;
            floorTiles.Add(coord);

            Vector3 pos = GridToWorld(coord);
            GameObject t = Instantiate(tilePrefab, pos, Quaternion.identity, parent != null ? parent : transform);

            return t;
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
                if (needUp && !skipUp) SpawnWall(wallPrefabsUp, ref wallUpIdx, pos);
                if (needLeft && !skipLeft) SpawnWall(wallPrefabsLeft, ref wallLeftIdx, pos);
                if (needDown && !skipDown) SpawnWall(wallPrefabsDown, ref wallDownIdx, pos);
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
            Vector3 targetPos = GridToWorld(RectCenter(room));
            StartCoroutine(TeleportAfterFrame(targetPos));
        }

        private IEnumerator TeleportAfterFrame(Vector3 pos)
        {
            yield return null;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) yield break;

            pos.y = tileSize.y + 1f;

            player.transform.position = pos;
        }

        private Vector3 GridToWorld(Vector2Int coord)
            => new Vector3(coord.x * tileSize.x, 0f, coord.y * tileSize.z);

        private static Vector2Int RectCenter(RectInt r)
            => new Vector2Int(r.x + r.width / 2, r.y + r.height / 2);

        private RectInt RandomRect(Vector2Int origin)
            => new RectInt(
                origin.x, origin.y,
                UnityEngine.Random.Range(minRoomSize.x, maxRoomSize.x + 1),
                UnityEngine.Random.Range(minRoomSize.y, maxRoomSize.y + 1));

        private RectInt AdjacentRect(RectInt baseRect, Direction dir)
        {
            int w = UnityEngine.Random.Range(minRoomSize.x, maxRoomSize.x + 1);
            int d = UnityEngine.Random.Range(minRoomSize.y, maxRoomSize.y + 1);
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