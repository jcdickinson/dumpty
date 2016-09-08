using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumpty
{
    /// <summary>
    /// Represents the REPL.
    /// </summary>
    public sealed class ReplCommand : ICommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string Name
        {
            get { return "repl"; }
        }

        /// <summary>
        /// Displays the usage for the command.
        /// </summary>
        public void Usage()
        {
            Console.WriteLine("repl");
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
            return new ReplCommand();
        }

        /// <summary>
        /// Executes the command against the specified dump.
        /// </summary>
        /// <param name="dump">The dump to execute the command against.</param>
        public void Execute(Dump dump)
        {
            while (true)
            {
                Console.Write("> ");
                var args = CmdSplit(Console.ReadLine()).ToList();
                if (args.Count == 0) continue;

                if (string.Equals(args[0], "quit", StringComparison.OrdinalIgnoreCase)) break;
                CommandController.Execute(dump, args);
            }
        }

        private static IEnumerable<string> CmdSplit(string value)
        {
            const int State_Literal = 0;
            const int State_LiteralQuote = 1;
            const int State_Quoted = 2;
            const int State_QuotedQuote = 3;

            var current = new StringBuilder(value.Length);
            var state = State_Literal;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (state)
                {
                    case State_Literal:
                        if (c == '"')
                            state = State_LiteralQuote;
                        else if (c == ' ')
                        {
                            if (current.Length != 0)
                            {
                                yield return current.ToString();
                                current.Clear();
                            }
                        }
                        else
                            current.Append(c);
                        break;
                    case State_LiteralQuote:
                        if (c == '"')
                        {
                            current.Append(c);
                            state = State_Literal;
                        }
                        else
                        {
                            if (current.Length != 0)
                            {
                                yield return current.ToString();
                                current.Clear();
                            }
                            current.Append(c);
                            state = State_Quoted;
                        }
                        break;
                    case State_Quoted:
                        if (c == '"')
                            state = State_QuotedQuote;
                        else
                            current.Append(c);
                        break;
                    case State_QuotedQuote:
                        if (c == '"')
                        {
                            current.Append(c);
                            state = State_Quoted;
                        }
                        else
                        {
                            if (current.Length != 0)
                            {
                                yield return current.ToString();
                                current.Clear();
                            }
                            if (c != ' ') current.Append(c);
                            state = State_Literal;
                        }
                        break;
                }
            }

            if (current.Length != 0) yield return current.ToString();
        }
    }
}
