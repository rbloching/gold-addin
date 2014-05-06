using System;
using NUnit.Framework;
using GoldAddin;
using bsn.GoldParser.Grammar;
using System.Collections.Generic;

namespace UnitTests
{
	public class ParsedGrammarDocumentTest
	{
		[Test]
		public void FindTerminalDeclarationsTest()
		{
			//case 1: terminal defined once
			string text = "if='if'\r\n"+
						  "<expr>::=<expr>'+'<add>";

			var target = new GoldParsedDocument ();
			target.Parse (text);
			var declList = target.FindDefinitionsByType(DefinitionType.Terminal);
			Assert.NotNull (declList);
			var enumerator = declList.GetEnumerator ();

			//expecting 1 terminal definition
			// put into a sequence
			var actualList = new List<DefinitionNode> ();
			while (enumerator.MoveNext())
			{
				actualList.Add (enumerator.Current);
			}

			Assert.AreEqual (1, actualList.Count);
			Assert.AreEqual (DefinitionType.Terminal, actualList [0].Type);

			//case 2: terminal contains redefinition
			text = "if='if'\r\n"+
					"<expr>::=<expr>'+'<add>\r\n"+
					"if = 'IF'";
			target = new GoldParsedDocument ();
			target.Parse (text);
			enumerator = target.FindDefinitionsByType (DefinitionType.Terminal).GetEnumerator ();
			actualList.Clear ();
			while (enumerator.MoveNext())
			{
				actualList.Add (enumerator.Current);
			}

			Assert.AreEqual (2, actualList.Count);
		}

		[Test]
		public void TerminalLiteralsNotConfusedForTerminalsTest()
		{
			string text = "'if'='if'\r\n"+
				"<expr>::=<expr>'+'<add>";
			var target = new GoldParsedDocument ();
			target.Parse (text);
			var enumerator = target.FindDefinitionsByType (DefinitionType.Terminal).GetEnumerator ();

			//should be no results
			Assert.IsFalse (enumerator.MoveNext ());
		}

		[Test]
		public void FindDefinitionsByNameTest()
		{
			string text = "if='if'\r\n"+
				"<expr>::=<expr>'+'<add>\r\n"+
					"if = 'IF'";

			var target = new GoldParsedDocument ();
			target.Parse (text);
			var enumerator = target.FindDefinitionsByName ("if").GetEnumerator ();

			var actualList = new List<DefinitionNode> ();
			while (enumerator.MoveNext())
			{
				actualList.Add (enumerator.Current);
			}

			Assert.AreEqual (2, actualList.Count);
			Assert.AreEqual ("if", actualList [0].Name);
			Assert.AreEqual ("if", actualList [1].Name);
		}

		[Test]
		public void FindDefinitionsByNameDifferentCaseTest()
		{
			string text = "\"Start symbol\" = <start>";
			var target = new GoldParsedDocument ();
			target.Parse (text);

			var enumerator = target.FindDefinitionsByName("\"START SYMBOL\"").GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext ());
		}

		[Test]
		public void FindUsesOfTest()
		{
			string text = "if='if'\r\n"+
						  "<expr>::=<Expr>'+'<add>\r\n"+
						  "<add>::=x!*<mul>if*! !<mul>\r\n"+
						  "if = 'IF'";

			var target = new GoldParsedDocument ();
			target.Parse (text);
			var enumerator = target.FindUsesOf ("<expr>").GetEnumerator();
			//should be exactly 2 uses (ignored case)
			Assert.IsTrue (enumerator.MoveNext ());
			Assert.IsTrue (enumerator.MoveNext ());
			Assert.IsFalse (enumerator.MoveNext ());

			enumerator = target.FindUsesOf ("<mul>").GetEnumerator();
			//should find none because it was in a comment
			Assert.IsFalse (enumerator.MoveNext ());
		}

		[Test]
		public void GetTokenAtTest()
		{
			string text = "if='if'\r\n"+
				"<expr>::=<Expr>'+'<add>\r\n"+
					"<add>::=x!*<mul>if*! !<mul>\r\n"+
					"if = 'IF'";
			var target = new GoldParsedDocument ();
			target.Parse (text);
			var token = target.GetTokenAt (19);
			string expected = "<Expr>";
			string actual = token.Text;
			Assert.AreEqual (expected, actual);

		}

		[Test]
		public void GetDefinitionAtTest()
		{
			//case 1: index on a continuation
			string text = 
				"<A>::=<B>\r\n"+ //0-10
				"|<C>";			//11-14
			string expected, actual;
			var target = new GoldParsedDocument ();
			target.Parse (text);
			var def = target.GetDefinitionAt (11);
			Assert.IsNotNull (def);

			expected = "<A>";
			actual = def.Name;
			Assert.AreEqual (expected, actual);


			//case 2: index where there is no definition
			text = 
				"<A>::=<B>\r\n" + //0-10
				"|<C>\r\n" + //11-16
				"(?\r\n" + //17-20
				"{myset}=[abc]";//21-33
			target = new GoldParsedDocument ();
			target.Parse (text);
			def = target.GetDefinitionAt (17);
			Assert.IsNull (def);


			//case 3: happy case
			def = target.GetDefinitionAt (33);
			Assert.IsNotNull (def);
			expected = "{myset}";
			actual = def.Name;
			Assert.AreEqual (expected, actual);

			//case 4: in comments
			text = "!*<A>::=a*!";
			target = new GoldParsedDocument ();
			target.Parse (text);
			def = target.GetDefinitionAt (2);
			Assert.IsNull (def);
		}

		[Test]
		public void FindTokenByTypeTest()
		{
			string text = 
				"<A>::=<B>\r\n" + //0-10
				"|<C>\r\n" + //11-16
				"(?\r\n" + //17-20
				"{myset}=[abc]";//21-33

			var target = new GoldParsedDocument ();
			target.Parse (text);
			var enumerator = target.FindTokensByType (DefinitionType.SetName).GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext ());
			Assert.AreEqual ("{myset}", enumerator.Current.Text);

			enumerator = target.FindTokensByType (DefinitionType.NonTerminal).GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext ());
			Assert.AreEqual("<A>",enumerator.Current.Text);
			Assert.IsTrue (enumerator.MoveNext ());
			Assert.AreEqual("<B>",enumerator.Current.Text);
			Assert.IsTrue (enumerator.MoveNext ());
			Assert.AreEqual("<C>",enumerator.Current.Text);
			Assert.IsFalse (enumerator.MoveNext ());

		}

	}
}

