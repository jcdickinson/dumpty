using System;
using System.Collections.Generic;
using System.Linq;

namespace Dumpty
{
    /// <summary>
    /// Represents the command controller.
    /// </summary>
    public static class CommandController
    {
        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void Register(ICommand command)
        {
            _commands[command.Name] = command;
        }

        /// <summary>
        /// Executes the specified arguments.
        /// </summary>
        /// <param name="dump">The dump.</param>
        /// <param name="args">The arguments.</param>
        public static void Execute(Dump dump, IEnumerable<string> args)
        {
            ICommand ic;
            var cmd = args.FirstOrDefault();
            args = args.Skip(1);
            if (string.IsNullOrEmpty(cmd))
                return;
            else if (string.Equals(cmd, "help", StringComparison.OrdinalIgnoreCase))
            {
                cmd = args.FirstOrDefault();
                if (string.IsNullOrEmpty(cmd) || !_commands.TryGetValue(cmd, out ic))
                {
                    Console.WriteLine("help <command>");
                    foreach (var c in _commands.Keys)
                        Console.WriteLine("  {0}", c);
                }
                else
                    ic.Usage();
            }
            else if (_commands.TryGetValue(cmd, out ic))
            {
                try
                {
                    var pic = ic.Parse(args);
                    if (pic == null) ic.Usage();
                    else pic.Execute(dump);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("help <command>");
                foreach (var c in _commands.Keys)
                    Console.WriteLine("  {0}", c);
            }
        }
    }
}
