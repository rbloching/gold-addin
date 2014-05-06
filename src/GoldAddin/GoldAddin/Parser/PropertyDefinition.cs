using bsn.GoldParser.Parser;

namespace GoldAddin
{
	public class PropertyDefinition : DefinitionNode
	{
		public PropertyDefinition (string name, LineInfo location, int length) : base(name,location,length)
		{
		}

		public override DefinitionType Type 
		{
			get 
			{
				return DefinitionType.Property;
			}
		}
	}
}

