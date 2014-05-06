using MonoDevelop.Core;

namespace GoldAddin
{
	public partial class CompletionProvider
	{
		static void initIcons()
		{
			iconMap.Add(DefinitionType.Property,new IconId("goldPropertyIcon"));
			iconMap.Add(DefinitionType.Terminal,new IconId("goldTerminalIcon"));
			iconMap.Add(DefinitionType.NonTerminal,new IconId("goldNonTerminalIcon"));
			iconMap.Add (DefinitionType.SetName, new IconId ("goldSetIcon"));
		}


		static void addCharacterSet(string name)
		{
			PreDefinedCharacterSet.Add (newCompletionData ("{"+name+"}", DefinitionType.SetName));
		}

		static void addProperty(string name)
		{
			PreDefinedProperties.Add (newCompletionData ("\""+name+"\"", DefinitionType.Property));
		}

		static void initProperties()
		{
			addProperty ("Name");
			addProperty ("Version");
			addProperty ("Author");
			addProperty ("About");
			addProperty ("Case Sensitive");
			addProperty ("Character Mapping");
			addProperty ("Auto Whitespace");
			addProperty ("Virtual Terminals");
			addProperty ("Start Symbol");
		}

		static void initCharacterSets()
		{
			addCharacterSet ("HT");
			addCharacterSet ("LF");
			addCharacterSet ("VT");
			addCharacterSet ("FF");
			addCharacterSet ("CR");
			addCharacterSet ("Space");
			addCharacterSet ("NBSP");
			addCharacterSet ("LS");
			addCharacterSet ("PS");
			addCharacterSet ("Number");
			addCharacterSet ("Digit");
			addCharacterSet ("Letter");
			addCharacterSet ("AlphaNumeric");
			addCharacterSet ("Printable");
			addCharacterSet ("LetterExtended");
			addCharacterSet ("PrintableExtended");
			addCharacterSet ("WhiteSpace");
			addCharacterSet ("All Latin");
			addCharacterSet ("All Letters");
			addCharacterSet ("All Printable");
			addCharacterSet ("All Space");
			addCharacterSet ("All Newline");
			addCharacterSet ("All WhiteSpace");
			addCharacterSet ("All Valid");
			addCharacterSet ("ANSI Mapped");
			addCharacterSet ("ANSI Printable");
			addCharacterSet ("Control Codes");
			addCharacterSet ("Euro Sign");
			addCharacterSet ("Formatting");
		}
	}
}

