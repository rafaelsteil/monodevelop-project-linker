using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace ProjectLinker
{
	public class StartupHandler : CommandHandler
	{
		protected override void Run()
		{
			IdeApp.Workspace.SolutionLoaded += (sender, args) => ProjectLinkerManager.Manager.SolutionLoaded();
			IdeApp.Workspace.SolutionUnloaded += (sender, args) => ProjectLinkerManager.Manager.SolutionUnloaded();
		}
	}
}
