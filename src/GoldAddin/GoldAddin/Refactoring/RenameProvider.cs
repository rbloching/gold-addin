using System.Collections.Generic;
using Mono.TextEditor;
using bsn.GoldParser.Parser;

namespace GoldAddin
{
	/// <summary>
	/// Provides a text segment list for rename-refactoring
	/// </summary>
	public interface IRenameProvider
	{	
		/// <summary>
		/// Gets all of the segments to be renamed 
		/// </summary>
		/// <value>The segments to rename.</value>
		ICollection<TextSegment> SegmentsToRename{ get; }

		/// <summary>
		/// Gets the text segment that was selected for renaming
		/// </summary>
		/// <value>The selected text segment.</value>
		TextSegment SelectedTextSegment{get;}

		/// <summary>
		/// True if there is at least one segment that can be renamed
		/// </summary>
		/// <value><c>true</c> if this instance has segments; otherwise, <c>false</c>.</value>
		bool HasSegments{get;}

		/// <summary>
		/// Gets the type of the symbol that was selected for renaming
		/// </summary>
		/// <value>The type of the selected symbol.</value>
		DefinitionType SelectedSymbolType{ get; }

	}


	/// <summary>
	/// Provides a text segment list for rename-refactoring
	/// </summary>
	public class RenameProvider : IRenameProvider
	{
		TextSegment primarySegment; 
		ICollection<TextSegment> textSegmentList;	
		DefinitionType symbolType;		


		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.RenameProvider"/> class.
		/// </summary>
		/// <param name="text">The document text</param>
		/// <param name="documentPosition">Position within the text</param>
		public RenameProvider(string text, int documentPosition)
		{
			textSegmentList = new List<TextSegment> ();
			primarySegment = new TextSegment ();
			symbolType = DefinitionType.None;
			if (!string.IsNullOrEmpty (text))
			{
				findSegmentsForRenaming (text, documentPosition);
			}
		}
		
		/// <summary>
		/// Gets all of the segments to be renamed 
		/// </summary>
		/// <value>The segments to rename.</value>
		public ICollection<TextSegment> SegmentsToRename
		{
			get{ return textSegmentList;}
		}


		/// <summary>
		/// Gets the text segment that was selected for renaming
		/// </summary>
		/// <value>The selected text segment.</value>
		public TextSegment SelectedTextSegment
		{
			get{ return primarySegment;}
		}
		

		/// <summary>
		/// True if there is at least one segment that can be renamed
		/// </summary>
		/// <value><c>true</c> if this instance has segments; otherwise, <c>false</c>.</value>
		public bool HasSegments
		{
			get{ return textSegmentList.Count > 0;}
		}


		/// <summary>
		/// Gets the type of the symbol that was selected for renaming
		/// </summary>
		/// <value>The type of the selected symbol.</value>
		public DefinitionType SelectedSymbolType
		{
			get{ return symbolType;}
		}


		void findSegmentsForRenaming(string text, int documentPosition)
		{
			var doc = new GoldParsedDocument ();
			doc.Parse (text);

			//not everything can be renamed
			var token = doc.GetTokenAt (documentPosition);

			if (tokenIsRenameable (token))
			{
				primarySegment = tokenToSegment (token);
				symbolType = TokenUtil.GetDefinitionType(token);

				//get our list of items to rename
				var renameItems = doc.FindUsesOf(token.Text);

				TextSegment segment;
				foreach (Token renameItem in renameItems)
				{
					segment = tokenToSegment (renameItem);
					textSegmentList.Add (segment);
				}
			}
		}

		//checks that the given token is one that could be renamed
		static bool tokenIsRenameable(Token token)
		{
			if (token == null)
				return false;

			var symbolType = TokenUtil.GetDefinitionType (token);

			return 
			(symbolType == DefinitionType.SetName ||
			symbolType == DefinitionType.Terminal ||
			symbolType == DefinitionType.NonTerminal) &&
				!TokenUtil.IsTerminalLiteral (token); //cant rename terminal literal
		}

		static char[] delims = { '{', '}', '<', '>' };
		//assumes that str is delimited
		static string trimDelimiters(string str)
		{
			return str.TrimStart(delims).TrimEnd(delims);
		}	

		static TextSegment tokenToSegment(Token token)
		{
			//we don't want to have to retype delimiters for sets or nonterminals, 
			// so filter down to just the content
			string str = trimDelimiters(token.Text);
			int startIndex = (int)token.Position.Index;

			//start index must be after the opening delimiter
			if (str.Length != token.Text.Length)
				startIndex+=1; 

			return new TextSegment(startIndex, str.Length);
		}	

	}
}
