using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using GLib;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace ProjectLinker
{
	public class LinkProjectHandler : CommandHandler
	{
		enum FileChangedActionType { Added, Removed, Renamed }
		private ProjectFileEventHandler fileAddedToProject;
		private ProjectFileEventHandler fileRemovedFromProject;
		private ProjectFileRenamedEventHandler fileRenamedInProject;
		private Project sourceProject;
		private List<Project> targetProjects;

		protected override void Run()
		{
			new SettingsDialog (OnSettingsSave);
		}

		private void ResetFileEvents()
		{
			if (sourceProject == null) {
				return;
			}

			if (fileAddedToProject != null) {
				sourceProject.FileAddedToProject -= fileAddedToProject;
			}

			if (fileRemovedFromProject != null) {
				sourceProject.FileRemovedFromProject -= fileRemovedFromProject;
			}

			if (fileRenamedInProject != null) {
				sourceProject.FileRenamedInProject -= fileRenamedInProject;
			}
		}

		private void InitializeFileEvents()
		{
			ResetFileEvents();

			fileAddedToProject = (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Added);
			fileRemovedFromProject = (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Removed);
			fileRenamedInProject = (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Renamed);

			sourceProject.FileAddedToProject += fileAddedToProject;
			sourceProject.FileRemovedFromProject += fileRemovedFromProject;
			sourceProject.FileRenamedInProject += fileRenamedInProject;
		}

		private void OnSettingsSave(string sourceProjectName, List<string> targetProjectsNames)
		{
			if (sourceProjectName == null || targetProjectsNames.Count == 0) {
				ResetFileEvents();
				sourceProject = null;
				SaveUserPreferences();
				return;
			}

			var allProjects = IdeApp.Workspace.GetAllProjects();
			sourceProject = allProjects.First(p => p.Name == sourceProjectName);
			InitializeFileEvents();
			targetProjects = (from p in allProjects where targetProjectsNames.Contains(p.Name) select p).ToList();
			SaveUserPreferences();
		}

		private void SaveUserPreferences()
		{
			Solution solution = IdeApp.Workspace.GetAllSolutions().First();
			string settingsPath = Path.Combine(solution.BaseDirectory, solution.Name + ".userprefs");
			XDocument doc = File.Exists(settingsPath) ? XDocument.Load(settingsPath) : XDocument.Parse("<Properties></Properties>");
			
			string tagName = "MonoDevelop.Addins.ProjectLinker";
			doc.Root.Descendants(tagName).Remove();

			if (sourceProject != null) {
				XElement prefsElement = new XElement(tagName,
					new XElement("sourceProject", sourceProject.Name),
					new XElement("targetProjects", (from t in targetProjects select new XElement("name", t.Name)).ToList()));
				doc.Root.Add(prefsElement);
			}

			File.WriteAllText(settingsPath, doc.ToString(), Encoding.UTF8);
		}

		private void ExecuteFileChangedAction<T>(EventArgsChain<T> args, FileChangedActionType actionType) where T : ProjectFileEventInfo
		{
			if (args is ProjectFileEventArgs) {
				HandleFileAddedRemoved(args as ProjectFileEventArgs, actionType);
			}
			else if (args is ProjectFileRenamedEventArgs) {
				HandleFileRenamed(args as ProjectFileRenamedEventArgs);
			}

			targetProjects.ForEach(p => p.Save(null));
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

			foreach (var targetProject in targetProjects) {
				string targetProjectBaseDir = targetProject.BaseDirectory.ToString();
				string filename = projectFile.FilePath.ToString();
				string linkPath = filename.Substring(project.BaseDirectory.ToString().Length + 1);

				string classDir = linkPath.Substring(0, linkPath.LastIndexOf(Path.DirectorySeparatorChar));
				FileService.EnsureDirectoryExists(Path.Combine(targetProjectBaseDir, classDir));

				ProjectFile pf = new ProjectFile(filename);
				pf.Link = linkPath;
				targetProject.AddFile(pf);
			}
		}

		private void RemoveFile(ProjectFile target)
		{
			foreach (var targetProject in targetProjects) {
				ProjectFile pf = targetProject.Files.FirstOrDefault(f => f.Name == target.Name);

				if (pf != null) {
					targetProject.Files.Remove(pf);
				}
			}
		}

		protected override void Update(CommandInfo info) {
			info.Enabled = IdeApp.ProjectOperations.CurrentSelectedProject != null;
		}
	}
}