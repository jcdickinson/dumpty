using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;

namespace Dumpty
{
    /// <summary>
    /// Represents the command that loads a dump.
    /// </summary>
    public sealed class LoadCommand : ICommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string Name
        {
            get { return "load"; }
        }

        /// <summary>
        /// Gets or sets the location of the dump to load.
        /// </summary>
        /// <value>
        /// The location of the dump to load.
        /// </value>
        public string Location
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location of the DAC.
        /// </summary>
        /// <value>
        /// The location of the DAC.
        /// </value>
        public string DacLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Displays the usage for the command.
        /// </summary>
        public void Usage()
        {
            Console.WriteLine("load <location> [dac location]");
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
            string loc = null, dloc = null;
            foreach (var arg in args)
            {
                if (loc == null)
                    loc = arg;
                else if (dloc == null)
                    dloc = arg;
                else
                    return null;
            }

            if (loc != null)
                return new LoadCommand()
                {
                    Location = loc,
                    DacLocation = dloc
                };
            else
                return null;
        }

        /// <summary>
        /// Executes the command against the specified dump.
        /// </summary>
        /// <param name="dump">The dump to execute the command against.</param>
        public void Execute(Dump dump)
        {
            dump.ActiveDump = DataTarget.LoadCrashDump(Location);
            dump.DacLocation = DacLocation;
            Console.WriteLine("Loaded: {0}", Location);
        }
    }
}
