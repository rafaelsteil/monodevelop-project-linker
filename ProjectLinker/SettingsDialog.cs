using System;
using System.Collections.ObjectModel;
using Gtk;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace ProjectLinker
{
	public partial class SettingsDialog : Gtk.Dialog
	{
		public SettingsDialog ()
		{
			Build ();

			ReadOnlyCollection<Project> projects = IdeApp.Workspace.GetAllProjects();

			foreach (var project in projects) {
				projectsCombo.AppendText(project.Name);
			}
		}

		protected void cancelButtonClicked (object sender, EventArgs e)
		{
			Destroy();
		}

		protected void saveButtonClicked (object sender, EventArgs e)
		{
			Destroy();
		}
	}
}
