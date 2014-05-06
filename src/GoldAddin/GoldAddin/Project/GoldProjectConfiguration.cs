using System;
using MonoDevelop.Projects;
using MonoDevelop.Core.Serialization;

namespace GoldAddin
{
    public class GoldProjectConfiguration : ProjectConfiguration
    {        
		[ItemProperty("OutputFormat")]
		GrammarTableFormat outputFormat;

		[ItemProperty("GrammarTableName")]
		String grammarTableName;

		/// <summary>
		/// Gets or sets the output format (.cgt, .egt) for a project
		/// </summary>
		/// <value>The output format.</value>
		public GrammarTableFormat OutputFormat
		{
			get{ return outputFormat;}
			set{ outputFormat = value;}
		}

		/// <summary>
		/// Gets or sets the name of the compiled grammar table.
		/// </summary>
		/// <value>The name of the grammar table.</value>
		public String GrammarTableName
		{
			get{ return grammarTableName; }
			set{ grammarTableName = value; }
		}


		/// <summary>
		/// Copies from given configuration
		/// </summary>
		/// <param name="conf">other configuration. expected to be a GoldProjectConfiguration</param>
		public override void CopyFrom (ItemConfiguration conf)
		{
			if (conf == null)
				return;

			base.CopyFrom (conf);
			var other = (GoldProjectConfiguration)conf;
			outputFormat = other.outputFormat;
			grammarTableName = other.grammarTableName;

		}

    }

	public enum GrammarTableFormat
	{
		CompiledGrammarTable,
		EnhancedGrammarTable
	}
}
