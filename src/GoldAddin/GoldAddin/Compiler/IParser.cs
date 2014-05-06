namespace GoldAddin
{
	/// <summary>
	/// Defines an interface for a document parser
	/// </summary>
	public interface IParser
	{
		/// <summary>
		/// Parse the specified input.
		/// </summary>
		/// <param name="input">Input.</param>
		void Parse (string input);

		/// <summary>
		/// Occurs when error is found during parsing
		/// </summary>
		event CompilerEventHandler ErrorFound;
	}
}

