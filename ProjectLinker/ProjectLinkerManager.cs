using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace ProjectLinker
{
	public class ProjectLinkerManager
	{
		public static readonly ProjectLinkerManager Manager = new ProjectLinkerManager();

		private ProjectFileEventHandler fileAddedToProject;
		private ProjectFileEventHandler fileRemovedFromProject;
		private ProjectFileRenamedEventHandler fileRenamedInProject;
		private Project sourceProject;
		private List<Project> targetProjects;

		public void SolutionLoaded() {
			InitProjectLinkSettings(SavedSourceProjectName, SavedTargetProjectNames);
		}

		public void SolutionUnloaded() {
			ResetFileEvents();
			sourceProject = null;
		}

		public void OnSync() {
			if (sourceProject == null) {
				return;
			}

			SyncRemovedFiles();
			SyncAddedFiles();
			SaveTargetProjects();
		}

		public List<string> SavedTargetProjectNames {
			get { return PropertyService.Get<List<string>>(UserPreferencesTargetProjects); }
		}

		public string SavedSourceProjectName {
			get { return PropertyService.Get<string>(UserPreferencesSourceProject); }
		}

		public void OnSettingsSave(string sourceProjectName, List<string> targetProjectsNames) {
			if (sourceProjectName == null || targetProjectsNames.Count == 0) {
				ResetFileEvents();
				sourceProject = null;
				SaveUserPreferences();
				return;
			}

			InitProjectLinkSettings(sourceProjectName, targetProjectsNames);
			SaveUserPreferences();
		}

		private void ResetFileEvents() {
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

		private void InitializeFileEvents() {
			ResetFileEvents();

			if (sourceProject != null) {
				fileAddedToProject = (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Added);
				fileRemovedFromProject = (sender, args) => ExecuteFileChangedAction(args, FileChangedActionType.Removed);
				fileRenamedInProject = (sender, args) => ExecuteFileRenamedAction(args);

				sourceProject.FileAddedToProject += fileAddedToProject;
				sourceProject.FileRemovedFromProject += fileRemovedFromProject;
				sourceProject.FileRenamedInProject += fileRenamedInProject;
			}
		}

		private void InitProjectLinkSettings(string sourceProjectName, List<string> targetProjectsNames)
		{
			var allProjects = IdeApp.Workspace.GetAllProjects();
			sourceProject = allProjects.First(p => p.Name == sourceProjectName);
			InitializeFileEvents();
			targetProjects = (from p in allProjects where targetProjectsNames.Contains(p.Name) select p).ToList();
		}

		private string SolutioName
		{
			get { return IdeApp.Workspace.GetAllSolutions().First().Name; }
		}

		private void SaveUserPreferences() {
			PropertyService.Set(UserPreferencesSourceProject, sourceProject != null ? sourceProject.Name : "");
			PropertyService.Set(UserPreferencesTargetProjects, sourceProject != null ? (from t in targetProjects select t.Name).ToList() : null);
		}

		private string UserPreferencesTargetProjects {
			get { return String.Format("{0}.TargetProjects", UserPreferencesRoot); }
		}

		private string UserPreferencesSourceProject {
			get { return String.Format("{0}.SourceProject", UserPreferencesRoot); }
		}

		private string UserPreferencesRoot {
			get { return String.Format("MonoDevelop.Addins.ProjectLinker.{0}", SolutioName); }
		}

		private void ExecuteFileChangedAction(ProjectFileEventArgs args, FileChangedActionType actionType) {
			if (sourceProject == null) {
				return;
			}

			if (actionType == FileChangedActionType.Added || actionType == FileChangedActionType.Removed) {
				HandleFileAddedRemoved(args, actionType);
			}

			SaveTargetProjects();
		}

		private void ExecuteFileRenamedAction(ProjectFileRenamedEventArgs args) {
			if (sourceProject == null) {
				return;
			}

			HandleFileRenamed(args);

			SaveTargetProjects();
		}

		private void SaveTargetProjects()
		{
			targetProjects.ForEach(p => p.Save(null));
		}

		private void HandleFileRenamed(ProjectFileRenamedEventArgs args) {
			foreach (var info in args) {
				RemoveFile(new ProjectFile(info.OldName));
				AddFile(new ProjectFile(info.NewName), info.Project);
			}
		}

		private void HandleFileAddedRemoved(ProjectFileEventArgs args, FileChangedActionType actionType) {
			foreach (var info in args) {
				if (actionType == FileChangedActionType.Added) {
					AddFile(info.ProjectFile, info.Project);
				}
				else if (actionType == FileChangedActionType.Removed) {
					RemoveFile(info.ProjectFile);
				}
			}
		}

		private void AddFile(ProjectFile projectFile, Project fromSourceProject) {
			if (FileService.IsDirectory(projectFile.Name)) {
				return;
			}

			foreach (var targetProject in targetProjects) {
				string targetProjectBaseDir = targetProject.BaseDirectory.ToString();
				string filename = projectFile.FilePath.ToString();
				string linkPath = projectFile.ProjectVirtualPath;

				if (IgnoreExistingFile(targetProject, linkPath)) {
					continue;
				}

				int lastIndexOf = linkPath.LastIndexOf(Path.DirectorySeparatorChar);

				if (lastIndexOf > -1) {
					string classDir = linkPath.Substring(0, lastIndexOf);
					FileService.EnsureDirectoryExists(Path.Combine(targetProjectBaseDir, classDir));
				}

				ProjectFile pf = new ProjectFile(filename);

				if (targetProject.BaseDirectory != fromSourceProject.BaseDirectory) {
					pf.Link = linkPath;
					pf.BuildAction = projectFile.BuildAction;
				}

				targetProject.AddFile(pf);
			}
		}

		private bool IgnoreExistingFile(Project targetProject, string linkPath)
		{
			return targetProject.Files.GetFileWithVirtualPath(linkPath) != null;
		}

		private void RemoveFile(ProjectFile target) {
			foreach (var targetProject in targetProjects) {
				ProjectFile pf = targetProject.Files.FirstOrDefault(f => f.Name == target.Name);

				if (pf != null) {
					targetProject.Files.Remove(pf);
				}
			}
		}

		private void SyncAddedFiles() {
			foreach (var pf in sourceProject.Files) {
				AddFile(pf, sourceProject);
			}
		}

		private void SyncRemovedFiles() {
			foreach (var project in targetProjects) {
				var inexistedFiles = (from pf in project.Files where !File.Exists(pf.FilePath) select pf).ToList();

				foreach (var pf in inexistedFiles) {
					project.Files.Remove(pf);
				}
			}
		}

		private ProjectLinkerManager() {}
	}
}
