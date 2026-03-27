using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Moq;
using Xunit;

namespace Confuser.MSBuild.Tasks.Tests {
	public class CreateProjectTests : VerifyTestBase {
		private readonly ITestOutputHelper outputHelper;
		private Mock<IBuildEngine> buildEngine;
		private List<BuildErrorEventArgs> errors;

		public CreateProjectTests(ITestOutputHelper outputHelper) {
			this.outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
			this.buildEngine = new Mock<IBuildEngine>();
			this.errors = new List<BuildErrorEventArgs>();
			this.buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback<BuildErrorEventArgs>(e => errors.Add(e));
		}

		[Fact]
		[Trait("Category", "MSBuildIntegration")]
		public async System.Threading.Tasks.Task CreateNewProjectIfNoSourceProject() {
			var assembly = new Mock<ITaskItem>();
			assembly.SetupAllProperties();
			assembly.Object.ItemSpec = $".\\bin\\debug\\test.dll";
			var baseDirectory = new Mock<ITaskItem>();
			baseDirectory.SetupAllProperties();
			baseDirectory.Object.ItemSpec = $"1.";
			var resultProject = new Mock<ITaskItem>();
			resultProject.SetupAllProperties();
			resultProject.Object.ItemSpec = $"result-empty.crproj";

			var task = new CreateProjectTask();
			task.AssemblyPath = assembly.Object;
			task.BuildEngine = buildEngine.Object;
			task.ResultProject = resultProject.Object;
			task.References = Array.Empty<ITaskItem>();

			//Act
			var success = task.Execute();

			//Assert
			Assert.True(success);
			Assert.Empty(errors);
			Assert.True(File.Exists(task.ResultProject.ItemSpec));
			await Verify(File.ReadAllText(task.ResultProject.ItemSpec), GetSettings());
		}

		[Fact]
		[Trait("Category", "MSBuildIntegration")]
		public async System.Threading.Tasks.Task BaseDirectoryOverridenTest() {
			var sourceProject = new Mock<ITaskItem>();
			sourceProject.SetupAllProperties();
			sourceProject.Object.ItemSpec = $".\\Resources\\confuser.src.crproj";
			var assembly = new Mock<ITaskItem>();
			assembly.SetupAllProperties();
			assembly.Object.ItemSpec = $".\\bin\\debug\\test.dll";
			var baseDirectory = new Mock<ITaskItem>();
			baseDirectory.SetupAllProperties();
			baseDirectory.Object.ItemSpec = $".";
			var resultProject = new Mock<ITaskItem>();
			resultProject.SetupAllProperties();
			resultProject.Object.ItemSpec = $"result.crproj";

			var task = new CreateProjectTask();
			task.AssemblyPath = assembly.Object;
			task.SourceProject = sourceProject.Object;
			task.BuildEngine = buildEngine.Object;
			task.ResultProject = resultProject.Object;
			task.References = Array.Empty<ITaskItem>();

			//Act
			var success = task.Execute();

			//Assert
			Assert.True(success);
			Assert.Empty(errors);
			Assert.True(File.Exists(task.ResultProject.ItemSpec));
			await Verify(File.ReadAllText(task.ResultProject.ItemSpec), GetSettings());
		}
	}
}
