using System.Collections.Generic;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace ProjectLinker
{
	public class LinkProjectSettingsHandler : CommandHandler
	{
		protected override void Run()
		{
			ProjectLinkerManager manager = ProjectLinkerManager.Manager;
			new SettingsDialog (manager.OnSettingsSave, manager.SavedSourceProjectName, manager.SavedTargetProjectNames);
		}

		protected override void Update(CommandInfo info) {
			info.Enabled = IdeApp.ProjectOperations.CurrentSelectedProject != null;
		}
	}
}
