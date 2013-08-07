using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace ProjectLinker
{
	public class LinkProjectHandler : CommandHandler
	{
		enum FileChangedActionType { Added, Removed, Renamed }
		private Project targetProject;

		protected override void Run()
		{
			SettingsDialog dialog = new SettingsDialog ();
			//new Dialog ();
			//dialog.Show ();

			/*
			ReadOnlyCollection<Project> projects = IdeApp.Workspace.GetAllProjects();

			if (projects.Count < 2) {
				return;
			}

			Project sourceProject = IdeApp.ProjectOperations.CurrentSelectedProject;
			targetProject = (from p in projects where p != sourceProject select p).First();

			sourceProject.FileAddedToProject += (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Added);
			sourceProject.FileRemovedFromProject += (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Removed);
			sourceProject.FileRenamedInProject += (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Renamed);
			 * */
		}

		private void ExecuteFileChangedAction<T>(EventArgsChain<T> args, FileChangedActionType actionType) where T : ProjectFileEventInfo
		{
			if (args is ProjectFileEventArgs) {
				HandleFileAddedRemoved(args as ProjectFileEventArgs, actionType);
			}
			else if (args is ProjectFileRenamedEventArgs) {
				HandleFileRenamed(args as ProjectFileRenamedEventArgs);
			}

			targetProject.Save(null);
		}

		private void HandleFileRenamed(ProjectFileRenamedEventArgs args)
		{
			foreach (var info in args) {
				RemoveFile(new ProjectFile(info.OldName));
				AddFile(new ProjectFile(info.NewName), info.Project);
			}	
		}

		private void HandleFileAddedRemoved(ProjectFileEventArgs args, FileChangedActionType actionType)
		{
			foreach (var info in args) {
				if (actionType == FileChangedActionType.Added) {
					AddFile(info.ProjectFile, info.Project);
				}
				else if (actionType == FileChangedActionType.Removed) {
					RemoveFile(info.ProjectFile);
				}
			}
		}

		private void AddFile(ProjectFile projectFile, Project project)
		{
			if (FileService.IsDirectory(projectFile.Name)) {
				return;
			}

			string targetProjectBaseDir = targetProject.BaseDirectory.ToString();
			string filename = projectFile.FilePath.ToString();
			string linkPath = filename.Substring(project.BaseDirectory.ToString().Length + 1);

			string classDir = linkPath.Substring(0, linkPath.LastIndexOf(Path.DirectorySeparatorChar));
			FileService.EnsureDirectoryExists(Path.Combine(targetProjectBaseDir, classDir));

			ProjectFile pf = new ProjectFile(filename);
			pf.Link = linkPath;
			targetProject.AddFile(pf);
		}

		private void RemoveFile(ProjectFile target)
		{
			ProjectFile pf = targetProject.Files.FirstOrDefault(f => f.Name == target.Name);

			if (pf != null) {
				targetProject.Files.Remove(pf);
			}
		}

		protected override void Update(CommandInfo info) {
			//info.Enabled = IdeApp.ProjectOperations.CurrentSelectedProject != null;
		}
	}
}