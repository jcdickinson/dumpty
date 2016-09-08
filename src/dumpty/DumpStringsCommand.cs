using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dumpty
{
    /// <summary>
    /// Represents a command that dumps all strings to a directory.
    /// </summary>
    public class DumpStringsCommand : ICommand
    {
        /// <summary>
        /// Gets or sets the directory.
        /// </summary>
        /// <value>
        /// The directory.
        /// </value>
        public string Directory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string Name
        {
            get { return "dumpstrings"; }
        }

        /// <summary>
        /// Displays the usage for the command.
        /// </summary>
        public void Usage()
        {
            Console.WriteLine("dumpstrings <directory>");
        }

        /// <summary>
        /// Parses the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// A new command instance that contains the parsed arguments. If the arguments are invalid
        /// <c>null</c> will be returned.
        /// </returns>
        public ICommand Parse(IEnumerable<string> args)
        {
            string dir = null;
            foreach (var arg in args)
            {
                if (dir == null)
                    dir = arg;
                else
                    return null;
            }

            if (dir == null) return null;
            return new DumpStringsCommand()
            {
                Directory = dir
            };
        }

        /// <summary>
        /// Executes the command against the specified dump.
        /// </summary>
        /// <param name="dump">The dump to execute the command against.</param>
        public void Execute(Dump dump)
        {
            System.IO.Directory.CreateDirectory(Directory);

            var counts = new Dictionary<byte[], Tuple<int, ulong, string>>(ByteArrayEqualityComparer.Default);

            foreach (var obj in dump.Heap.EnumerateObjectAddresses())
            {
                var type = dump.Heap.GetObjectType(obj);
                if (type.Name == "System.String")
                {
                    var value = (string)type.GetValue(obj);
                    using (var murmur = new Murmur3())
                    {
                        using (var hash = new CryptoStream(Stream.Null, murmur, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(hash, Encoding.UTF8))
                            sw.Write(value);
                        var key = murmur.Hash;

                        Tuple<int, ulong, string> data;
                        if (!counts.TryGetValue(key, out data))
                        {
                            var keyBuilder = new StringBuilder(key.Length * 2);
                            for (var i = 0; i < key.Length; i++)
                                keyBuilder.Append(key[i].ToString("x2"));

                            var path = Path.Combine(Directory, keyBuilder + ".txt");
                            if (!File.Exists(path))
                            {
                                using (var sw = new StreamWriter(path, false))
                                    sw.Write(value);
                            }

                            counts[key] = Tuple.Create(1, type.GetSize(obj), keyBuilder.ToString());
                        }
                        else
                        {
                            counts[key] = Tuple.Create(data.Item1 + 1, data.Item2, data.Item3);
                        }
                    }
                }
            }

            using (var sw = new StreamWriter(Path.Combine(Directory, "summary.txt"), false))
            {
                foreach (var item in counts.OrderBy(x => (ulong)x.Value.Item1 * x.Value.Item2))
                {
                    sw.WriteLine("{0}: {1} - {2}b", item.Value.Item3, item.Value.Item1, (ulong)item.Value.Item1 * item.Value.Item2);
                }
            }
        }
    }
}
