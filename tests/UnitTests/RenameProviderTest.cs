using System;
using NUnit.Framework;
using GoldAddin;
using Mono.TextEditor;
using System.Collections.Generic;

namespace UnitTests
{
	[TestFixture]
	public class RenameProviderTest
	{
		//checks correct number of elements are returned
		[Test]
		public void GetSegmentsToRenameTest ()
		{
			string text = "";
			RenameProvider target;
			ICollection<TextSegment> segmentList;

			//Empty/bad input
			//-------------------------------------------
			text = "";
			target = new RenameProvider (text,0);
			segmentList = target.SegmentsToRename;
			Assert.AreEqual (0, segmentList.Count);


			text = string.Empty;
			target = new RenameProvider (text,0);
			segmentList = target.SegmentsToRename;
			Assert.AreEqual (0, segmentList.Count);

			text = null;
			target = new RenameProvider (text,0);
			segmentList = target.SegmentsToRename;
			Assert.AreEqual (0, segmentList.Count);

			//Happy cases
			//-------------------------------------

			text        = "<expr>!*some comments*!     {myset}\r\n" +
						  "keyword = 'keyword' <expr>!another comment\r\n"+
						  "myterm = '<'{myset}'>'keyword";

			//in <expr>
			target = new RenameProvider (text, 1);
			segmentList = target.SegmentsToRename;
			Assert.AreEqual (2, segmentList.Count);

			//in keyword
			target = new RenameProvider (text, 40);
			segmentList = target.SegmentsToRename;
			Assert.AreEqual (2, segmentList.Count);

			//in 'keyword' (terminal literal)
			target = new RenameProvider (text, 50);
			segmentList = target.SegmentsToRename;
			Assert.AreEqual (0, segmentList.Count);
		}


		//check that correct active segment is set
		[Test]
		public void GetSegmentsToRenameTest2()
		{
			string text = "";
			RenameProvider target;
			TextSegment segment = new TextSegment ();

			text        = "<expr>!*some comments*!     {myset}\r\n" +
						"keyword = 'keyword' <expr>!another comment\r\n"+
						"myterm = '<'{myset}'>'keyword";
			//in <expr>
			target = new RenameProvider (text, 1);
			segment = target.SelectedTextSegment;
			Assert.AreEqual ("expr".Length, segment.Length);
			Assert.AreEqual (1, segment.Offset);


			//in keyword - cursor is at 40, and item starts at 37
			target = new RenameProvider (text, 40);
			segment = target.SelectedTextSegment;
			Assert.AreEqual ("keyword".Length, segment.Length);
			Assert.AreEqual (37, segment.Offset); 

			//in {myset}
			target = new RenameProvider (text, 29);
			segment = target.SelectedTextSegment;
			Assert.AreEqual ("myset".Length, segment.Length);
			Assert.AreEqual (29, segment.Offset);

			//in <expr> (2nd occurence)
			target = new RenameProvider (text, 60);
			segment = target.SelectedTextSegment;
			Assert.AreEqual ("expr".Length, segment.Length);
			Assert.AreEqual (58, segment.Offset);
		}

		[Test]
		public void SelectedSymbolTypeTest()
		{
			string text = "";
			RenameProvider target;
			DefinitionType symbolType;

			text        = "<expr>!*some comments*!     {myset}\r\n" +
				"keyword = 'keyword' <expr>!another comment\r\n"+
					"myterm = '<'{myset}'>'keyword";
			//in <expr>
			target = new RenameProvider (text, 1);
			symbolType = target.SelectedSymbolType;
			Assert.AreEqual(DefinitionType.NonTerminal,symbolType);

			//in {myset}
			target = new RenameProvider (text, 29);
			symbolType = target.SelectedSymbolType;
			Assert.AreEqual(DefinitionType.SetName,symbolType);

			//in keyword - cursor is at 40, and item starts at 37
			target = new RenameProvider (text, 40);
			symbolType = target.SelectedSymbolType;
			Assert.AreEqual(DefinitionType.Terminal,symbolType);
	
		}
	}
}

