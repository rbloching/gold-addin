using System;
using NUnit.Framework;
using GoldAddin;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Grammar;

namespace UnitTests
{
	[TestFixture]
	public class TokenizerTest
	{
		[Test]
		public void EmptyInputTest()
		{
			string input;
			GrammarTokenizer target;
			Token token;

			input = String.Empty;
			target = new GrammarTokenizer (input);
			token = target.NextToken ();
			Assert.AreEqual (SymbolKind.End, token.Symbol.Kind);

			input = null;
			target = new GrammarTokenizer (input);
			token = target.NextToken ();
			Assert.AreEqual (SymbolKind.End, token.Symbol.Kind);

		}

		[Test]
		public void NextTokenTest()
		{
			string input;
			GrammarTokenizer target;
			Token token;

			input = "\0";
			target = new GrammarTokenizer (input);
			token = target.NextToken ();
			Assert.AreEqual (SymbolKind.Error, token.Symbol.Kind);

			token = target.NextToken ();
			Assert.AreEqual (SymbolKind.End, token.Symbol.Kind);


			input = "%{set}";
			target = new GrammarTokenizer (input);
			token = target.NextToken ();
			Assert.AreEqual (SymbolKind.Error, token.Symbol.Kind);
			token = target.NextToken ();
			Assert.AreEqual (SymbolKind.Terminal, token.Symbol.Kind);
		}

		[Test]
		public void NextTokenIgnoreNoiseTest()
		{
			string input;
			GrammarTokenizer target;
			Token token;

			input = "!* block comment \r\n*!<nonTerminal>";
			target = new GrammarTokenizer (input);
			token = target.NextTokenIgnoreNoise ();
			Assert.AreEqual (SymbolKind.Terminal, token.Symbol.Kind);


			input = "!* block comment \r\n<nonTerminal>";
			target = new GrammarTokenizer (input);
			token = target.NextTokenIgnoreNoise ();
			Assert.AreEqual (SymbolKind.End, token.Symbol.Kind);
		}
	}
}

