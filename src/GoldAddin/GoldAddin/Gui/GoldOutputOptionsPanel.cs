using MonoDevelop.Ide.Gui.Dialogs;

namespace GoldAddin
{
	/// <summary>
	/// Gold item options panel.
	/// </summary>
	public class GoldOutputOptionsPanel :  MultiConfigItemOptionsPanel 
	{
		OutputOptionsWidget optionsWidget;

		public override Gtk.Widget CreatePanelWidget ()
		{
			optionsWidget= new OutputOptionsWidget();
			optionsWidget.ShowAll ();
			return optionsWidget;
		}

		public override void ApplyChanges ()
		{
			optionsWidget.Store ();
		}


		//this is called when the current configuration changes
		public override void LoadConfigData ()
		{
			optionsWidget.Load ((GoldProjectConfiguration)CurrentConfiguration);
		}
	}
}

