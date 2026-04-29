using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheBindingOfV1.Generation
{
    /// <summary>
    /// Represents a single node in the floor graph.
    /// Each node corresponds to one room in the generated floor layout.
    /// Connections between nodes define the corridors that will be built by A*.
    /// </summary>
    public class RoomNode
    {
        public RoomType roomType;
        public List<RoomNode> connections = new List<RoomNode>();
        public PlacedRoom placedRoom; // filled later by RoomPlacer
    }

    /// <summary>
    /// Generates the connection graph for a single floor of the roguelike.
    ///
    /// The generation process runs in three steps:
    ///
    /// 1. Node creation — instantiates one RoomNode per room type
    ///    (Start, 4x Combat, MiniBoss, Shop, ItemRoom, Boss).
    ///
    /// 2. Spanning tree — builds a guaranteed connected graph.
    ///    The main path goes: Start → Combat rooms → Boss.
    ///    Optional rooms (MiniBoss, Shop, ItemRoom) are attached as side
    ///    branches off random combat rooms. This ensures every room is
    ///    reachable from Start while keeping side content skippable.
    ///
    /// 3. Extra edges — adds 1-2 additional connections between existing
    ///    nodes to create crossroads and loops, making the layout less
    ///    linear and more interesting to explore.
    ///
    /// Constraints:
    /// - Each room has at most <see cref="MAX_EXITS_PER_ROOM"/> exit connections.
    /// - The Boss room is always reachable from Start.
    /// - Side rooms (MiniBoss, Shop, ItemRoom) may be skipped by the player.
    ///
    /// The resulting graph is then passed to <see cref="RoomPlacer"/> which
    /// assigns a 3D position to each node, and subsequently to
    /// <see cref="AStarPathfinder"/> which routes corridors between them.
    /// </summary>
    public class GraphGenerator
    {
        // ── Constants ────────────────────────────────────────────────

        /// <summary>Number of combat rooms generated per floor.</summary>
        private const int COMBAT_ROOM_COUNT = 4;

        /// <summary>
        /// Maximum number of exit connections a single room can have.
        /// Keeps the graph readable and prevents rooms with too many corridors.
        /// </summary>
        private const int MAX_EXITS_PER_ROOM = 2;

        // ── Entry point ──────────────────────────────────────────────

        /// <summary>
        /// Generates and returns the full room graph for one floor.
        /// The returned list contains all <see cref="RoomNode"/> instances
        /// with their connections already established.
        /// </summary>
        /// <returns>
        /// A list of RoomNodes representing the floor layout.
        /// The Start node is always at index 0.
        /// The Boss node is always the last element.
        /// </returns>
        public List<RoomNode> Generate()
        {
            List<RoomNode> nodes = CreateNodes();
            BuildSpanningTree(nodes);
            AddExtraEdges(nodes);
            return nodes;
        }

        // ── Step 1 — Create all nodes ────────────────────────────────

        /// <summary>
        /// Instantiates one RoomNode per room in the floor.
        /// Order: Start, Combat x4, MiniBoss, Shop, ItemRoom, Boss.
        /// </summary>
        private List<RoomNode> CreateNodes()
        {
            List<RoomNode> nodes = new List<RoomNode>();

            nodes.Add(new RoomNode { roomType = RoomType.Start });

            for (int i = 0; i < COMBAT_ROOM_COUNT; i++)
                nodes.Add(new RoomNode { roomType = RoomType.Combat });

            nodes.Add(new RoomNode { roomType = RoomType.MiniBoss });
            nodes.Add(new RoomNode { roomType = RoomType.Shop });
            nodes.Add(new RoomNode { roomType = RoomType.ItemRoom });
            nodes.Add(new RoomNode { roomType = RoomType.Boss });

            return nodes;
        }

        // ── Step 2 — Build spanning tree ─────────────────────────────

        /// <summary>
        /// Builds a spanning tree that guarantees full connectivity.
        ///
        /// Main path: Start → shuffled Combat rooms → Boss.
        /// Side branches: MiniBoss, Shop and ItemRoom are each attached
        /// to a random combat room that still has available exit slots.
        ///
        /// If no combat room has available slots for a side room,
        /// that side room is left unconnected (should not happen with
        /// default counts but is handled gracefully).
        /// </summary>
        private void BuildSpanningTree(List<RoomNode> nodes)
        {
            RoomNode start = nodes.First(n => n.roomType == RoomType.Start);
            RoomNode boss = nodes.Last(n => n.roomType == RoomType.Boss);

            List<RoomNode> combatRooms = nodes
                .Where(n => n.roomType == RoomType.Combat)
                .ToList();

            List<RoomNode> sideRooms = nodes
                .Where(n => n.roomType == RoomType.MiniBoss
                         || n.roomType == RoomType.Shop
                         || n.roomType == RoomType.ItemRoom)
                .ToList();

            // Shuffle combat rooms for layout variety across runs
            combatRooms = combatRooms.OrderBy(_ => Random.value).ToList();

            // Build main path: Start → Combat 0 → Combat 1 → ... → Boss
            Connect(start, combatRooms[0]);
            for (int i = 0; i < combatRooms.Count - 1; i++)
                Connect(combatRooms[i], combatRooms[i + 1]);
            Connect(combatRooms[combatRooms.Count - 1], boss);

            // Attach each side room to a random combat room with available slots
            List<RoomNode> shuffledSideRooms = sideRooms
                .OrderBy(_ => Random.value).ToList();

            foreach (RoomNode side in shuffledSideRooms)
            {
                List<RoomNode> available = combatRooms
                    .Where(c => c.connections.Count < MAX_EXITS_PER_ROOM)
                    .ToList();

                if (available.Count == 0) break;

                RoomNode parent = available[Random.Range(0, available.Count)];
                Connect(parent, side);
            }
        }

        // ── Step 3 — Add extra edges ──────────────────────────────────

        /// <summary>
        /// Adds 1-2 additional random connections to the graph.
        /// These extra edges create crossroads and loops, making the
        /// layout more interesting to explore and less strictly linear.
        ///
        /// Only nodes with available exit slots are eligible.
        /// The Start node is excluded to avoid making the start room
        /// a hub with too many exits.
        /// </summary>
        private void AddExtraEdges(List<RoomNode> nodes)
        {
            int extraEdges = Random.Range(1, 3);

            for (int i = 0; i < extraEdges; i++)
            {
                List<RoomNode> candidates = nodes
                    .Where(n => n.roomType != RoomType.Start
                             && n.connections.Count < MAX_EXITS_PER_ROOM)
                    .ToList();

                if (candidates.Count < 2) break;

                RoomNode a = candidates[Random.Range(0, candidates.Count)];
                List<RoomNode> bCandidates = candidates
                    .Where(n => n != a && !a.connections.Contains(n))
                    .ToList();

                if (bCandidates.Count == 0) continue;

                RoomNode b = bCandidates[Random.Range(0, bCandidates.Count)];
                Connect(a, b);
            }
        }

        // ── Helper ────────────────────────────────────────────────────

        /// <summary>
        /// Creates a bidirectional connection between two nodes.
        /// Does nothing if the connection already exists.
        /// </summary>
        private void Connect(RoomNode a, RoomNode b)
        {
            if (!a.connections.Contains(b)) a.connections.Add(b);
            if (!b.connections.Contains(a)) b.connections.Add(a);
        }
    }
}
