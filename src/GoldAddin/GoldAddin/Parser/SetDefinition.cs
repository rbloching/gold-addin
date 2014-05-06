using bsn.GoldParser.Parser;

namespace GoldAddin
{
	public class SetDefinition : DefinitionNode
	{
		public SetDefinition (string name, LineInfo location, int length) : base(name,location,length)
		{
		}

		public override DefinitionType Type 
		{
			get 
			{
				return DefinitionType.SetName;
			}
		}

	}
}

