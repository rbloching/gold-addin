using System;
using System.Collections.Generic;
using System.Linq;
using bsn.GoldParser.Parser;
using MonoDevelop.Ide.TypeSystem;

namespace GoldAddin
{
	/// <summary>
	/// Represents a user-defined grammar, parsed from a document that is in work. Likely the 
	/// document cannot be parsed to an 'accept' state. This object makes available the 
	/// grammar definitions which could be determined
	/// </summary>
	public class GoldParsedDocument : DefaultParsedDocument
	{
		List<DefinitionNode> definitions;
		List<Token> tokenList;	


		public GoldParsedDocument() :base(string.Empty)
		{
			definitions = new List<DefinitionNode> ();
			tokenList = new List<Token> ();
		}

		public void Parse(string input)
		{
			definitions.Clear ();
			tokenList.Clear ();
			parse (input);
		}

		public void ParseFromFile(string fileName)
		{
			string input = string.Empty;
			if (System.IO.File.Exists (fileName))
			{
				input = System.IO.File.ReadAllText (fileName);
			}
			Parse (input);
		}
	

		void parse(string text)
		{
			var tokenizer = new GrammarTokenizer (text);
			var tempList = new List<Token> ();
			Token token=null;
			Token prev=null;

			while (!TokenUtil.IsEnd(token))
			{
				token = tokenizer.NextToken ();
				tokenList.Add (token);

				if (!TokenUtil.IsNoise (token))
				{
					if (!TokenUtil.IsNewLine (token))
					{
						if (isDefinitionStart (token, prev) || TokenUtil.IsEnd (token))
						{
							addDefinitionFrom (tempList);
							tempList.Clear ();
						}
						tempList.Add (token);
					}
					prev = token;
				}
			}
			 
		}

		void addDefinitionFrom(IList<Token> lineTokens)
		{
			var def = getDefinitionFromTokenLine (lineTokens);
			if (def != null)
				definitions.Add (def);
		}
			

		static DefinitionNode getDefinitionFromTokenLine(IList<Token> lineTokens)
		{
			if (lineTokens.Count <2) //at least 2 tokens needed for every declaration
				return null;

			Token firstToken = lineTokens [0];
			Token secondToken = lineTokens [1];

			int length = calcTextLength (lineTokens);
			var firstSymbolType = TokenUtil.GetDefinitionType (firstToken);

			if (firstSymbolType == DefinitionType.NonTerminal && secondToken.Symbol.Name == "::=")
				return new NonTerminalDefinition (firstToken.Text, firstToken.Position, length);

			if(firstSymbolType==DefinitionType.SetName && secondToken.Symbol.Name=="=")
				return new SetDefinition (firstToken.Text, firstToken.Position, length);

			if(firstSymbolType==DefinitionType.Property && secondToken.Symbol.Name=="=")
				return new PropertyDefinition (firstToken.Text, firstToken.Position, length);

			if(firstSymbolType==DefinitionType.Terminal && !TokenUtil.IsTerminalLiteral(firstToken) && secondToken.Symbol.Name=="=")
				return new TerminalDefinition(firstToken.Text, firstToken.Position, length);

			return null;
		}

		static int calcTextLength(IList<Token> lineTokens)
		{
			Token firstToken = lineTokens [0];
			Token lastToken = lineTokens [lineTokens.Count - 1];
			int lastIndex = ((int)lastToken.Position.Index + lastToken.Text.Length) - 1;
			int length = (lastIndex - (int)firstToken.Position.Index) + 1;
			return length;
		}


		/// <summary>
		/// Finds definitions of the given type
		/// </summary>
		/// <returns>The definitions of.</returns>
		/// <param name="symbolType">Symbol.</param>
		public IEnumerable<DefinitionNode> FindDefinitionsByType(DefinitionType symbolType)
		{
			return definitions.Where (def => def.Type == symbolType);
		}

		/// <summary>
		/// Finds all definitions of the given name.
		/// </summary>
		/// <returns></returns>
		/// <remarks>The names are assumed to delimited for properties, 
		/// sets, and non-terminals. terminal names are not delimited.
		/// Names are also not case sensitive.
		/// </remarks>
		public IEnumerable<DefinitionNode> FindDefinitionsByName(string name)
		{
			return definitions.Where (def => string.Compare (def.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
		}
		

		/// <summary>
		/// Gets the token at index.
		/// </summary>
		/// <returns>The <see cref="bsn.GoldParser.Parser.Token"/>.</returns>
		/// <param name="index">Index.</param>
		public Token GetTokenAt(int index)
		{
			Token result = null;
			foreach (Token token in tokenList)
			{
				int startPos = (int)token.Position.Index;
				int endPos = startPos + token.Text.Length - 1;
				if (startPos <= index && endPos >= index)
				{
					result = token;
					break;
				}

				if (startPos > index)
				{
					//its not here. stop looking
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Finds the definition that the given document index corresponds to.
		/// </summary>
		/// <returns>The <see cref="GoldAddin.DefinitionNode"/>. Will return null if 
		/// no definition exists at the given index</returns>
		/// <param name="index">document index</param>
		public DefinitionNode GetDefinitionAt(int index)
		{
			//this is the tricky one....
			//definition nodes need a known length, or a last-index
			// more tricky for non-terminal definitions, since they
			// can span multiple lines
			DefinitionNode result = null;
			foreach (DefinitionNode def in definitions)
			{
				int startPos = def.Index;
				int endPos = startPos + def.Length;
				if (startPos <= index && endPos >= index)
				{
					result = def;
					break;
				}
				if (startPos > index)
					break;
			}
			return result;
		}

		

		/// <summary>
		/// Gets all instances of tokens that match the given name. Assumes that name is delimited
		/// e.g., {set}, <non-terminal>, etc...
		/// </summary>
		/// <returns>The uses of.</returns>
		/// <param name="name">Name.</param>
		public IEnumerable<Token> FindUsesOf(string name)
		{
			return tokenList.Where (token => string.Compare (token.Text, name, StringComparison.OrdinalIgnoreCase) == 0);
		}

		/// <summary>
		/// Gets the tokens in the document that match the given type
		/// </summary>
		/// <returns>The tokens by type.</returns>
		/// <param name="symbolType">Symbol type.</param>
		public IEnumerable<Token> FindTokensByType(DefinitionType symbolType)
		{
			return tokenList.Where (token => TokenUtil.GetDefinitionType (token) == symbolType);
		}


		static bool isDefinitionStart(Token token, Token previous)
		{
			return TokenUtil.IsNewLine (previous) && (TokenUtil.IsUserDefinitionToken (token) || !isValidLineContinuationToken (token));
		}

		//only some tokens are allowed to follow a new line
		static bool isValidLineContinuationToken(Token token)
		{
			return token != null &&
				(token.Symbol.Name == "=" ||
				token.Symbol.Name == "::=" ||
				token.Symbol.Name == "|" ||
				token.Symbol.Name == "+" ||
				token.Symbol.Name == "-" ||
				TokenUtil.IsUserDefinitionToken (token));
		}
			
	}

}
