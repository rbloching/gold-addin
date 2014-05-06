using bsn.GoldParser.Parser;

namespace GoldAddin
{

	public class NonTerminalDefinition : DefinitionNode 
	{

		public NonTerminalDefinition (string name, LineInfo location, int length) : base(name,location,length)
		{
		}

		public override DefinitionType Type 
		{
			get
			{
				return DefinitionType.NonTerminal;
			}
		}


	}
}

