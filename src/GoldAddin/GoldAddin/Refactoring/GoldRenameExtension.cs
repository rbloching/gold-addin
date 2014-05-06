using System;
using System.Collections.Generic;
using Mono.TextEditor;
using Mono.TextEditor.PopupWindow;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui.Content;

namespace GoldAddin
{
	//Most of the following was stolen from the MD RenameRefactoring addin

	/// <summary>
	/// Rename refactoring functionality
	/// </summary>
	public class GoldRenameExtension : TextEditorExtension
	{
		IRenameProvider renameProvider;
		
		//this method will intercept the Edit Rename command
		[CommandHandler(EditCommands.Rename)]
		public void RenameCommand()
		{
			if (!renameProvider.HasSegments)
				return; //nothing to rename

			var textSegments = renameProvider.SegmentsToRename;
			TextSegment primarySegment = renameProvider.SelectedTextSegment;

			var link = new TextLink ("name"); //not sure why the link has to be named
			var links = new List<TextLink> ();

			//make sure that the segment where the cursor is is first in the list
			// othewise the document will jump to the location of the first link
			link.AddLink (primarySegment); 

			foreach(TextSegment segment in textSegments)
			{
				if (segment != primarySegment)
					link.AddLink (segment);
			}
			links.Add (link);


			var mode = new TextLinkEditMode (Editor.Parent, 0, links);
			mode.SetCaretPosition = false;
			mode.SelectPrimaryLink = true;

			mode.OldMode = Editor.Parent.CurrentMode;
			mode.Cancel += textLinkEditorCanceled;
			mode.HelpWindow = getHelpWindowFor (renameProvider.SelectedSymbolType);
			mode.StartMode ();
			Editor.Parent.CurrentMode = mode;

		}

		//This is called before the command handler is invoked, when menus are displayed.
		// Makes it possible to show/hide rename option
		[CommandUpdateHandler(EditCommands.Rename)]
		public void RenameCommandUpdate(CommandInfo commandInfo)
		{
			int offset = Editor.Caret.Offset;
			renameProvider = new RenameProvider (Editor.Text, offset);
			commandInfo.Visible = renameProvider.HasSegments;
		}
		
		static void textLinkEditorCanceled(object sender, EventArgs e)
		{
			var textLinkEditor = (TextLinkEditMode)sender;
			if(textLinkEditor.HasChangedText)
				textLinkEditor.Editor.Document.Undo ();
		}

		static TableLayoutModeHelpWindow getHelpWindowFor(DefinitionType symbolType)
		{
			var helpWindow = new TableLayoutModeHelpWindow ();
			helpWindow.TitleText = "<b>Rename " + getDescriptionOfSymbol(symbolType)+"</b>";
			helpWindow.Items.Add (new KeyValuePair<string, string> ("<b>Key</b>", "<b>Behavior</b>"));
			helpWindow.Items.Add (new KeyValuePair<string, string> ("<b>Enter</b>", "<b>Accept</b> this refactoring"));
			helpWindow.Items.Add (new KeyValuePair<string, string> ("<b>ESC</b>", "<b>Cancel</b> this refactoring"));
			return helpWindow;
		}

		static string getDescriptionOfSymbol(DefinitionType symbolType)
		{
			switch (symbolType)
			{
			case DefinitionType.NonTerminal:
				return "Non-Terminal";
			case DefinitionType.SetName:
				return "Character Set";
			case DefinitionType.Terminal:
				return "Terminal";
			default:
				return string.Empty;
			}
		}
		

		
	}

}

