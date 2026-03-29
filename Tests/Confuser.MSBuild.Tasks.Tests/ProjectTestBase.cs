// SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
//
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using TruePath;
using Xunit;
using Xunit.Sdk;

namespace Confuser.MSBuild.Tasks.Tests {
	// Adapted from SdkTestBase.cs
	public abstract class ProjectTestBase : IDisposable {
		private readonly ITestOutputHelper _testOutputHelper;
		private readonly string _temporaryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		private readonly Dictionary<string, string> _dotNetEnvVars;

		private string NuGetConfigPath => Path.Combine(_temporaryPath, "NuGet.config");
		private string GlobalJsonPath => Path.Combine(_temporaryPath, "global.json");

		public ProjectTestBase(ITestOutputHelper testOutputHelper) {
			_testOutputHelper = testOutputHelper;
			_dotNetEnvVars = new() { ["NUGET_PACKAGES"] = Path.Combine(_temporaryPath, "package-cache") };

			File.Delete(_temporaryPath);

			_testOutputHelper.WriteLine($"Test projects folder: {_temporaryPath}");

			var assemblyPath = Assembly.GetExecutingAssembly().Location;
			var testDataPath = Path.Combine(Path.GetDirectoryName(assemblyPath)!, "TestProjects");
			_testOutputHelper.WriteLine($"Copying TestProjects to {_temporaryPath}...");
			CopyDirectoryRecursive(testDataPath, _temporaryPath);

#if DEBUG
			var nupkgPath = (SolutionMetadata.SourceRoot / "artifacts/package/debug").Canonicalize();
#else
			var nupkgPath = (SolutionMetadata.SourceRoot / "artifacts/package/release").Canonicalize();
#endif
			_testOutputHelper.WriteLine($"Local NuGet feed: {nupkgPath}.");
			EmitNuGetConfig(NuGetConfigPath, nupkgPath);
			//EmitGlobalJson(GlobalJsonPath, $"{SolutionMetadata.VersionPrefix}");
		}
		public static void AssertIncludes<T>(IReadOnlyCollection<T> expected, IReadOnlyCollection<T> all) {
			var foundItems = all.Where(expected.Contains).ToList();
			var remainingItems = expected.Except(foundItems).ToList();
			if (remainingItems.Count != 0)
				throw new XunitException($"Expected elements are missing: [{string.Join(", ", remainingItems)}]");
		}



		private static void EmitNuGetConfig(string configFilePath, AbsolutePath packageSourcePath) {
			File.WriteAllText(configFilePath, $"""
            <configuration>
                <packageSources>
                    <add key="local" value="{packageSourcePath.Value}" />
               </packageSources>
            </configuration>
            """);
		}

		//private static void EmitGlobalJson(string globalJsonPath, string packageVersion) {
		//	var actualGlobalJson = SolutionMetadata.SourceRoot / "global.json";
		//	var globalConfig = JsonNode.Parse(File.ReadAllText(actualGlobalJson.Value))!;
		//	globalConfig["msbuild-sdks"] = new JsonObject([new KeyValuePair<string, JsonNode?>("Cesium.Sdk", packageVersion)]);
		//	var content = globalConfig.ToJsonString(new JsonSerializerOptions {
		//		WriteIndented = true
		//	});
		//	File.WriteAllText(globalJsonPath, content);
		//}

		protected async Task<BuildResult> ExecuteTargets(string projectName, params string[] targets) {
			var projectFile = $"{projectName}/{projectName}.csproj";
			var joinedTargets = string.Join(";", targets);
			var testProjectFile = Path.GetFullPath(Path.Combine(_temporaryPath, projectFile));
			var testProjectFolder = Path.GetDirectoryName(testProjectFile) ?? throw new ArgumentNullException(nameof(testProjectFile));
			var binLogFile = Path.Combine(testProjectFolder, $"build_result_{projectName}_{DateTime.UtcNow:yyyy-dd-M_HH-mm-s}.binlog");

			const string objFolderPropertyName = "IntermediateOutputPath";
			const string binFolderPropertyName = "OutDir";

			var startInfo = new ProcessStartInfo {
				WorkingDirectory = testProjectFolder,
				FileName = "dotnet",
				ArgumentList = { "msbuild", testProjectFile, $"/t:{joinedTargets}", "/restore", $"/bl:{binLogFile}" },
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				UseShellExecute = false,
			};
			foreach (var pair in _dotNetEnvVars) {
				var name = pair.Key;
				var var = pair.Value;
				startInfo.Environment[name] = var;
			}

			using var process = new Process();
			process.StartInfo = startInfo;
			var stdOutOutput = "";
			var stdErrOutput = "";
			process.OutputDataReceived += (_, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data)) {
					stdOutOutput += e.Data + Environment.NewLine;
					_testOutputHelper.WriteLine($"[stdout]: {e.Data}");
				}
			};

			process.ErrorDataReceived += (_, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data)) {
					stdErrOutput += e.Data + Environment.NewLine;
					_testOutputHelper.WriteLine($"[stderr]: {e.Data}");
				}
			};

			process.Start();

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			process.WaitForExit();

			var success = process.ExitCode == 0;

			_testOutputHelper.WriteLine(success
				? "Build succeeded"
				: $"Build failed with exit code {process.ExitCode}");

			var properties = await DotNetCliHelper.EvaluateMSBuildProperties(
				_testOutputHelper,
				testProjectFile,
				env: _dotNetEnvVars,
				objFolderPropertyName,
				binFolderPropertyName);
			_testOutputHelper.WriteLine($"Properties request result: {JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = false })}");

			var binFolder = NormalizePath(Path.GetFullPath(properties[binFolderPropertyName], testProjectFolder));
			var objFolder = NormalizePath(Path.GetFullPath(properties[objFolderPropertyName], testProjectFolder));

			var binArtifacts = CollectArtifacts(binFolder);
			var objArtifacts = CollectArtifacts(objFolder);

			var result = new BuildResult(process.ExitCode, stdOutOutput, stdErrOutput, binArtifacts, objArtifacts);
			_testOutputHelper.WriteLine($"Build result: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");
			return result;

			IReadOnlyCollection<BuildArtifact> CollectArtifacts(string folder) {
				_testOutputHelper.WriteLine($"Collecting artifacts from '{folder}' folder");
				return Directory.Exists(folder)
					? Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
						.Select(path => new BuildArtifact(Path.GetRelativePath(folder, path), path))
						.ToList()
					: Array.Empty<BuildArtifact>();
			}
		}

		private static void CopyDirectoryRecursive(string source, string target) {
			Directory.CreateDirectory(target);

			foreach (var subDirPath in Directory.GetDirectories(source)) {
				var dirName = Path.GetFileName(subDirPath);
				CopyDirectoryRecursive(subDirPath, Path.Combine(target, dirName));
			}

			foreach (var filePath in Directory.GetFiles(source)) {
				var fileName = Path.GetFileName(filePath);
				File.Copy(filePath, Path.Combine(target, fileName));
			}
		}

		private static string NormalizePath(string path) {
			var normalizedPath = new Uri(path).LocalPath;
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? normalizedPath
				: normalizedPath.Replace('\\', '/');
		}

		private void ClearOutput() {
			Directory.Delete(_temporaryPath, true);
		}

		public void Dispose() {
			ClearOutput();
		}

		protected class BuildResult(
			int exitCode,
			string stdOutOutput,
			string stdErrOutput,
			IReadOnlyCollection<BuildArtifact> outputArtifacts,
			IReadOnlyCollection<BuildArtifact> intermediateArtifacts) {
			public int ExitCode { get; private set; } = exitCode;
			public string StdOutOutput { get; private set; } = stdOutOutput;
			public string StdErrOutput { get; private set; } = stdErrOutput;
			public IReadOnlyCollection<BuildArtifact> OutputArtifacts { get; private set; } = outputArtifacts;
			public IReadOnlyCollection<BuildArtifact> IntermediateArtifacts { get; private set; } = intermediateArtifacts;
		}

		protected class BuildArtifact(
			string fileName,
			string fullPath) {
			public string FileName { get; private set; } = fileName;
			public string FullPath { get; private set; } = fullPath;
		}
	}
}
