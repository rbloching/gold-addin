using System;

namespace GoldAddin
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OutputOptionsWidget : Gtk.Bin
	{
		GoldProjectConfiguration config;

		public OutputOptionsWidget ()
		{
			this.Build ();
		}

		public void Load(GoldProjectConfiguration projectConfiguration)
		{
			this.config = projectConfiguration;
			outputTableName.Text = config.GrammarTableName;
			outputFormatSelector.Active = getIndexFromFormat (config.OutputFormat);
		}

		public void Store()
		{
			config.GrammarTableName = outputTableName.Text;
			config.OutputFormat = getFormatFromIndex (outputFormatSelector.Active);
		}
			
		
		static GrammarTableFormat[] formatArray = 
			{GrammarTableFormat.CompiledGrammarTable, GrammarTableFormat.EnhancedGrammarTable};

		static int getIndexFromFormat(GrammarTableFormat format)
		{
			if (format == GrammarTableFormat.CompiledGrammarTable)
				return 0;
			return 1;
		}	

		static GrammarTableFormat getFormatFromIndex(int index)
		{
			return formatArray [index];
		}

	}
}

