namespace Dungeonlicious.Assets.Scripts
{
    using System.Collections.Generic;
    using UnityEngine;

    public class TileDungeonGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;

        [Header("Wall Prefabs")]
        [SerializeField] private GameObject wallPrefabRight;
        [SerializeField] private GameObject wallPrefabUp;
        [SerializeField] private GameObject wallPrefabLeft;
        [SerializeField] private GameObject wallPrefabDown;

        [SerializeField] private GameObject cornerPrefabRightUp;
        [SerializeField] private GameObject cornerPrefabUpLeft;
        [SerializeField] private GameObject cornerPrefabLeftDown;
        [SerializeField] private GameObject cornerPrefabDownRight;

        [Header("Seed Settings")]
        [SerializeField] private bool useCustomSeed = false;
        [SerializeField] private int seed = 1337;

        [SerializeField, Range(1, 9)] private int level = 1;
        [SerializeField] private int maxLevel = 9;

        [SerializeField] private int roomCount = 5;
        [SerializeField] private Vector2Int minRoomSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(7, 7);
        [SerializeField] private int roomSpacing = 2;

        private Vector3 tileSize;
        private readonly List<RoomData> placedRooms = new List<RoomData>();
        private readonly List<Vector3> corridorEntrances = new List<Vector3>();
        private readonly List<Vector3> corridorTiles = new List<Vector3>();

        private enum Direction
        {
            North,
            South,
            East,
            West
        }

        private struct RoomData
        {
            public RectInt rect;
            public Vector3 center;
            public Transform root;

            public RoomData(RectInt rect, Vector3 center, Transform root)
            {
                this.rect = rect;
                this.center = center;
                this.root = root;
            }
        }

        private void Start()
        {
            tileSize = GetTileSize(tilePrefab);

            GenerateDungeon();
        }

        private void GenerateDungeon()
        {
            int clampedLevel = Mathf.Clamp(level, 1, maxLevel);
            int actualSeed = useCustomSeed ? seed + clampedLevel : System.Environment.TickCount + clampedLevel;
            Random.InitState(actualSeed);

            placedRooms.Clear();
            corridorEntrances.Clear();
            corridorTiles.Clear();

            roomCount = Random.Range(5, 5 + clampedLevel);
            int firstWidth = Random.Range(minRoomSize.x, maxRoomSize.x + 1);
            int firstDepth = Random.Range(minRoomSize.y, maxRoomSize.y + 1);
            RectInt firstRect = new RectInt(0, 0, firstWidth, firstDepth);
            RoomData firstRoom = CreateRoom(transform, "Room_1", firstRect);
            placedRooms.Add(firstRoom);

            TeleportPlayerToRoom(firstRoom);

            bool connectedToFirst = false;
            for (int attempt = 0; attempt < 20 && !connectedToFirst; attempt++)
            {
                Direction sideDirection = Random.value > 0.5f ? Direction.East : Direction.West;
                int width = Random.Range(minRoomSize.x, maxRoomSize.x + 1);
                int depth = Random.Range(minRoomSize.y, maxRoomSize.y + 1);

                RectInt candidateRect = GetAdjacentRect(firstRoom.rect, sideDirection, width, depth);
                if (DoesOverlap(candidateRect))
                    continue;

                RoomData newRoom = CreateRoom(transform, "Room_2", candidateRect);
                placedRooms.Add(newRoom);
                CreateCorridor(transform, firstRoom.center, newRoom.center);
                connectedToFirst = true;
            }

            for (int i = 2; i < roomCount; i++)
            {
                bool placed = false;
                for (int attempt = 0; attempt < 20 && !placed; attempt++)
                {
                    List<RoomData> availableRooms = new List<RoomData>();
                    for (int j = 1; j < placedRooms.Count; j++)
                    {
                        availableRooms.Add(placedRooms[j]);
                    }

                    if (availableRooms.Count == 0)
                        continue;

                    RoomData baseRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                    Direction direction = (Direction)Random.Range(0, 4);
                    int width = Random.Range(minRoomSize.x, maxRoomSize.x + 1);
                    int depth = Random.Range(minRoomSize.y, maxRoomSize.y + 1);

                    RectInt candidateRect = GetAdjacentRect(baseRoom.rect, direction, width, depth);
                    if (DoesOverlap(candidateRect))
                        continue;

                    RoomData newRoom = CreateRoom(transform, $"Room_{i + 1}", candidateRect);
                    placedRooms.Add(newRoom);
                    CreateCorridor(transform, baseRoom.center, newRoom.center);
                    placed = true;
                }
            }

            PlaceWalls(transform);
        }

        private RoomData CreateRoom(Transform parent, string name, RectInt rect)
        {
            GameObject roomRoot = new GameObject(name);
            roomRoot.transform.parent = parent;

            for (int x = rect.x; x < rect.x + rect.width; x++)
            {
                for (int z = rect.y; z < rect.y + rect.height; z++)
                {
                    Vector3 position = new Vector3(x * tileSize.x, 0f, z * tileSize.z);
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, roomRoot.transform);

                    if (tile.GetComponent<Collider>() == null)
                    {
                        BoxCollider bc = tile.AddComponent<BoxCollider>();
                        bc.size = tileSize;
                    }
                }
            }

            Vector3 roomMinWorld = new Vector3(rect.x * tileSize.x, 0f, rect.y * tileSize.z);
            Vector3 roomMaxWorld = new Vector3((rect.x + rect.width) * tileSize.x, 0f, (rect.y + rect.height) * tileSize.z);
            Vector3 roomCenter = (roomMinWorld + roomMaxWorld) * 0.5f;
            Vector3 roomSize = roomMaxWorld - roomMinWorld;
            roomSize.y = tileSize.y * 0.5f;

            GameObject floorObj = new GameObject("Floor");
            floorObj.transform.parent = roomRoot.transform;
            floorObj.transform.position = roomCenter;
            BoxCollider collider = floorObj.AddComponent<BoxCollider>();
            collider.size = roomSize;

            Vector3 center = GetRoomCenter(rect);
            return new RoomData(rect, center, roomRoot.transform);
        }

        private void CreateCorridor(Transform parent, Vector3 startCenter, Vector3 endCenter)
        {
            int startTileX = Mathf.RoundToInt(startCenter.x / tileSize.x);
            int startTileZ = Mathf.RoundToInt(startCenter.z / tileSize.z);
            int endTileX = Mathf.RoundToInt(endCenter.x / tileSize.x);
            int endTileZ = Mathf.RoundToInt(endCenter.z / tileSize.z);

            int stepsX = endTileX - startTileX;
            int stepsZ = endTileZ - startTileZ;
            int signX = stepsX >= 0 ? 1 : -1;
            int signZ = stepsZ >= 0 ? 1 : -1;
            stepsX = Mathf.Abs(stepsX);
            stepsZ = Mathf.Abs(stepsZ);

            int currentTileX = startTileX;
            int currentTileZ = startTileZ;

            Vector3? firstEntrance = null;
            Vector3? lastEntrance = null;

            for (int i = 0; i <= stepsX; i++)
            {
                Vector3 position = new Vector3(currentTileX * tileSize.x, 0f, currentTileZ * tileSize.z);
                if (!IsTileInsideRoom(position))
                {
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, parent);
                    if (tile.GetComponent<Collider>() == null)
                    {
                        BoxCollider bc = tile.AddComponent<BoxCollider>();
                        bc.size = tileSize;
                    }

                    corridorTiles.Add(position);
                    if (!firstEntrance.HasValue)
                        firstEntrance = position;
                }
                currentTileX += signX;
            }

            for (int i = 0; i <= stepsZ; i++)
            {
                Vector3 position = new Vector3(endTileX * tileSize.x, 0f, currentTileZ * tileSize.z);
                if (!IsTileInsideRoom(position))
                {
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, parent);
                    if (tile.GetComponent<Collider>() == null)
                    {
                        BoxCollider bc = tile.AddComponent<BoxCollider>();
                        bc.size = tileSize;
                    }

                    corridorTiles.Add(position);
                    lastEntrance = position;
                }
                currentTileZ += signZ;
            }

            if (firstEntrance.HasValue)
                corridorEntrances.Add(firstEntrance.Value);
            if (lastEntrance.HasValue)
                corridorEntrances.Add(lastEntrance.Value);

            if (corridorTiles.Count > 0)
            {
                Vector3 minPos = Vector3.zero;
                Vector3 maxPos = Vector3.zero;
                bool firstPos = true;

                foreach (Vector3 pos in corridorTiles)
                {
                    if (firstPos)
                    {
                        minPos = maxPos = pos;
                        firstPos = false;
                        continue;
                    }

                    minPos = Vector3.Min(minPos, pos);
                    maxPos = Vector3.Max(maxPos, pos);
                }

                Vector3 colliderCenter = (minPos + maxPos) * 0.5f;
                Vector3 colliderSize = maxPos - minPos + tileSize;
                colliderSize.y = tileSize.y * 0.5f;

                GameObject floor = new GameObject("CorridorFloor");
                floor.transform.parent = parent;
                floor.transform.position = colliderCenter;

                BoxCollider bc = floor.AddComponent<BoxCollider>();
                bc.size = colliderSize;
            }
        }

        private bool IsTileInsideRoom(Vector3 position)
        {
            foreach (RoomData room in placedRooms)
            {
                if (IsTileInRect(position, room.rect))
                    return true;
            }

            return false;
        }

        private bool IsTileInRect(Vector3 position, RectInt rect)
        {
            int tileX = Mathf.RoundToInt(position.x / tileSize.x);
            int tileZ = Mathf.RoundToInt(position.z / tileSize.z);

            return tileX >= rect.x && tileX < rect.x + rect.width &&
                   tileZ >= rect.y && tileZ < rect.y + rect.height;
        }

        private bool DoesOverlap(RectInt rect)
        {
            foreach (RoomData room in placedRooms)
            {
                if (rect.Overlaps(room.rect))
                    return true;
            }

            return false;
        }

        private RectInt GetAdjacentRect(RectInt baseRect, Direction direction, int width, int depth)
        {
            switch (direction)
            {
                case Direction.East:
                    return new RectInt(
                        baseRect.xMax + roomSpacing,
                        baseRect.y + Mathf.FloorToInt((baseRect.height - depth) * 0.5f),
                        width,
                        depth);
                case Direction.West:
                    return new RectInt(
                        baseRect.xMin - roomSpacing - width,
                        baseRect.y + Mathf.FloorToInt((baseRect.height - depth) * 0.5f),
                        width,
                        depth);
                case Direction.North:
                    return new RectInt(
                        baseRect.x + Mathf.FloorToInt((baseRect.width - width) * 0.5f),
                        baseRect.yMax + roomSpacing,
                        width,
                        depth);
                case Direction.South:
                    return new RectInt(
                        baseRect.x + Mathf.FloorToInt((baseRect.width - width) * 0.5f),
                        baseRect.yMin - roomSpacing - depth,
                        width,
                        depth);
                default:
                    return baseRect;
            }
        }

        private Vector3 GetRoomCenter(RectInt rect)
        {
            int centerTileX = rect.x + rect.width / 2;
            int centerTileZ = rect.y + rect.height / 2;
            return new Vector3(centerTileX * tileSize.x, 0f, centerTileZ * tileSize.z);
        }

        private void TeleportPlayerToRoom(RoomData room)
        {
            GameObject player = GameObject.FindWithTag("Player");

                Vector3 spawnPos = room.center;
                spawnPos.y = tileSize.y;
                player.transform.position = spawnPos;
            
        }

        private void PlaceWalls(Transform parent)
        {
            HashSet<Vector3> rightWallPositions = new HashSet<Vector3>();
            HashSet<Vector3> upWallPositions = new HashSet<Vector3>();
            HashSet<Vector3> leftWallPositions = new HashSet<Vector3>();
            HashSet<Vector3> downWallPositions = new HashSet<Vector3>();

            foreach (RoomData room in placedRooms)
            {
                for (int x = room.rect.x; x < room.rect.x + room.rect.width; x++)
                {
                    for (int z = room.rect.y; z < room.rect.y + room.rect.height; z++)
                    {
                        Vector3 tilePos = new Vector3(x * tileSize.x, 0f, z * tileSize.z);

                        Vector3 rightPos = tilePos + new Vector3(tileSize.x, 0f, 0f);
                        Vector3 upPos = tilePos + new Vector3(0f, 0f, tileSize.z);
                        Vector3 leftPos = tilePos + new Vector3(-tileSize.x, 0f, 0f);
                        Vector3 downPos = tilePos + new Vector3(0f, 0f, -tileSize.z);

                        if (!IsTileInsideRoom(rightPos) && !corridorEntrances.Contains(rightPos) && !corridorTiles.Contains(rightPos))
                            rightWallPositions.Add(rightPos);
                        if (!IsTileInsideRoom(upPos) && !corridorEntrances.Contains(upPos) && !corridorTiles.Contains(upPos))
                            upWallPositions.Add(upPos);
                        if (!IsTileInsideRoom(leftPos) && !corridorEntrances.Contains(leftPos) && !corridorTiles.Contains(leftPos))
                            leftWallPositions.Add(leftPos);
                        if (!IsTileInsideRoom(downPos) && !corridorEntrances.Contains(downPos) && !corridorTiles.Contains(downPos))
                            downWallPositions.Add(downPos);
                    }
                }
            }

            foreach (Vector3 corridorPos in corridorTiles)
            {
                Vector3 rightPos = corridorPos + new Vector3(tileSize.x, 0f, 0f);
                Vector3 upPos = corridorPos + new Vector3(0f, 0f, tileSize.z);
                Vector3 leftPos = corridorPos + new Vector3(-tileSize.x, 0f, 0f);
                Vector3 downPos = corridorPos + new Vector3(0f, 0f, -tileSize.z);

                bool hasHorizontalNeighbor = corridorTiles.Contains(leftPos) || corridorTiles.Contains(rightPos);
                bool hasVerticalNeighbor = corridorTiles.Contains(upPos) || corridorTiles.Contains(downPos);

                if (hasHorizontalNeighbor && !hasVerticalNeighbor)
                {
                    if (!IsTileInsideRoom(upPos) && !corridorTiles.Contains(upPos))
                        upWallPositions.Add(upPos);
                    if (!IsTileInsideRoom(downPos) && !corridorTiles.Contains(downPos))
                        downWallPositions.Add(downPos);
                }
                else if (hasVerticalNeighbor && !hasHorizontalNeighbor)
                {
                    if (!IsTileInsideRoom(leftPos) && !corridorTiles.Contains(leftPos))
                        leftWallPositions.Add(leftPos);
                    if (!IsTileInsideRoom(rightPos) && !corridorTiles.Contains(rightPos))
                        rightWallPositions.Add(rightPos);
                }
                else
                {
                    if (!IsTileInsideRoom(rightPos) && !corridorTiles.Contains(rightPos))
                        rightWallPositions.Add(rightPos);
                    if (!IsTileInsideRoom(upPos) && !corridorTiles.Contains(upPos))
                        upWallPositions.Add(upPos);
                    if (!IsTileInsideRoom(leftPos) && !corridorTiles.Contains(leftPos))
                        leftWallPositions.Add(leftPos);
                    if (!IsTileInsideRoom(downPos) && !corridorTiles.Contains(downPos))
                        downWallPositions.Add(downPos);
                }
            }

            Vector3 rightOffset = new Vector3(tileSize.x, 0f, 0f);
            Vector3 upOffset = new Vector3(0f, 0f, tileSize.z);
            Vector3 leftOffset = new Vector3(-tileSize.x, 0f, 0f);
            Vector3 downOffset = new Vector3(0f, 0f, -tileSize.z);

            HashSet<Vector3> cornerRightUp = new HashSet<Vector3>();
            foreach (Vector3 rightPos in rightWallPositions)
            {
                Vector3 upCandidate = rightPos + leftOffset + upOffset;
                if (upWallPositions.Contains(upCandidate))
                    cornerRightUp.Add(rightPos + upOffset);
            }

            HashSet<Vector3> cornerUpLeft = new HashSet<Vector3>();
            foreach (Vector3 leftPos in leftWallPositions)
            {
                Vector3 upCandidate = leftPos + upOffset;
                if (upWallPositions.Contains(upCandidate))
                    cornerUpLeft.Add(leftPos + upOffset);
            }

            HashSet<Vector3> cornerLeftDown = new HashSet<Vector3>();
            foreach (Vector3 leftPos in leftWallPositions)
            {
                Vector3 downCandidate = leftPos + downOffset;
                if (downWallPositions.Contains(downCandidate))
                    cornerLeftDown.Add(leftPos);
            }

            HashSet<Vector3> cornerDownRight = new HashSet<Vector3>();
            foreach (Vector3 rightPos in rightWallPositions)
            {
                Vector3 downCandidate = rightPos + leftOffset;
                if (downWallPositions.Contains(downCandidate))
                    cornerDownRight.Add(rightPos);
            }

            rightWallPositions.ExceptWith(cornerRightUp);
            upWallPositions.ExceptWith(cornerRightUp);
            upWallPositions.ExceptWith(cornerUpLeft);
            leftWallPositions.ExceptWith(cornerUpLeft);
            leftWallPositions.ExceptWith(cornerLeftDown);
            downWallPositions.ExceptWith(cornerLeftDown);
            downWallPositions.ExceptWith(cornerDownRight);
            rightWallPositions.ExceptWith(cornerDownRight);

            foreach (Vector3 cornerPos in cornerRightUp)
            {
                    Instantiate(cornerPrefabRightUp, cornerPos, cornerPrefabRightUp.transform.rotation, parent);

            }

            foreach (Vector3 cornerPos in cornerUpLeft)
            {
                    Instantiate(cornerPrefabUpLeft, cornerPos, cornerPrefabUpLeft.transform.rotation, parent);

            }

            foreach (Vector3 cornerPos in cornerLeftDown)
            {
                    Instantiate(cornerPrefabLeftDown, cornerPos, cornerPrefabLeftDown.transform.rotation, parent);

            }

            foreach (Vector3 cornerPos in cornerDownRight)
            {
                    Instantiate(cornerPrefabDownRight, cornerPos, cornerPrefabDownRight.transform.rotation, parent);

            }

            foreach (Vector3 wallPos in rightWallPositions)
            {
                    Instantiate(wallPrefabRight, wallPos, wallPrefabRight.transform.rotation, parent);

            }

            foreach (Vector3 wallPos in upWallPositions)
            {
                    Instantiate(wallPrefabUp, wallPos, wallPrefabUp.transform.rotation, parent);

            }

            foreach (Vector3 wallPos in leftWallPositions)
            {
                    Instantiate(wallPrefabLeft, wallPos, wallPrefabLeft.transform.rotation, parent);

            }

            foreach (Vector3 wallPos in downWallPositions)
            {
                Instantiate(wallPrefabDown, wallPos, wallPrefabDown.transform.rotation, parent);
            }
        }

        private Vector3 GetTileSize(GameObject prefab)
        {
            GameObject temp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            temp.hideFlags = HideFlags.HideAndDontSave;

            Bounds bounds = new Bounds(temp.transform.position, Vector3.zero);
            Renderer[] renderers = temp.GetComponentsInChildren<Renderer>();
            Collider[] colliders = temp.GetComponentsInChildren<Collider>();

            if (renderers.Length > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            else if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            Destroy(temp);

            return bounds.size;
        }
    }
}
