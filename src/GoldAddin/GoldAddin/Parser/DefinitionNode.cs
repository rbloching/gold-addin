using bsn.GoldParser.Parser;

namespace GoldAddin
{
	/// <summary>
	/// Definition node.
	/// </summary>
	public abstract class DefinitionNode
	{
        LineInfo location;
        string name;
		int length;

		protected DefinitionNode(string name, LineInfo location, int length)
		{
			this.location = location;
			this.length = length;
			this.name = name;
		}

		/// <summary>
		/// Gets the name of this definition
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{ 
			get{ return name; } 
		}

		/// <summary>
		/// Gets the location in the document of this definition
		/// </summary>
		/// <value>The location.</value>
		public LineInfo Location
		{
			get{ return location;}
		}

		/// <summary>
		/// Gets this definition type
		/// </summary>
		/// <value>The type.</value>
		public abstract DefinitionType Type{ get; }


		/// <summary>
		/// Gets the length of the text used by this definition statement
		/// </summary>
		/// <value>The length.</value>
		public int Length
		{
			get{ return length;}
		}

		/// <summary>
		/// Gets the index in the document where this definition starts
		/// </summary>
		/// <value>The index.</value>
		public int Index
		{
			get{ return (int)(location.Index);}
		}
	}



	public enum DefinitionType
	{
		Property, SetName, Terminal, NonTerminal, None
	}
}

