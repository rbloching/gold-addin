using GoldAddin;
using NUnit.Framework;

namespace UnitTests
{
	public class GrammarTextTest
	{
		[Test]
		public void FindNonTerminalTest()
		{
			string str = "alkdjfa<non-terminal>asldkj";
			string expected, actual;
			expected = "<non-terminal>";
			actual = GrammarText.FindNonTerminalName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa<non terminal>asldkj";
			expected = "<non terminal>";
			actual = GrammarText.FindNonTerminalName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa<non.terminal>asldkj";
			expected = "<non.terminal>";
			actual = GrammarText.FindNonTerminalName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa<non_terminal>asldkj";
			expected = "<non_terminal>";
			actual = GrammarText.FindNonTerminalName (str);
			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void FindSetNameTest()
		{
			string str = "alkdjfa{setname}asldkj";
			string expected, actual;
			expected = "{setname}";
			actual = GrammarText.FindSetName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa{set!@#$W%^*() name}asldkj";
			expected = "{set!@#$W%^*() name}";
			actual = GrammarText.FindSetName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa{set{}name}asldkj";
			expected = string.Empty;
			actual = GrammarText.FindSetName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa{set}{name}asldkj";
			expected = "{set}";
			actual = GrammarText.FindSetName (str);
			Assert.AreEqual (expected, actual);
		}
	
		[Test]
		public void FindTerminalNameTest()
		{
			string str = "alkdjfa'terminal'asldkj";
			string expected, actual;
			expected = "terminal";
			actual = GrammarText.FindTerminalName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa'a12345terminal.-_'asldkj";
			expected = "a12345terminal.-_";
			actual = GrammarText.FindTerminalName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa'my terminal'asldkj";
			expected = string.Empty;
			actual = GrammarText.FindTerminalName (str);
			Assert.AreEqual (expected, actual);

			str = "Duplicate definition for the terminal 'if' :";
			expected = "if";
			actual = GrammarText.FindTerminalName (str);
			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void FindPropertyTest()
		{
			string str = "alkdjfa\"grammar property\"asldkj";
			string expected, actual;
			expected = "\"grammar property\"";
			actual = GrammarText.FindPropertyName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa\"grammar'property\"asldkj";
			expected = string.Empty;
			actual = GrammarText.FindPropertyName (str);
			Assert.AreEqual (expected, actual);

			str = "alkdjfa\"grammar\"property\"asldkj";
			expected = "\"grammar\"";
			actual = GrammarText.FindPropertyName (str);
			Assert.AreEqual (expected, actual);

		}
	}
}

