using System.Collections.Generic;
using UnityEngine;

namespace TheBindingOfV1.Generation
{
    /// <summary>
    /// Assembles corridor prefab segments along an A* path.
    ///
    /// Given a list of <see cref="AStarNode"/> returned by <see cref="AStarPathfinder"/>,
    /// this class determines the correct prefab for each cell based on the incoming
    /// and outgoing directions, loads it from an <see cref="AssetBundle"/>, and
    /// instantiates it at the correct world position and rotation.
    ///
    /// Prefab naming convention (must match names in the AssetBundle):
    /// - Corridor_Straight              — straight horizontal segment
    /// - Corridor_Corner_NE/NW/SE/SW   — 90-degree horizontal turns
    /// - Corridor_Transition_Vent       — palier 0 → +1
    /// - Corridor_Transition_WallJump   — palier +1 → +2
    /// - Corridor_Transition_JumpPad    — palier +2 → +3
    /// - Corridor_Transition_FreeFall   — palier 0 → -1, -1 → -2
    /// - Corridor_Transition_Elevator   — palier -2 → -3
    /// - Corridor_Transition_DirectFall — direct shaft between stacked rooms
    /// - Corridor_Doorway               — first and last cell of every path
    ///
    /// All prefabs must be tagged to the same AssetBundle in Rude and built
    /// before being passed to this class. See the AssetBundle setup guide in
    /// the project documentation.
    /// </summary>
    public class CorridorInstancer
    {
        // ── Prefab names ──────────────────────────────────────────────

        private const string PREFAB_STRAIGHT    = "Corridor_Straight";
        private const string PREFAB_CORNER_NE   = "Corridor_Corner_NE";
        private const string PREFAB_CORNER_NW   = "Corridor_Corner_NW";
        private const string PREFAB_CORNER_SE   = "Corridor_Corner_SE";
        private const string PREFAB_CORNER_SW   = "Corridor_Corner_SW";
        private const string PREFAB_VENT        = "Corridor_Transition_Vent";
        private const string PREFAB_WALLJUMP    = "Corridor_Transition_WallJump";
        private const string PREFAB_JUMPPAD     = "Corridor_Transition_JumpPad";
        private const string PREFAB_FREEFALL    = "Corridor_Transition_FreeFall";
        private const string PREFAB_ELEVATOR    = "Corridor_Transition_Elevator";
        private const string PREFAB_DIRECTFALL  = "Corridor_Transition_DirectFall";
        private const string PREFAB_DOORWAY     = "Corridor_Doorway";

        // ── Internal state ────────────────────────────────────────────

        private readonly AssetBundle _bundle;
        private readonly NavigationGrid _grid;

        /// <summary>Cache to avoid loading the same prefab multiple times.</summary>
        private readonly Dictionary<string, GameObject> _prefabCache
            = new Dictionary<string, GameObject>();

        // ── Constructor ───────────────────────────────────────────────

        /// <summary>
        /// Creates a new CorridorInstancer using the given AssetBundle and grid.
        /// </summary>
        /// <param name="bundle">
        /// The AssetBundle containing all corridor prefabs.
        /// Load it via AssetBundle.LoadFromStream with the embedded resource.
        /// </param>
        /// <param name="grid">
        /// The NavigationGrid of the current floor, used to convert grid
        /// coordinates back to world positions.
        /// </param>
        public CorridorInstancer(AssetBundle bundle, NavigationGrid grid)
        {
            _bundle = bundle;
            _grid   = grid;
        }

        // ── Entry point ───────────────────────────────────────────────

        /// <summary>
        /// Instantiates all corridor segments along the given A* path.
        ///
        /// For each cell in the path, determines the correct prefab based on
        /// the incoming and outgoing directions, then instantiates it at the
        /// correct world position and rotation.
        ///
        /// The first and last cells always use the Doorway prefab to connect
        /// cleanly to room entrance and exit door anchors.
        /// </summary>
        /// <param name="path">
        /// The ordered list of <see cref="AStarNode"/> returned by
        /// <see cref="AStarPathfinder.FindPath"/>.
        /// </param>
        /// <returns>
        /// A list of all instantiated corridor GameObjects.
        /// Returns an empty list if the path is null or has fewer than 2 nodes.
        /// </returns>
        public List<GameObject> InstantiateCorridor(List<AStarNode> path)
        {
            List<GameObject> instances = new List<GameObject>();

            if (path == null || path.Count < 2)
            {
                Debug.LogWarning("[CorridorInstancer] Path is null or too short.");
                return instances;
            }

            for (int i = 0; i < path.Count; i++)
            {
                // Determine incoming and outgoing directions
                Vector3Int? incoming = i > 0
                    ? (Vector3Int?)(path[i].gridPosition - path[i - 1].gridPosition)
                    : null;

                Vector3Int? outgoing = i < path.Count - 1
                    ? (Vector3Int?)(path[i + 1].gridPosition - path[i].gridPosition)
                    : null;

                // Select the correct prefab
                string prefabName = SelectPrefab(i, path.Count, incoming, outgoing);

                // Load and instantiate
                GameObject prefab = LoadPrefab(prefabName);
                if (prefab == null) continue;

                Vector3 worldPos  = GridToWorldPosition(path[i]);
                Quaternion rotation = GetRotation(incoming, outgoing);

                GameObject instance = Object.Instantiate(prefab, worldPos, rotation);
                instance.name = $"Corridor_{prefabName}_{i}";
                instances.Add(instance);
            }

            return instances;
        }

        // ── Prefab selection ──────────────────────────────────────────

        /// <summary>
        /// Selects the correct prefab name for a cell based on its position
        /// in the path and the incoming/outgoing directions.
        /// </summary>
        private string SelectPrefab(
            int index,
            int pathCount,
            Vector3Int? incoming,
            Vector3Int? outgoing)
        {
            // First and last cells always use Doorway
            if (index == 0 || index == pathCount - 1)
                return PREFAB_DOORWAY;

            // Vertical transition — incoming or outgoing changes palier (Y)
            if (incoming.HasValue && incoming.Value.y != 0)
                return SelectVerticalPrefab(incoming.Value, outgoing);

            if (outgoing.HasValue && outgoing.Value.y != 0)
                return SelectVerticalPrefab(incoming, outgoing.Value);

            // Horizontal — both directions are on the same palier
            return SelectHorizontalPrefab(incoming.Value, outgoing.Value);
        }

        /// <summary>
        /// Selects the correct horizontal segment prefab (Straight or Corner)
        /// based on incoming and outgoing directions on the same palier.
        /// </summary>
        private string SelectHorizontalPrefab(Vector3Int incoming, Vector3Int outgoing)
        {
            // Straight — same direction
            if (incoming == outgoing)
                return PREFAB_STRAIGHT;

            // Corner — determine which turn
            // incoming is where we came FROM, outgoing is where we go TO
            // North = +Z, South = -Z, East = +X, West = -X

            // Coming from West (moving East), turning North
            if (incoming.x == 1 && outgoing.z == 1)  return PREFAB_CORNER_NE;
            // Coming from South (moving North), turning East
            if (incoming.z == 1 && outgoing.x == 1)  return PREFAB_CORNER_NE;

            // Coming from West (moving East), turning South
            if (incoming.x == 1 && outgoing.z == -1) return PREFAB_CORNER_SE;
            // Coming from North (moving South), turning East
            if (incoming.z == -1 && outgoing.x == 1) return PREFAB_CORNER_SE;

            // Coming from East (moving West), turning North
            if (incoming.x == -1 && outgoing.z == 1)  return PREFAB_CORNER_NW;
            // Coming from South (moving North), turning West
            if (incoming.z == 1 && outgoing.x == -1)  return PREFAB_CORNER_NW;

            // Coming from East (moving West), turning South
            if (incoming.x == -1 && outgoing.z == -1) return PREFAB_CORNER_SW;
            // Coming from North (moving South), turning West
            if (incoming.z == -1 && outgoing.x == -1) return PREFAB_CORNER_SW;

            // Fallback
            Debug.LogWarning($"[CorridorInstancer] Unknown corner: in={incoming} out={outgoing}");
            return PREFAB_STRAIGHT;
        }

        /// <summary>
        /// Selects the correct vertical transition prefab based on the palier
        /// change direction and the palier indices involved.
        /// </summary>
        private string SelectVerticalPrefab(Vector3Int? incoming, Vector3Int? outgoing)
        {
            // Determine the direction of the vertical movement
            int dy = incoming.HasValue && incoming.Value.y != 0
                ? incoming.Value.y
                : outgoing.Value.y;

            // Determine which palier index we are transitioning from
            // (This would need the current node's palier index — passed via context)
            // For now, we use the magnitude of dy to detect direct fall
            if (Mathf.Abs(dy) >= 2)
                return PREFAB_DIRECTFALL;

            // Adjacent palier transitions — going up
            if (dy > 0)
            {
                // We need the actual palier to know which transition type to use.
                // This is resolved by the caller passing palier context.
                // Default to Vent as placeholder — override in InstantiateCorridorWithContext.
                return PREFAB_VENT;
            }

            // Adjacent palier transitions — going down
            return PREFAB_FREEFALL;
        }

        // ── World position & rotation ─────────────────────────────────

        /// <summary>
        /// Converts a grid node to its world position.
        /// Uses the NavigationGrid coordinate helpers and the FloorHeights table.
        /// </summary>
        private Vector3 GridToWorldPosition(AStarNode node)
        {
            float worldX = _grid.GridToWorldX(node.gridPosition.x);
            float worldZ = _grid.GridToWorldZ(node.gridPosition.z);
            float worldY = NavigationGrid.FloorHeights[
                NavigationGrid.IndexToFloor(node.gridPosition.y)];

            return new Vector3(worldX, worldY, worldZ);
        }

        /// <summary>
        /// Computes the rotation of a corridor segment based on its direction.
        /// Corridor prefabs are assumed to face +Z by default (North).
        /// </summary>
        private Quaternion GetRotation(Vector3Int? incoming, Vector3Int? outgoing)
        {
            // Use outgoing direction to orient the segment
            Vector3Int dir = outgoing ?? (incoming.HasValue ? incoming.Value : Vector3Int.forward);

            // Ignore Y for rotation — vertical transitions use default rotation
            Vector3 flatDir = new Vector3(dir.x, 0, dir.z);
            if (flatDir == Vector3.zero)
                return Quaternion.identity;

            return Quaternion.LookRotation(flatDir);
        }

        // ── Asset loading ─────────────────────────────────────────────

        /// <summary>
        /// Loads a prefab from the AssetBundle by name.
        /// Results are cached to avoid redundant bundle lookups.
        /// Returns null and logs a warning if the prefab is not found.
        /// </summary>
        private GameObject LoadPrefab(string name)
        {
            if (_prefabCache.TryGetValue(name, out GameObject cached))
                return cached;

            GameObject prefab = _bundle.LoadAsset<GameObject>(name);

            if (prefab == null)
            {
                Debug.LogWarning($"[CorridorInstancer] Prefab not found in bundle: {name}");
                return null;
            }

            _prefabCache[name] = prefab;
            return prefab;
        }
    }
}