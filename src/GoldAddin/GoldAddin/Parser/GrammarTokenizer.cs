using System.IO;
using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace GoldAddin
{

	/// <summary>
	/// Tokenizes a GOLD grammar document
	/// </summary>
	public class GrammarTokenizer 
	{
		static CompiledGrammar goldGrammar;

		static GrammarTokenizer()
		{
			goldGrammar = GoldGrammarTable.GetCompiledGrammar (CgtVersion.None);
		}


		Tokenizer tokenizer;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldAddin.GrammarTokenizer"/> class.
		/// </summary>
		/// <param name="input">Input.</param>
		public GrammarTokenizer(string input)
		{
			string str = input ?? string.Empty;
			tokenizer = new Tokenizer(new StringReader(str), goldGrammar);
		}
						

		/// <summary>
		/// Returns the next token in the document
		/// </summary>
		/// <remarks>The last token's symbol will have kind SymbolKind.End</remarks>
		public Token NextToken ()
		{
			Token token;		
			//bsn.goldparser tokenizer will instantiate token - won't be null
			tokenizer.NextToken (out token);
			return token;
		}



		/// <summary>
		/// Returns the next non-noise (whitespace, comments) token in the document
		/// </summary>
		/// <remarks>The last token's symbol will have kind SymbolKind.End</remarks>
		public Token NextTokenIgnoreNoise ()
		{
			Token token;
			do
			{
				token = NextToken ();
			} 
			while(TokenUtil.IsNoise(token));
			return token;
		}
		
	}
}

