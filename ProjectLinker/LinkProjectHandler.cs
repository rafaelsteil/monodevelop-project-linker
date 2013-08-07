using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
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
			string sourceProjectName;
			List<string> targetProjectNames;
			LoadUserPreferences(out sourceProjectName, out targetProjectNames);
			new SettingsDialog (OnSettingsSave, sourceProjectName, targetProjectNames);
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
			var settingsPath = UserPreferencesPath;
			XDocument doc = File.Exists(UserPreferencesPath) ? XDocument.Load(settingsPath) : XDocument.Parse("<Properties></Properties>");
			
			doc.Root.Descendants(UserPreferencesTagName).Remove();

			if (sourceProject != null) {
				XElement prefsElement = new XElement(UserPreferencesTagName,
					new XElement("sourceProject", sourceProject.Name),
					new XElement("targetProjects", (from t in targetProjects select new XElement("name", t.Name)).ToList()));
				doc.Root.Add(prefsElement);
			}

			File.WriteAllText(settingsPath, doc.ToString(), Encoding.UTF8);
		}

		private static string UserPreferencesTagName
		{
			get { return "MonoDevelop.Addins.ProjectLinker"; }
		}

		private void LoadUserPreferences(out string sourceProjectName, out List<string> targetProjectNames)
		{
			var settingsPath = UserPreferencesPath;
			sourceProjectName = null;
			targetProjectNames = new List<string>();

			if (!File.Exists(settingsPath)) {
				return;
			}

			XDocument doc = XDocument.Load(settingsPath);
			sourceProjectName = doc.XPathSelectElements(String.Format("//{0}/sourceProject", UserPreferencesTagName)).Select(x => x.Value).FirstOrDefault();
			targetProjectNames = doc.XPathSelectElements(String.Format("//{0}/targetProjects/name", UserPreferencesTagName)).Select(x => x.Value).ToList();
		}

		private static string UserPreferencesPath
		{
			get
			{
				Solution solution = IdeApp.Workspace.GetAllSolutions().First();
				return Path.Combine(solution.BaseDirectory, solution.Name + ".userprefs");
			}
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