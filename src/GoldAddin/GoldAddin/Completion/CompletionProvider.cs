using System.Collections.Generic;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Core;
using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace GoldAddin
{
	/// <summary>
	/// Provides a code completion list
	/// </summary>
	public partial class CompletionProvider
	{						

		//				Possible Completion Combinations
		/*========================================================================
		symbol       		context							what to show
		--------------------------------------------------------------------------
		<					rule declaration				NonTerminals
		alphanum.			rule declaration				Termnials
		{					terminal declaration			SetNames
		alphanum.			terminal declaration			Terminals
		{					set  declaration				SetNames
		"					new declaration					Properties
		<					new declaration					NonTerminals
		===========================================================================

		Notes
		----
		* List of SetNames and Terminals are those that have been defined in the document
		* List of NonTerminals are any that appear in the document
		* The empty terminal (<>) appears in the NonTerminal list for a rule declaration
			context, but not a new line context
		 */					
				
		static readonly HashSet<CompletionData> PreDefinedCharacterSet;
		static readonly ICollection<CompletionData> PreDefinedProperties;
		static readonly Dictionary<DefinitionType,IconId> iconMap;			
		
		static CompletionProvider()
		{
			iconMap = new Dictionary<DefinitionType, IconId> ();
			PreDefinedProperties = new List<CompletionData> ();
			PreDefinedCharacterSet = new HashSet<CompletionData> ();

			initIcons (); //icons must be setup first so they are available
			initCharacterSets ();
			initProperties ();
		}
	

		GoldParsedDocument doc;
		string data;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.CompletionProvider"/> class.
		/// </summary>
		/// <param name="text">text to provide completion data from</param>
		public CompletionProvider (string text)
		{
			data = text;
			doc = new GoldParsedDocument ();
			doc.Parse (text);
		}

		public CompletionProvider (GoldParsedDocument parsedDocument, string text)
		{
			doc = parsedDocument;
			data = text;

		}

		//creates a completion list for non-terminals. The names included in the 
		// list will match any that exist in the document, not just those that
		// have been defined already
		HashSet<CompletionData> createCompletionsForNonTerminals()
		{
			var completionList = new HashSet<CompletionData> ();
			var namesFound = new AnyCaseStringSet ();

			var tokenList = doc.FindTokensByType (DefinitionType.NonTerminal);
			foreach (Token token in tokenList)
			{
				//completion lists should contain each item only once
				if (!namesFound.Contains (token.Text) && 
				    //always ignore literal expressions
				    !token.Text.StartsWith("\'",System.StringComparison.Ordinal)) 
				{
					namesFound.Add (token.Text);
					completionList.Add (tokenToCompletionData (token));
				}
			}
			return completionList;
		}

		/// <summary>
		/// Gets the code completion list to display
		/// </summary>
		/// <returns>A completion list. Will be null if the character at index 
		/// is not in a completion context</returns>
		/// <param name="index">This is the position of the trigger character in the text</param>
		public ICompletionDataList GetCompletionList(int index)
		{
			//TODO: make it possible to check this condition
			//if (index < 0 || index >= data.Length)
			//	return null;

			char ch = data [index];
			if (!isValidTriggerChar (ch))
				return null;

			string lineStr = getLineUpTo (index);
			if (isTerminalLiteralStart (lineStr))
				return null;

			Context context = getContext (index);
			return getList (ch, context);
		}

		//returns true if the trigger character is one that we want to 
		// (possibly) show a list for
		static bool isValidTriggerChar(char ch)
		{
			return ch == '{' || ch == '<' || ch == '\"' || char.IsLetterOrDigit (ch);
		}


		Context getContext(int index)
		{
			Context context = Context.Unknown;

			if (!isInComment (index))
			{
				string lineStr = getLineUpTo (index);
				//tokenize the line, but don't include the trigger character
				var tokenizer = new GrammarTokenizer (lineStr.Remove (lineStr.Length - 1));
				var firstToken = tokenizer.NextTokenIgnoreNoise ();
				var firstSymbolType = TokenUtil.GetDefinitionType (firstToken);

				if (TokenUtil.IsUserDefinitionToken (firstToken))
				{
					Token secondToken = tokenizer.NextTokenIgnoreNoise ();
					if (secondToken.Symbol.Name == "=")
					{
						if (firstSymbolType==DefinitionType.Terminal)
							context = Context.TerminalDeclaration;
						else if (firstSymbolType==DefinitionType.SetName)
							context = Context.SetDeclaration;
					}
					else if (secondToken.Symbol.Name == "::=" && firstSymbolType==DefinitionType.NonTerminal)
					{
						context = Context.RuleDeclaration;
					}
				}
				else if (firstToken.Symbol.Name == "|")
				{
					context = Context.RuleDeclaration;
				}
				else if (firstToken.Symbol.Kind == SymbolKind.End)
				{
					context= Context.NewDeclaration;
				}
			}
			return context;
		}

		bool isInComment(int index)
		{
			var token = doc.GetTokenAt (index);
			return token != null && TokenUtil.IsNoise (token);				
		}	


		//returns a CompletionDataList based on the given character and context
		ICompletionDataList getList(char ch, Context context)
		{
			bool isAlphaNum = char.IsLetterOrDigit (ch);


			if (context == Context.RuleDeclaration && ch == '<')
			{
				//this case is different - the rule list should show undefined as well!!
				var nonTerminalList = createCompletionsForNonTerminals ();
				nonTerminalList.Add (nullableTerminal ());
				return new CompletionDataList (nonTerminalList);
			}

			if (context == Context.RuleDeclaration && isAlphaNum)
			{
				var terminalList = createCompletionsForType(DefinitionType.Terminal);
				return new CompletionDataList (terminalList);
			}

			if (context == Context.TerminalDeclaration && ch == '{')
			{
				var sets = createCompletionsForType(DefinitionType.SetName);
				sets.UnionWith (PreDefinedCharacterSet);
				return new CompletionDataList (sets);
			}

			if (context == Context.TerminalDeclaration && isAlphaNum)
			{
				return new CompletionDataList (createCompletionsForType(DefinitionType.Terminal));
			}

			if (context == Context.SetDeclaration && ch == '{')
			{
				var sets = createCompletionsForType(DefinitionType.SetName);
				sets.UnionWith (PreDefinedCharacterSet);
				return new CompletionDataList (sets);
			}

			if (context == Context.NewDeclaration && ch=='\"')
			{
				return new CompletionDataList (PreDefinedProperties);
			}

			if (context == Context.NewDeclaration && ch=='<')
			{
				return new CompletionDataList (createCompletionsForNonTerminals ());
			}
			return null;

		}
		
	 	
		//converts the given token to a CompletionData object
		static CompletionData tokenToCompletionData(Token token)
		{
			return new CompletionData (token.Text, getIcon(token));
		}

		static CompletionData nullableTerminal()
		{
			return newCompletionData ("<>", DefinitionType.NonTerminal);
		}


					
		//get the line up to the given position in the text. 
		string getLineUpTo(int index)
		{
			//GOLD considers one of \r, \n, or \r\n to be a new line. 
			// so we need to search back from index to find the last one

			if(index<0 || index>=data.Length)
				return string.Empty;


			//the index could already be at the end of the line 
			// so start before it, otherwise we miss the line
			int pos = index;
			if (data [pos] == '\n')
			{
				pos -= 1;
			}
			if (pos>=0 && data [pos] == '\r')
			{
				pos -= 1;
			}

			while (pos>=0)
			{
				if (data [pos] == '\r' || data [pos] == '\n')
				{
					break;
				}
				pos -= 1;
			}

			//pos now marks the end of the previous line
			return data.Substring (pos + 1, index - pos);
		}			

		//checks that the given line string is inside a terminal literal
		static bool isTerminalLiteralStart(string lineStr)
		{
			var tokenizer = new GrammarTokenizer (lineStr);
			var tokenList = new List<Token> ();
			Token token;
			do
			{
				token = tokenizer.NextToken();
				tokenList.Add(token);
			} while(token.Symbol.Kind!=SymbolKind.End);


			//when we are inside a terminal literal, the sequence would end with these tokens:
			// ERROR,TERMINAL,END
			const int ExpectedTokenCount = 3;
			int lastIndex = tokenList.Count - 1;
			if (tokenList.Count >= ExpectedTokenCount)
			{
				return
					tokenList [lastIndex - 2].Symbol.Kind == SymbolKind.Error &&
					tokenList [lastIndex - 1].Symbol.Kind == SymbolKind.Terminal;
			}
			return false;									   
		}
		

		//creates a completions list for the given defintion type - so only symbols
		// that have been defined will appear in the list
		HashSet<CompletionData> createCompletionsForType(DefinitionType type)
		{
			var completionList = new HashSet<CompletionData> ();

			//we want only defined symbols in the completion list
			var definitions = doc.FindDefinitionsByType(type);
			//...and no duplicates of names
			var names = new AnyCaseStringSet ();

			foreach (DefinitionNode def in definitions)
			{
				string name = def.Name;
				if (!names.Contains (name))
				{
					names.Add (name);
					var completion = newCompletionData (name, type);
					completionList.Add (completion);
				}
			}
			return completionList;
		}

				
		static IconId getIcon(Token token)
		{
			return getIcon (TokenUtil.GetDefinitionType (token));
		}

		static IconId getIcon(DefinitionType symbolType)
		{
			IconId icon;
			bool foundIcon = iconMap.TryGetValue (symbolType, out icon);
			if (!foundIcon)
				icon = IconId.Null;
			return icon;
		}


		static CompletionData newCompletionData(string name, DefinitionType symbolType)
		{
			return new CompletionData (name, getIcon (symbolType));
		}

		//Possible semantic contexts
		enum Context
		{
			RuleDeclaration,
			TerminalDeclaration,
			SetDeclaration,
			NewDeclaration,
			Unknown
		}



	}
}

