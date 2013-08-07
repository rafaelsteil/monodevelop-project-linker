using System;
using System.Linq;
using System.Collections.Generic;
using Gtk;
using MonoDevelop.Ide;

namespace ProjectLinker
{
	public partial class SettingsDialog : Dialog
	{
		private readonly Action<string, List<string>> onSettingsSave;
		private readonly string savedSourceProjectName;
		private readonly List<string> savedTargetProjectNames;

		public SettingsDialog (Action<string, List<string>> onSettingsSave, string savedSourceProjectName, List<string> savedTargetProjectNames)
		{
			this.onSettingsSave = onSettingsSave;
			this.savedSourceProjectName = savedSourceProjectName;
			this.savedTargetProjectNames = savedTargetProjectNames;
			Build ();
			FillTargetProjects();
			FillSourceProjectCombo();
		}

		private void FillTargetProjects()
		{
			var projects = (from p in IdeApp.Workspace.GetAllProjects() select p.Name).ToList();
			
			foreach (var project in projects) {
				CheckButton checkButton = new CheckButton(project);
				targetProjectsBox.PackStart(checkButton, false, false, 0);

				if (savedTargetProjectNames != null && savedTargetProjectNames.Contains(project)) {
					checkButton.Active = true;
				}
			}

			targetProjectsBox.ShowAll();
		}

		private void FillSourceProjectCombo()
		{
			var projects = (from p in IdeApp.Workspace.GetAllProjects() select p.Name).ToList();
			projectsCombo.AppendText("Do not link any projects");

			foreach (var project in projects) {
				projectsCombo.AppendText(project);
			}

			if (savedSourceProjectName == null) {
				projectsCombo.Active = 0;
			}
			else {
				int index = projects.IndexOf(savedSourceProjectName) + 1;
				TreeIter iter;
				projectsCombo.Model.IterNthChild(out iter, index);
				projectsCombo.SetActiveIter(iter);
			}
		}

		protected void cancelButtonClicked (object sender, EventArgs e)
		{
			Destroy();
		}

		protected void saveButtonClicked (object sender, EventArgs e)
		{
			string sourceProject = projectsCombo.Active == 0 ? null : projectsCombo.ActiveText;
			List<string> targetProjects = (from widget in targetProjectsBox.Children
								  let check = (widget as CheckButton)
								  where check != null && check.Active
								  select check.Label).ToList();

			onSettingsSave(sourceProject, targetProjects);
			Destroy();
		}

		protected void sourceProjectChanged (object sender, EventArgs e)
		{
			targetProjectsBox.Sensitive = projectsCombo.Active > 0;

			foreach (var widget in targetProjectsBox.Children) {
				var check = (widget as CheckButton);

				if (check == null) {
					continue;
				}

				check.Sensitive = check.Label != projectsCombo.ActiveText;

				if (!check.Sensitive || !targetProjectsBox.Sensitive) {
					check.Active = false;
				}
			}
		}
	}
}
