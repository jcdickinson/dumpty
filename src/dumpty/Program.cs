using System;
using System.Collections.Generic;
using System.Linq;

namespace Dumpty
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandController.Register(new ReplCommand());
            CommandController.Register(new LoadCommand());
            CommandController.Register(new DumpStringsCommand());
            CommandController.Register(new AggregateRoots());

            var dump = new Dump();
            foreach (var cmd in SplitArgs(args))
                CommandController.Execute(dump, cmd);
        }

        static IEnumerable<IEnumerable<string>> SplitArgs(string[] args)
        {
            var start = 0;
            for (var end = 0; end < args.Length; end++)
            {
                var arg = args[end];
                if (string.Equals(arg, "-e", StringComparison.OrdinalIgnoreCase))
                {
                    if (start != end) yield return args.Skip(start).Take(end - start);
                    start = end + 1;
                }
            }

            if (start != args.Length)
                yield return args.Skip(start).Take(args.Length - start);
        }
    }
}
