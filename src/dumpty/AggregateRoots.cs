using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Dumpty
{
    public class AggregateRoots : ICommand
    {
        private class Node
        {
            public Node Next;
            public Node Prev;
            public ulong Object;
            public ClrType Type;
            public int Offset;
            public ulong[] Children;
            public int[] Offsets;
            public int Curr;

            public Node(ulong obj, ClrType type, Node prev = null)
            {
                Object = obj;
                Prev = prev;
                Type = type;

                if (type == null)
                    throw new ArgumentNullException("type");

                if (prev != null)
                    prev.Next = this;
            }
        }

        private static readonly Func<ulong, IList<ClrRoot>> _createRootList = _ => new List<ClrRoot>();

        public string Name
        {
            get { return "agcroot"; }
        }

        public long TypeHandle
        {
            get;
            set;
        }

        public string TypeName
        {
            get;
            set;
        }

        public string ReportFile
        {
            get;
            set;
        }

        public void Usage()
        {
            Console.WriteLine("agcroot <typename|0xtypehandle|all> <report file>");
        }

        public ICommand Parse(IEnumerable<string> args)
        {
            var type = args.FirstOrDefault();
            var reportFile = args.Skip(1).FirstOrDefault();

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(reportFile)) return null;

            try
            {
                if (type.StartsWith("0x"))
                {
                    return new AggregateRoots()
                    {
                        TypeHandle = uint.Parse(type.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture),
                        ReportFile = reportFile
                    };
                }
                else
                {
                    return new AggregateRoots()
                    {
                        TypeName = type,
                        ReportFile = reportFile
                    };
                }
            }
            catch
            {
                return null;
            }
        }

        public void Execute(Dump dump)
        {
            var considered = new HashSet<ulong>();
            var completed = new HashSet<ClrRoot>();
            var targets = new Dictionary<ulong, Node>();
            var rootDictionary = new Dictionary<ulong, IList<ClrRoot>>();

            var pos = Console.CursorTop;
            Console.WriteLine("Finding objects...");

            ClrType type = null;
            var isAll = string.Equals(TypeName, "all", StringComparison.OrdinalIgnoreCase);
            foreach (var obj in dump.Heap.EnumerateObjectAddresses())
            {
                var objType = dump.Heap.GetObjectType(obj);
                if (objType != null && objType.MetadataToken != 0 &&
                    (isAll || objType.MetadataToken == TypeHandle || string.Equals(objType.Name, TypeName, StringComparison.Ordinal)))
                {
                    targets.Add(obj, new Node(obj, objType));
                    type = objType;
                }
            }

            Console.SetCursorPosition(0, pos);
            Console.WriteLine("{0} objects found.", targets.Count);
            if (targets.Count == 0) return;

            pos = Console.CursorTop;
            Console.WriteLine("Enumerating roots...");
            var roots = new List<ClrRoot>(dump.Heap.EnumerateRoots());
            for (var i = 0; i < roots.Count; i++)
            {
                if (i % 1000 == 0)
                {
                    Console.SetCursorPosition(0, pos);
                    Console.WriteLine("Enumerating roots [{0}/{1}]...", i, roots.Count);
                }

                var root = roots[i];

                IList<ClrRoot> list;
                if (!rootDictionary.TryGetValue(root.Object, out list))
                    rootDictionary.Add(root.Object, list = new List<ClrRoot>());
                list.Add(root);
            }

            var byEntirePath = new Dictionary<string, ulong>();
            var byRoot = new Dictionary<string, ulong>();

            pos = Console.CursorTop;
            Console.WriteLine("Traversing roots...");
            for (var i = 0; i < roots.Count; i++)
            {
                var root = roots[i];
                if (completed.Contains(root) || considered.Contains(root.Object))
                    continue;

                if (i % 1000 == 0)
                {
                    Console.SetCursorPosition(0, pos);
                    Console.WriteLine("Traversing roots [{0}/{1}]...", i, roots.Count);
                }

                var path = FindPathToTarget(dump, root, considered, targets);
                var fullPath = new StringBuilder();

                for (var node = path; node != null; node = node.Next)
                {
                    IList<ClrRoot> nodeRoots;
                    if (rootDictionary.TryGetValue(node.Object, out nodeRoots))
                    {
                        foreach (var nodeRoot in nodeRoots)
                        {
                            completed.Add(nodeRoot);
                            fullPath.AppendFormat("{0,12:X} -> {1,12:X} {2}", nodeRoot.Address, nodeRoot.Object, nodeRoot.Name);
                            IncrementKey(byRoot, nodeRoot.Name);
                        }
                    }
                }

                fullPath.AppendLine();
                for (var node = path; node != null; node = node.Next)
                {
                    var fmt = string.Format("-> {0,12:X} {1}", node.Object, node.Type.Name);
                    fullPath.AppendLine(fmt);
                    IncrementKey(byRoot, string.Format("-> {0}", node.Type.Name));
                }

                IncrementKey(byEntirePath, fullPath.ToString());
            }

            Console.WriteLine("Writing report...");
            using (var sw = new StreamWriter(ReportFile, false))
            {
                sw.WriteLine("By root:");
                sw.WriteLine("--------------------------------------------------------------------------------");
                foreach (var kv in byRoot.OrderByDescending(x => x.Value))
                {
                    sw.WriteLine("{0}: {1}", kv.Value, kv.Key);
                }

                sw.WriteLine();
                sw.WriteLine("By path:");
                sw.WriteLine("--------------------------------------------------------------------------------");
                foreach (var kv in byEntirePath.OrderByDescending(x => x.Value))
                {
                    sw.WriteLine("----------------------------------------------------------------------");
                    sw.WriteLine("{0}: ", kv.Value);
                    sw.WriteLine(kv.Key);
                }
            }
        }

        private static void IncrementKey<TKey>(IDictionary<TKey, ulong> dictionary, TKey key)
        {
            ulong value;
            if (dictionary.TryGetValue(key, out value))
                dictionary[key] = value + 1;
            else
                dictionary.Add(key, 1);
        }

        private Node FindPathToTarget(Dump dump, ClrRoot root, HashSet<ulong> considered, Dictionary<ulong, Node> targets)
        {
            var type = dump.Heap.GetObjectType(root.Object);
            if (type == null) return null;

            var refList = new List<ulong>();
            var offsetList = new List<int>();

            var curr = new Node(root.Object, type);
            while (curr != null)
            {
                if (curr.Children == null)
                {
                    refList.Clear();
                    offsetList.Clear();

                    curr.Type.EnumerateRefsOfObject(curr.Object, (child, offset) =>
                    {
                        if (child != 0)
                        {
                            refList.Add(child);
                            offsetList.Add(offset);
                        }
                    });

                    curr.Children = refList.ToArray();
                    curr.Offsets = offsetList.ToArray();
                }
                else
                {
                    if (curr.Curr < curr.Children.Length)
                    {
                        ulong nextObj = curr.Children[curr.Curr];
                        int offset = curr.Offsets[curr.Curr];
                        curr.Curr++;

                        if (considered.Contains(nextObj))
                            continue;

                        considered.Add(nextObj);

                        Node next = null;
                        if (targets.TryGetValue(nextObj, out next))
                        {
                            curr.Next = next;
                            next.Prev = curr;
                            next.Offset = offset;

                            while (curr.Prev != null)
                            {
                                targets[curr.Object] = curr;
                                curr = curr.Prev;
                            }

                            targets[curr.Object] = curr;
                            return curr;
                        }

                        type = dump.Heap.GetObjectType(nextObj);
                        if (type != null && type.ContainsPointers)
                        {
                            curr = new Node(nextObj, type, curr);
                            curr.Offset = offset;
                        }
                    }
                    else
                    {
                        curr = curr.Prev;

                        if (curr != null)
                            curr.Next = null;
                    }
                }
            }

            return null;
        }
    }
}
