using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gtk;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace ProjectLinker
{
	public partial class SettingsDialog : Dialog
	{
		private readonly Action<string, List<string>> onSettingsSave;

		public SettingsDialog (Action<string, List<string>> onSettingsSave)
		{
			this.onSettingsSave = onSettingsSave;
			Build ();
			FillSourceProjectCombo();
			FillTargetProjects();
		}

		private void FillTargetProjects()
		{
			ReadOnlyCollection<Project> projects = IdeApp.Workspace.GetAllProjects();
			
			foreach (var project in projects) {
				CheckButton checkButton = new CheckButton(project.Name);
				targetProjectsBox.PackStart(checkButton, false, false, 0);
			}

			targetProjectsBox.ShowAll();
		}

		private void FillSourceProjectCombo()
		{
			ReadOnlyCollection<Project> projects = IdeApp.Workspace.GetAllProjects();
			projectsCombo.AppendText("Do not link any projects");

			foreach (var project in projects) {
				projectsCombo.AppendText(project.Name);
			}

			projectsCombo.Active = 0;
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
