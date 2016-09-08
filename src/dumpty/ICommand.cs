using System.Collections.Generic;

namespace Dumpty
{
    /// <summary>
    /// Represents the implementation of a dump command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Displays the usage for the command.
        /// </summary>
        void Usage();

        /// <summary>
        /// Parses the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// A new command instance that contains the parsed arguments. If the arguments are invalid
        /// <c>null</c> will be returned.
        /// </returns>
        ICommand Parse(IEnumerable<string> args);

        /// <summary>
        /// Executes the command against the specified dump.
        /// </summary>
        /// <param name="dump">The dump to execute the command against.</param>
        void Execute(Dump dump);
    }
}
