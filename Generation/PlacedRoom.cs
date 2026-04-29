using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheBindingOfV1.Generation
{
    public class PlacedRoom
    {
        public RoomData room;
        public Vector3 position;
        public int floor;
        public Bounds bounds;

        // Stacking relationships
        public List<PlacedRoom> stackedAbove = new List<PlacedRoom>();
        public List<PlacedRoom> stackedBelow = new List<PlacedRoom>();

        // Graph connections (filled by GraphGenerator)
        public List<PlacedRoom> connectedRooms = new List<PlacedRoom>();
    }
}
