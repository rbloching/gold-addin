using MonoDevelop.Ide.TypeSystem;

namespace GoldAddin
{
	/// <summary>
	/// Defines a handler for background parsing of a grammar document
	/// </summary>
	/// <remarks>This class must be registered with MonoDevelop/TypeSystem/Parser in
	/// the addin.xml file.
	/// </remarks>
	public class GrammarTypeSystemParser : TypeSystemParser
	{
		/// <summary>
		/// Parses the given file.
		/// </summary>
		/// <remarks>
		/// This will be invoked by the IDE when the document is changed in the editor
		/// </remarks>
		public override ParsedDocument Parse (bool storeAst, string fileName, System.IO.TextReader content, MonoDevelop.Projects.Project project = null)
		{
			var doc = new GoldParsedDocument ();
			doc.Parse (content.ReadToEnd ());
			return doc;
		}	

		/// <summary>
		/// Parses the given file.
		/// </summary>
		/// <remarks>
		/// This will be invoked by the IDE when the file is opened in the text editor
		/// </remarks>
		public override ParsedDocument Parse (bool storeAst, string fileName, MonoDevelop.Projects.Project project = null)
		{
			var doc = new GoldParsedDocument ();
			doc.ParseFromFile (fileName);
			return doc;
		}	

	}

}
