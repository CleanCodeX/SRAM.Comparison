using System.Collections.Generic;

namespace SramComparer.Services
{
	/// <summary>
	/// Interface for parsing command line options in either an existing instance or creating a new.
	/// </summary>
	public interface ICmdLineParser
	{
		/// <summary>
		/// Parses a list of string arguments into an <see cref="IOptions"/> instance
		/// </summary>
		/// <param name="args">The string arguments to be parsed</param>
		/// <returns>Returns an <see cref="IOptions"/> instance</returns>
		IOptions Parse(IReadOnlyList<string> args);

		/// <summary>
		/// Parses a list of string arguments into an <see cref="IOptions"/> instance
		/// </summary>
		/// <param name="args">The string arguments to be parsed</param>
		/// <param name="options">The existing options</param>
		/// <returns>Returns an <see cref="IOptions"/> instance</returns>
		IOptions Parse(IReadOnlyList<string> args, IOptions options);
	}
}