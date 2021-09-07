using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageBank
{
    public class Node
    {
        public Mat Core { get; set; }
        public int Depth { get; }
        public int ChildId { get; set; }
        public DateTime LastAdded { get; set; }
        public SortedDictionary<string, object> Members { get; }

    public Node(
            Mat core,
            int depth,
            int childid,
            string members,
            DateTime lastadded
            )
        {
            Core = core;
            Depth = depth;
            ChildId = childid;
            Members = new SortedDictionary<string, object>();
            AddMembers(members);
            LastAdded = lastadded;
        }

        public bool AddMember(string name)
        {
            if (!Members.ContainsKey(name)) {
                Members.Add(name, null);
                return true;
            }

            return false;
        }

        public void RemoveMember(string name)
        {
            Members.Remove(name);
        }

        public void AddMembers(string buffer)
        {
            var offset = 0;
            while (offset < buffer.Length) {
                var name = buffer.Substring(offset, 10);
                AddMember(name);
                offset += 10;
            }
        }

        public string GetMembers()
        {
            var sb = new StringBuilder();
            foreach (var name in Members.Keys) {
                sb.Append(name);
            }

            return sb.ToString();
        }

        public void Kill()
        {
            if (Core != null) {
                Core.Dispose();
                Core = null;
            }
            
            Members.Clear();
        }
    }
}
