
namespace GoldAddin
{
	/// <summary>
	/// Defines the compiler interface to a GOLD grammar compiler
	/// </summary>
	public interface ICompiler
	{	
		/// <summary>
		/// Compile the specified grammar 
		/// </summary>
		/// <param name="grammarFile">Grammar file to compile</param>
		/// <param name="outputFile">The name of the binary file output</param>
		/// <remarks>The GOLD compiler outputs 2 versions of binary grammar tables:
		/// CGT and EGT. The format generated depends on the file extension of the 
		/// output file (.cgt or .egt)</remarks>
		void Compile(string grammarFile, string outputFile);
						
		/// <summary>
		/// Occurs when a compilation output step completes.
		/// </summary>
		event CompilerEventHandler MessageOutput;
	}

	public delegate void CompilerEventHandler(CompilerMessage message);
}

