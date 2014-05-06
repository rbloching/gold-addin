using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Gui.Content;
using Mono.TextEditor;

namespace GoldAddin
{
	/// <summary>
	/// Code completion functionality
	/// </summary>
	public class GoldCompletionExtension : CompletionTextEditorExtension
	{
		///<summary>
		/// Returns a list of completion suggestions to display while editing a document 
		/// </summary>
		public override ICompletionDataList HandleCodeCompletion (CodeCompletionContext completionContext, char completionChar, ref int triggerWordLength)
		{
			//When we are in a different editing mode do not provide completions
			if (Editor.Parent.CurrentMode is TextLinkEditMode)
			{
				return null;
			}


			var doc = Document.ParsedDocument as GoldParsedDocument;
			var completionProvider = new CompletionProvider (doc, Editor.Text);

			//ICompletionProvider completionProvider = new CompletionProvider (Editor.Text);
			ICompletionDataList completionList = completionProvider.GetCompletionList (Editor.Caret.Offset-1);


			if (completionList != null)
			{
				//the triggerWordLength sets the length of the text already entered.
				// so when the user types < the length so far is 1, and the first < will
				// be highlighted for every suggestion in the completion list
				triggerWordLength = 1;
			}
			return completionList;
		}	
	}
}

