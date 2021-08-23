using System;

namespace ImageBank
{
    public class Node
    {
        public int NodeId { get; }
        public int Radius { get; }
        public ulong[] Core { get; }
        public int ChildId { get; }

        public Node(
            int nodeid,
            ulong[] core,
            int radius,
            int childid
            )
        {
            NodeId = nodeid;
            Core = core;
            Radius = radius;
            ChildId = childid;
        }

        public Node(
            int nodeid
            )
        {
            NodeId = nodeid;
            Core = Array.Empty<ulong>();
            Radius = 0;
            ChildId = 0;
        }
    }
}
