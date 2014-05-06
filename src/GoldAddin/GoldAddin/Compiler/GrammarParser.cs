using System.IO;
using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace GoldAddin
{
	/// <summary>
	/// Provides full syntax parsing of text written in the GOLD meta-language
	/// </summary>
	public class GrammarParser : IParser
	{
		static readonly CompiledGrammar goldGrammar;
		static GrammarParser()
		{
			goldGrammar = GoldGrammarTable.GetCompiledGrammar (CgtVersion.None);
		}


		public event CompilerEventHandler ErrorFound;
			
		/// <summary>
		/// Parses the specified input.
		/// </summary>
		/// <param name="input">Input.</param>
		public void Parse(string input)
		{
			var parser = getParser (input);
			ParseMessage msg;
			Token token;

			msg = parser.ParseAll ();
			token = parser.CurrentToken;
			if (msg == ParseMessage.LexicalError)
			{
				var result = toErrorMessage ("Cannot recognize symbol " + token.Text, token);				                                    
				onErrorFound (result);					                                   
			}
			else if (msg == ParseMessage.SyntaxError)
			{
				string symbolStr = "symbol";
				if (token.Symbol.Name != token.Text)
					symbolStr = token.Symbol.Name;

				var result = toErrorMessage (
					"Unexpected " + symbolStr + " " + token.Text,
					token);

				onErrorFound (result);
				result = toErrorMessage (getExpectedTokensMessage (parser), token);
					
				onErrorFound (result);
			}
			else if (msg == ParseMessage.BlockError)
			{
				var result = toErrorMessage ("Unterminated " + token.Symbol.Name, token);
				onErrorFound (result);
			}
		}


		static LalrProcessor getParser(string input)
		{
			string str = input ?? string.Empty;
			var tokenizer = new Tokenizer(new StringReader(str), goldGrammar);
			return new LalrProcessor (tokenizer);
		}

		static bool isErrorMessage(ParseMessage parseMessage)
		{
			return 
				parseMessage == ParseMessage.BlockError ||
				parseMessage == ParseMessage.InternalError ||
				parseMessage == ParseMessage.LexicalError ||
				parseMessage == ParseMessage.SyntaxError;
		}

		void onErrorFound(CompilerMessage message)
		{
			var copy = ErrorFound;
			if (copy != null)
				copy (message);
		}

		static readonly char[] listSeparators = { ',', ' ' };
		static string getExpectedTokensMessage(LalrProcessor parser)
		{
			var expectedSymbols = parser.GetExpectedTokens (); //actually gets a list of symbols, not tokens
			if (expectedSymbols.Count == 0)
				return string.Empty;

			string result = "Expected one of the following: ";
			foreach (Symbol symbol in expectedSymbols)
			{
				result = result + getDisplayTextForSymbol(symbol.Name) + ", ";
			}
			//remove last comma
			result = result.TrimEnd (listSeparators);
			return result;
		}

		//gets the first character expected for the symbol in the case its a definition.
		// this is to provide a cleaner error message for an "Expecting:..." message.
		static string getDisplayTextForSymbol(string symbolName)
		{
			switch (symbolName)
			{			
			case"ParameterName":
				return "\"";
			case "Nonterminal":
					return "<";
			case "SetLiteral":
					return "[";
			case "SetName":
					return "{";
			default:
				return symbolName;
			}
		}	

		static CompilerMessage toErrorMessage(string message, Token token)
		{
			return new CompilerMessage (MessageSeverity.Error,
			                            message,
			                            string.Empty,
			                            token.Position.Line, 
			                            token.Position.Column);
		}

	}
}

