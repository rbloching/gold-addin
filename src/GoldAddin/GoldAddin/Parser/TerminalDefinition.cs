using bsn.GoldParser.Parser;

namespace GoldAddin
{
	public class TerminalDefinition : DefinitionNode
	{
		public TerminalDefinition (string name, LineInfo location, int length) : base(name,location,length)
		{
		}

		public override DefinitionType Type 
		{
			get 
			{
				return DefinitionType.Terminal;
			}
		}
	}
}

