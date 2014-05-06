using NUnit.Framework;
using GoldAddin;
using MonoDevelop.Ide.CodeCompletion;


namespace UnitTests
{
	[TestFixture]
	public class CompletionProviderTest
	{
		[Test]
		public void NonTerminalListTest()
		{
			string text = "<s-Expression> ::= <Quote> Atom\r\n" + //index: 0-32
						  "\t| <Quote> '(' <Series> ')'\r\n" +  //index: 33-61
						  "errorline<Value\r\n"+			    //index: 62- 78
						  "<";
			var target = new CompletionProvider (text);
			int nonTermCount = 3; //<s-Expression>, <Quote>, and <Series>

			//case 1: < at new line
			var completionList = target.GetCompletionList (79);
			Assert.AreEqual (nonTermCount, completionList.Count); 

			//case 2: < inside rule declaration (first line)
			completionList = target.GetCompletionList (19);
			Assert.AreEqual(nonTermCount+1, completionList.Count); //+1 for <>

			//case 3: < inside rule declaration (continuation line)
			completionList = target.GetCompletionList (36);
			Assert.AreEqual(nonTermCount+1, completionList.Count); //+1 for <>

			//case 4: < inside bad syntax line 
			completionList = target.GetCompletionList (71);
			Assert.AreEqual(null, completionList);
		}

		[Test]
		public void InCommentsTest()
		{
			string text = 	"!*<*!\"\r\n" +
							"!*{*!{<value>::=<expr>\r\n" +
							"\t    <\r\n";

			var target = new CompletionProvider (text);

			//completion should not show up while inside a comment
			var completionList = target.GetCompletionList (2);
			Assert.IsNull (completionList);

			completionList = target.GetCompletionList (10);
			Assert.IsNull (completionList);

			text = "<A>::=a !<A>";
			target = new CompletionProvider (text);
			completionList = target.GetCompletionList (9);
			Assert.IsNull (completionList);
		}

		[Test]
		public void TerminalListTest()
		{
			string text = 	"while = 'while'\n"+  		//index 0-15
						  	"integer = {Digit}+\n"+ 	//16-34
							"<expr>::= !*..*! integer\n"+ //35-59
							"integer = integer'.'integer"; //60-86
							


			ICompletionDataList completionList;
			int expectedTermCount = 2; //while, integer

			//case 1: on a new line, no completion
			var target = new CompletionProvider (text);
			completionList = target.GetCompletionList (16);
			Assert.IsNull (completionList);


			//case 2: in terminal literal, no completion
			completionList = target.GetCompletionList (9);
			Assert.IsNull (completionList);


			//case 3: alpha  in a rule declaration
			completionList = target.GetCompletionList (52);
			Assert.NotNull(completionList);
			//list should only have "while" and "integer", but NOT "'while'" (terminal literal)
			Assert.AreEqual(expectedTermCount,completionList.Count); 

			//case 4: alpha in a terminal declaration
			completionList = target.GetCompletionList (72);
			Assert.AreEqual (expectedTermCount, completionList.Count);

			//verify what's in the list
			Assert.AreEqual ("while", completionList [0].DisplayText);
			Assert.AreEqual ("integer", completionList [1].DisplayText);

		}


		[Test]
		public void SetNameListTest()
		{
			string text = "{set}=[xyz]\n" + //0-11
						"int = {digit}+\n" + //12-26
						"{digit} = {set}\n"; //27-42
							

			var target = new CompletionProvider (text);
			ICompletionDataList completionList;
			int predefinedSetCount = 29;  //the list will return several others that are defined in the meta language
			int expectedSetCount = predefinedSetCount + 2; //include {set} and {digit}

			//case 1: no completion for lhs of assignment 
			completionList = target.GetCompletionList (0);
			Assert.IsNull (completionList);

			//case 2: { in a terminal declaration
			completionList = target.GetCompletionList (18);
			Assert.AreEqual (expectedSetCount, completionList.Count);

			//case 3: { in a set declaration
			completionList = target.GetCompletionList (37);
			Assert.AreEqual (expectedSetCount, completionList.Count);

			//verify what's in the list
			Assert.IsTrue(completionListContains(completionList,"{set}"));
			Assert.IsTrue (completionListContains (completionList, "{digit}"));

		}

		[Test]
		public void NoListInLiteralTest()
		{
			//A list should not be created when typing in a 
			// terminal literal

			string text;
			ICompletionDataList completionList;

			//start of a terminal literal - should be no list
			text = "keyword = !*'*!'inaliteral";
			var target = new CompletionProvider (text);
			completionList = target.GetCompletionList (16); //at the 'i'
			Assert.IsNull (completionList);
		}


		[Test]
		public void NoListInBlockCommentTest()
		{
			//A list should not be created when the context is a 
			// block comment

			string text;
			ICompletionDataList completionList;


			text="!* start of block\r\n"+
				"<expr";
			var target = new CompletionProvider (text);
			completionList = target.GetCompletionList (19); //at the '<'
			Assert.IsNull (completionList);
		}

		[Test]
		public void NonTerminalDeclarationTest()
		{
			//Typing a < on a new line should bring up a non-terminal list
			string text = 	"int = {Digit}+\r\n" + //0-15
							"<\r\n" + //16-18
							"<value>::=int";
			var target = new CompletionProvider (text);
			var list = target.GetCompletionList (16);
			Assert.IsNotNull (list);
			//list should contain <value>
			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("<value>", list [0].DisplayText);
		}

		[Test]
		public void OnlyDefinedTerminalTest()
		{
			//the list of terminals should contain only what has been defined
			// in the grammar
			string text = 	"int = {Digit}+\r\n" + //0-15
							"<value>::=i"; //16-26
			var target = new CompletionProvider (text);
			var list = target.GetCompletionList (26);
			Assert.IsNotNull (list);
			//list should contain int, and only int. 'i' should not be there
			Assert.AreEqual (1, list.Count);
			Assert.AreEqual ("int", list [0].DisplayText);
		}

		[Test]
		public void OnlyDefinedSetsTest()
		{
			//the list of sets should contain only what has been defined
			// in the grammar
			string text = "{hex ch}= {Digit}+[abcdef]\r\n" + //0-27
				"hexnum = {\r\n" + //28-39
				"{notdefined}";

			var target = new CompletionProvider (text);
			var list = target.GetCompletionList (37);
			Assert.IsNotNull (list);

			//The list of sets will contained built-in sets also, so
			// we can't just check the count..
			//The list should NOT contain {notdefined}
			Assert.IsTrue(completionListContains(list,"{hex ch}"));
			Assert.IsFalse(completionListContains(list,"{notdefined}"));


		}


		[Test]
		public void NullableInRuleDeclarationTest()
		{
			//<> should show up in a rule declaration
			string text = 	"int = {Digit}+\r\n" + //0-15
							"<value>::=<"; //16-26
			var target = new CompletionProvider (text);
			var list = target.GetCompletionList (26);
			Assert.IsNotNull (list);
			Assert.IsTrue (completionListContains (list, "<>"));
		}

		[Test]
		public void NullableInNewLineTest()
		{
			//<> should NOT show up when starting a new line
			string text = 	"int = {Digit}+\r\n" + //0-15
							"<\r\n" +
							"<value>::=<x>";
					
			var target = new CompletionProvider (text);
			var list = target.GetCompletionList (16);
			Assert.IsNotNull (list);
			Assert.IsFalse (completionListContains (list, "<>"));
		}

		static bool completionListContains(ICompletionDataList completionList, string expected)
		{
			foreach (CompletionData completionItem in completionList)
			{
				if (completionItem.DisplayText == expected)
					return true;
			}
			return false;
		}
	}
}

