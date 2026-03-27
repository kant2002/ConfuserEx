using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Confuser.MSBuild.Tasks.Tests {
	public class ConfuseTaskTests : VerifyTestBase {
		private Mock<IBuildEngine> buildEngine;
		private List<BuildErrorEventArgs> errors;

		public ConfuseTaskTests() {
			this.buildEngine = new Mock<IBuildEngine>();
			this.errors = new List<BuildErrorEventArgs>();
			this.buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e));
		}

		[Fact]
		[Trait("Category", "MSBuildIntegration")]
		public async Task NonExistingFileInProjectProduceError() {
			var assembly = new Mock<ITaskItem>();
			assembly.SetupAllProperties();
			assembly.Object.ItemSpec = $".\\bin\\debug\\test.dll";
			var resultProject = new Mock<ITaskItem>();
			resultProject.SetupAllProperties();
			resultProject.Object.ItemSpec = $"Resources\\non-existing-file.crproj";

			var task = new ConfuseTask();
			task.Project = resultProject.Object;
			task.BuildEngine = buildEngine.Object;
			task.OutputAssembly = assembly.Object;

			//Act
			var success = task.Execute();

			//Assert
			Assert.True(success);
			await Verify(string.Join("\n", errors.Select(_ => _.Message)), GetSettings());
		}

		[Fact]
		[Trait("Category", "MSBuildIntegration")]
		public async Task CorrectProjectProduceRunnableExecutable() {
			var assembly = new Mock<ITaskItem>();
			assembly.SetupAllProperties();
			assembly.Object.ItemSpec = $".\\test\\net472\\obfuscated\\Confuser.CLI.exe";
			var resultProject = new Mock<ITaskItem>();
			resultProject.SetupAllProperties();
			resultProject.Object.ItemSpec = $"Resources\\valid.crproj";

			var task = new ConfuseTask();
			task.Project = resultProject.Object;
			task.BuildEngine = buildEngine.Object;
			task.OutputAssembly = assembly.Object;

			//Act
			var success = task.Execute();

			//Assert
			Assert.True(success);
			Assert.Empty(errors);
			Assert.True(File.Exists(task.OutputAssembly.ItemSpec));
		}
	}
}
