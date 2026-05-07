using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace TheBindingOfV1.Generation
{
    /// <summary>
    /// Orchestrates the full procedural generation pipeline for a single floor.
    ///
    /// The generation runs in five sequential steps:
    ///
    /// 1. <see cref="GenerateGraph"/>    — creates room nodes and connections
    /// 2. <see cref="PlaceRooms"/>       — assigns 3D positions to each node
    /// 3. <see cref="BuildGrid"/>        — constructs the A* navigation grid
    /// 4. <see cref="RouteCorridors"/>   — runs A* for each room connection
    /// 5. <see cref="InstantiateFloor"/> — loads prefabs and builds the scene
    ///
    /// Usage:
    /// <code>
    /// FloorGenerator generator = new FloorGenerator(bundle, roomPrefabs, floorIndex);
    /// yield return generator.GenerateAsync();
    /// </code>
    ///
    /// The generator runs asynchronously using a coroutine to avoid blocking
    /// the main thread during heavy operations (grid construction, A* runs).
    ///
    /// If generation fails (e.g. a room cannot be placed after
    /// <see cref="MAX_GENERATION_ATTEMPTS"/> retries), the generator logs an
    /// error and returns without building the floor.
    /// </summary>
    public class FloorGenerator
    {
        // ── Constants ─────────────────────────────────────────────────

        /// <summary>
        /// Maximum number of full generation attempts before giving up.
        /// A generation attempt may fail if rooms cannot be placed without
        /// overlap after all retries, or if A* cannot route all corridors.
        /// </summary>
        private const int MAX_GENERATION_ATTEMPTS = 10;

        /// <summary>Name of the embedded AssetBundle resource for corridor prefabs.</summary>
        private const string CORRIDOR_BUNDLE_RESOURCE = "TheBindingOfV1.corridors";

        /// <summary>Name of the embedded AssetBundle resource for room prefabs.</summary>
        private const string ROOM_BUNDLE_RESOURCE = "TheBindingOfV1.rooms";

        // ── Internal state ────────────────────────────────────────────

        private readonly int _floorIndex;
        private readonly int _seed;

        private AssetBundle _corridorBundle;
        private AssetBundle _roomBundle;

        private List<RoomNode> _graph;
        private List<PlacedRoom> _placedRooms;
        private NavigationGrid _grid;
        private List<List<AStarNode>> _corridorPaths;
        private List<GameObject> _instantiatedObjects = new List<GameObject>();

        // ── Constructor ───────────────────────────────────────────────

        /// <summary>
        /// Creates a new FloorGenerator for the given floor.
        /// </summary>
        /// <param name="floorIndex">
        /// The floor number (1, 2, or 3). Used for logging and difficulty scaling.
        /// </param>
        /// <param name="seed">
        /// The random seed for this floor. Use the same seed to reproduce
        /// the same layout (useful for debugging).
        /// Pass -1 to use a random seed.
        /// </param>
        public FloorGenerator(int floorIndex, int seed = -1)
        {
            _floorIndex = floorIndex;
            _seed       = seed == -1 ? Random.Range(0, int.MaxValue) : seed;
        }

        // ── Entry point ───────────────────────────────────────────────

        /// <summary>
        /// Runs the full generation pipeline asynchronously.
        /// Use as a coroutine: <c>yield return StartCoroutine(generator.GenerateAsync())</c>
        ///
        /// Yields after each major step to avoid frame drops.
        /// If generation fails after <see cref="MAX_GENERATION_ATTEMPTS"/> attempts,
        /// logs an error and stops.
        /// </summary>
        public System.Collections.IEnumerator GenerateAsync()
        {
            Random.InitState(_seed);
            Debug.Log($"[FloorGenerator] Starting floor {_floorIndex} generation (seed: {_seed})");

            // Load asset bundles
            LoadBundles();
            yield return null;

            // Attempt generation up to MAX_GENERATION_ATTEMPTS times
            for (int attempt = 1; attempt <= MAX_GENERATION_ATTEMPTS; attempt++)
            {
                Debug.Log($"[FloorGenerator] Attempt {attempt}/{MAX_GENERATION_ATTEMPTS}");

                // Step 1 — Graph
                _graph = GenerateGraph();
                yield return null;

                // Step 2 — Room placement
                bool placementSuccess = PlaceRooms();
                if (!placementSuccess)
                {
                    Debug.LogWarning($"[FloorGenerator] Room placement failed on attempt {attempt}. Retrying...");
                    continue;
                }
                yield return null;

                // Step 3 — Navigation grid
                BuildGrid();
                yield return null;

                // Step 4 — Corridor routing
                bool routingSuccess = RouteCorridors();
                if (!routingSuccess)
                {
                    Debug.LogWarning($"[FloorGenerator] Corridor routing failed on attempt {attempt}. Retrying...");
                    CleanUp();
                    continue;
                }
                yield return null;

                // Step 5 — Instantiation
                InstantiateFloor();
                yield return null;

                Debug.Log($"[FloorGenerator] Floor {_floorIndex} generated successfully " +
                    $"(attempt {attempt}, seed {_seed})");
                yield break;
            }

            Debug.LogError($"[FloorGenerator] Failed to generate floor {_floorIndex} " +
                $"after {MAX_GENERATION_ATTEMPTS} attempts.");
        }

        // ── Step 1 — Graph generation ─────────────────────────────────

        /// <summary>
        /// Generates the room connection graph using <see cref="GraphGenerator"/>.
        /// </summary>
        private List<RoomNode> GenerateGraph()
        {
            GraphGenerator generator = new GraphGenerator();
            List<RoomNode> graph = generator.Generate();

            Debug.Log($"[FloorGenerator] Graph generated: {graph.Count} rooms, " +
                $"{graph.Sum(n => n.connections.Count) / 2} connections");

            return graph;
        }

        // ── Step 2 — Room placement ───────────────────────────────────

        /// <summary>
        /// Assigns 3D positions to all room nodes using <see cref="RoomPlacer"/>.
        ///
        /// Placement order:
        /// 1. Start room at origin
        /// 2. Boss room far from Start
        /// 3. All other rooms randomly between them
        ///
        /// Returns false if any room fails to place after all retries.
        /// </summary>
        private bool PlaceRooms()
        {
            _placedRooms = new List<PlacedRoom>();

            // Find Start and Boss nodes
            RoomNode startNode = _graph.First(n => n.roomType == RoomType.Start);
            RoomNode bossNode  = _graph.Last(n => n.roomType == RoomType.Boss);

            // Load room prefabs from bundle
            RoomData startPrefab = LoadRoomPrefab(RoomType.Start);
            RoomData bossPrefab  = LoadRoomPrefab(RoomType.Boss);

            if (startPrefab == null || bossPrefab == null)
            {
                Debug.LogError("[FloorGenerator] Failed to load Start or Boss prefab.");
                return false;
            }

            // Place Start
            PlacedRoom startRoom = RoomPlacer.PlaceStartRoom(startPrefab);
            startNode.placedRoom = startRoom;
            _placedRooms.Add(startRoom);

            // Place Boss
            PlacedRoom bossRoom = RoomPlacer.PlaceBossRoom(bossPrefab, _placedRooms);
            if (bossRoom == null)
            {
                Debug.LogWarning("[FloorGenerator] Failed to place Boss room.");
                return false;
            }
            bossNode.placedRoom = bossRoom;
            _placedRooms.Add(bossRoom);

            // Place all other rooms
            List<RoomNode> remainingNodes = _graph
                .Where(n => n.roomType != RoomType.Start && n.roomType != RoomType.Boss)
                .OrderBy(_ => Random.value) // randomize placement order
                .ToList();

            int[] availableFloors = { -3, -2, -1, 0, 1, 2, 3 };

            foreach (RoomNode node in remainingNodes)
            {
                RoomData prefab = LoadRoomPrefab(node.roomType);
                if (prefab == null)
                {
                    Debug.LogError($"[FloorGenerator] Failed to load prefab for {node.roomType}.");
                    return false;
                }

                int targetFloor = availableFloors[Random.Range(0, availableFloors.Length)];
                PlacedRoom placed = RoomPlacer.TryPlaceRoom(prefab, targetFloor, _placedRooms);

                if (placed == null)
                {
                    Debug.LogWarning($"[FloorGenerator] Failed to place {node.roomType} room.");
                    return false;
                }

                node.placedRoom = placed;
                _placedRooms.Add(placed);
            }

            Debug.Log($"[FloorGenerator] Placed {_placedRooms.Count} rooms.");
            return true;
        }

        // ── Step 3 — Navigation grid ──────────────────────────────────

        /// <summary>
        /// Builds the <see cref="NavigationGrid"/> from all placed rooms.
        /// </summary>
        private void BuildGrid()
        {
            _grid = new NavigationGrid(_placedRooms);
            Debug.Log($"[FloorGenerator] Navigation grid built.");
        }

        // ── Step 4 — Corridor routing ─────────────────────────────────

        /// <summary>
        /// Runs A* for each edge in the room graph to find corridor paths.
        ///
        /// For each connection between two rooms, A* routes from the exit door
        /// of the source room to the entrance door of the destination room.
        ///
        /// Returns false if any corridor path cannot be found.
        /// </summary>
        private bool RouteCorridors()
        {
            _corridorPaths = new List<List<AStarNode>>();
            AStarPathfinder pathfinder = new AStarPathfinder(_grid);

            // Track already-routed pairs to avoid duplicates
            HashSet<(RoomNode, RoomNode)> routedPairs = new HashSet<(RoomNode, RoomNode)>();

            foreach (RoomNode node in _graph)
            {
                foreach (RoomNode connected in node.connections)
                {
                    // Skip if already routed in the other direction
                    if (routedPairs.Contains((connected, node))) continue;
                    routedPairs.Add((node, connected));

                    if (node.placedRoom == null || connected.placedRoom == null)
                    {
                        Debug.LogWarning($"[FloorGenerator] Skipping corridor — " +
                            $"PlacedRoom missing for {node.roomType} or {connected.roomType}");
                        continue;
                    }

                    // Get start node from exit door of source room
                    AStarNode startNode = GetDoorNode(node.placedRoom, isDoor: false);

                    // Get end node from entrance door of destination room
                    AStarNode endNode = GetDoorNode(connected.placedRoom, isDoor: true);

                    if (startNode == null || endNode == null)
                    {
                        Debug.LogWarning($"[FloorGenerator] Could not find door nodes for " +
                            $"{node.roomType} → {connected.roomType}");
                        return false;
                    }

                    List<AStarNode> path = pathfinder.FindPath(startNode, endNode);

                    if (path == null)
                    {
                        Debug.LogWarning($"[FloorGenerator] No path found from " +
                            $"{node.roomType} to {connected.roomType}");
                        return false;
                    }

                    _corridorPaths.Add(path);
                    Debug.Log($"[FloorGenerator] Corridor routed: {node.roomType} → " +
                        $"{connected.roomType} ({path.Count} segments)");
                }
            }

            Debug.Log($"[FloorGenerator] Routed {_corridorPaths.Count} corridors.");
            return true;
        }

        // ── Step 5 — Instantiation ────────────────────────────────────

        /// <summary>
        /// Instantiates all room prefabs and corridor segments in the scene.
        /// </summary>
        private void InstantiateFloor()
        {
            CorridorInstancer instancer = new CorridorInstancer(_corridorBundle, _grid);

            // Instantiate rooms
            foreach (PlacedRoom placed in _placedRooms)
            {
                GameObject roomPrefab = _roomBundle.LoadAsset<GameObject>(
                    placed.room.roomType.ToString());

                if (roomPrefab == null)
                {
                    Debug.LogWarning($"[FloorGenerator] Room prefab not found: {placed.room.roomType}");
                    continue;
                }

                GameObject roomInstance = Object.Instantiate(
                    roomPrefab,
                    placed.position,
                    Quaternion.identity
                );

                roomInstance.name = $"Room_{placed.room.roomId}";
                _instantiatedObjects.Add(roomInstance);
            }

            // Instantiate corridors
            foreach (List<AStarNode> path in _corridorPaths)
            {
                List<GameObject> segments = instancer.InstantiateCorridor(path);
                _instantiatedObjects.AddRange(segments);
            }

            Debug.Log($"[FloorGenerator] Instantiated {_instantiatedObjects.Count} objects.");
        }

        // ── Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Loads the room prefab for a given room type from the room AssetBundle.
        /// Returns null if the prefab is not found.
        /// </summary>
        private RoomData LoadRoomPrefab(RoomType roomType)
        {
            GameObject prefab = _roomBundle.LoadAsset<GameObject>(roomType.ToString());
            if (prefab == null)
            {
                Debug.LogWarning($"[FloorGenerator] Room prefab not found in bundle: {roomType}");
                return null;
            }
            return prefab.GetComponent<RoomData>();
        }

        /// <summary>
        /// Converts a room's door anchor transform to the corresponding
        /// <see cref="AStarNode"/> in the navigation grid.
        /// </summary>
        /// <param name="room">The placed room.</param>
        /// <param name="isDoor">
        /// True to use the entrance door, false to use the first exit door.
        /// </param>
        private AStarNode GetDoorNode(PlacedRoom room, bool isDoor)
        {
            Transform door = isDoor
                ? room.room.entranceDoor
                : (room.room.exitDoors.Count > 0 ? room.room.exitDoors[0] : null);

            if (door == null)
            {
                Debug.LogWarning($"[FloorGenerator] Door anchor missing on {room.room.roomId}");
                return null;
            }

            // Offset position slightly outside the room so A* starts in free space
            Vector3 doorWorldPos = door.position + door.forward * NavigationGrid.CELL_SIZE;

            return _grid.WorldToNode(doorWorldPos, room.floor);
        }

        /// <summary>
        /// Loads both AssetBundles (rooms and corridors) from embedded resources.
        /// </summary>
        private void LoadBundles()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            System.IO.Stream corridorStream =
                assembly.GetManifestResourceStream(CORRIDOR_BUNDLE_RESOURCE);
            _corridorBundle = AssetBundle.LoadFromStream(corridorStream);

            System.IO.Stream roomStream =
                assembly.GetManifestResourceStream(ROOM_BUNDLE_RESOURCE);
            _roomBundle = AssetBundle.LoadFromStream(roomStream);

            if (_corridorBundle == null)
                Debug.LogError("[FloorGenerator] Failed to load corridor AssetBundle.");
            if (_roomBundle == null)
                Debug.LogError("[FloorGenerator] Failed to load room AssetBundle.");
        }

        /// <summary>
        /// Destroys all instantiated objects from a failed generation attempt.
        /// Called before retrying generation.
        /// </summary>
        private void CleanUp()
        {
            foreach (GameObject obj in _instantiatedObjects)
            {
                if (obj != null) Object.Destroy(obj);
            }
            _instantiatedObjects.Clear();
            _placedRooms = null;
            _grid = null;
            _corridorPaths = null;
        }

        // ── Public accessors ──────────────────────────────────────────

        /// <summary>The random seed used for this floor's generation.</summary>
        public int Seed => _seed;

        /// <summary>All placed rooms after a successful generation.</summary>
        public List<PlacedRoom> PlacedRooms => _placedRooms;

        /// <summary>All instantiated GameObjects (rooms + corridors).</summary>
        public List<GameObject> InstantiatedObjects => _instantiatedObjects;
    }
}