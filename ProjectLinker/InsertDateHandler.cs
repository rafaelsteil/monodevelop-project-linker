using System;
using Mono.TextEditor;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace ProjectLinker
{
	public enum DateInserterCommands
	{
		InsertDate
	}

	public class InsertDateHandler : CommandHandler
	{
		protected override void Run() {
			/*
			Document doc = IdeApp.Workbench.ActiveDocument;
			var data = doc.GetContent<ITextEditorDataProvider>().GetTextEditorData();
			string date = DateTime.Now.ToString();
			data.InsertAtCaret(date); 
			 * */
		}

		// This method is queried whenever a command is shown in a menu or executed via keybindings
		protected override void Update(CommandInfo info) {
			Document doc = IdeApp.Workbench.ActiveDocument;
			info.Enabled = doc != null && doc.GetContent<ITextEditorDataProvider>() != null;
		}
	}
}
