using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Confuser.Core;
using Confuser.Core.Project;
using Xunit;
using Xunit.Sdk;

namespace Confuser.UnitTest {
	public abstract class TestBase {
		private const string _externalPrefix = "external:";

		readonly ITestOutputHelper outputHelper;

		protected static IEnumerable<SettingItem<Protection>> NoProtections => Enumerable.Empty<SettingItem<Protection>>();

		protected TestBase(ITestOutputHelper outputHelper) =>
			this.outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));

		protected Task Run(string inputFileName, string[] expectedOutput, SettingItem<Protection> protection,
			string outputDirSuffix = "", Action<string> outputAction = null, SettingItem<Packer> packer = null,
			Action<ProjectModule> projectModuleAction = null, Func<string, Task> postProcessAction = null,
			string seed = null, bool checkOutput = true) =>

			Run(new[] { inputFileName }, expectedOutput, protection, outputDirSuffix, outputAction, packer,
				projectModuleAction, postProcessAction, seed, checkOutput);

		protected Task Run(string inputFileName, string[] expectedOutput, IEnumerable<SettingItem<Protection>> protections,
			string outputDirSuffix = "", Action<string> outputAction = null, SettingItem<Packer> packer = null,
			Action<ProjectModule> projectModuleAction = null, Func<string, Task> postProcessAction = null) =>

			Run(new[] { inputFileName }, expectedOutput, protections, outputDirSuffix, outputAction, packer,
				projectModuleAction, postProcessAction);

		protected Task Run(string[] inputFileNames, string[] expectedOutput, SettingItem<Protection> protection,
			string outputDirSuffix = "", Action<string> outputAction = null, SettingItem<Packer> packer = null,
			Action<ProjectModule> projectModuleAction = null, Func<string, Task> postProcessAction = null,
			string seed = null, bool checkOutput = true) {
			var protections = (protection is null) ? Enumerable.Empty<SettingItem<Protection>>() : new[] { protection };
			return Run(inputFileNames, expectedOutput, protections, outputDirSuffix, outputAction, packer, projectModuleAction, postProcessAction, seed, checkOutput);
		}

		protected async Task Run(string[] inputFileNames, string[] expectedOutput, IEnumerable<SettingItem<Protection>> protections,
			string outputDirSuffix = "", Action<string> outputAction = null, SettingItem<Packer> packer = null,
			Action<ProjectModule> projectModuleAction = null, Func<string, Task> postProcessAction = null,
			string seed = null, bool checkOutput = true) {

			var baseDir = Environment.CurrentDirectory;
			var outputDir = Path.Combine(baseDir, "obfuscated" + outputDirSuffix);
			if (Directory.Exists(outputDir)) {
				Directory.Delete(outputDir, true);
			}

			string firstFileName = GetFileName(inputFileNames[0]);
			string entryInputFileName = Path.Combine(baseDir, firstFileName);
			var entryOutputFileName = Path.Combine(outputDir, firstFileName);
			var proj = new ConfuserProject {
				BaseDirectory = baseDir,
				OutputDirectory = outputDir,
				Packer = packer,
				Seed = seed
			};

			foreach (string name in inputFileNames) {
				var projectModule = new ProjectModule {
					Path = Path.Combine(baseDir, GetFileName(name)),
					IsExternal = IsExternal(name)
				};
				projectModuleAction?.Invoke(projectModule);
				proj.Add(projectModule);
			}

			var rule = new Rule();
			rule.AddRange(protections);
			if (rule.Count > 0)
				proj.Rules.Add(rule);

			var parameters = new ConfuserParameters {
				Project = proj,
				Logger = new XunitLogger(outputHelper, outputAction)
			};

			await ConfuserEngine.Run(parameters);

			for (var index = 0; index < inputFileNames.Length; index++) {
				string name = GetFileName(inputFileNames[index]);
				string outputName = Path.Combine(outputDir, name);

				bool exists;
				if (index == 0) {
					Assert.True(File.Exists(outputName));
					exists = true;
				}
				else {
					exists = File.Exists(outputName);
				}

				if (exists) {
					// Check if output assemblies is obfuscated
					Assert.NotEqual(FileUtilities.ComputeFileChecksum(Path.Combine(baseDir, name)),
						FileUtilities.ComputeFileChecksum(outputName));
				}
				else if (IsExternal(inputFileNames[index])) {
					File.Copy(
						Path.Combine(baseDir, GetFileName(inputFileNames[index])),
						Path.Combine(outputDir, GetFileName(inputFileNames[index])));
				}
			}

			if (Path.GetExtension(entryInputFileName) == ".exe") {
				var info = new ProcessStartInfo(entryOutputFileName) {
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false
				};
				using (var process = Process.Start(info)) {
					using (var stdout = process.StandardOutput) {
						try {
							if (checkOutput) {
								Assert.Equal("START", await stdout.ReadLineAsync());

								foreach (string line in expectedOutput) {
									Assert.Equal(line, await stdout.ReadLineAsync());
								}

								Assert.Equal("END", await stdout.ReadLineAsync());
								Assert.Empty(await stdout.ReadToEndAsync());
							}
						}
						catch (XunitException) {
							try {
								LogRemainingStream("Remaining standard output:", stdout);
								using (var stderr = process.StandardError) {
									LogRemainingStream("Remaining standard error:", stderr);
								}
							}
							catch {
								// ignore
							}
							throw;
						}
					}

					using (var stderr = process.StandardError) {
						Assert.Empty(await stderr.ReadToEndAsync());
					}

					Assert.True(process.HasExited);
					Assert.Equal(42, process.ExitCode);
				}
			}

			if (!(postProcessAction is null))
				await postProcessAction.Invoke(outputDir);
		}

		private void LogRemainingStream(string header, StreamReader reader) {
			var remainingOutput = reader.ReadToEnd();
			if (!string.IsNullOrWhiteSpace(remainingOutput)) {
				outputHelper.WriteLine(header);
				outputHelper.WriteLine(remainingOutput.Trim());
			}
		}

		private static string GetFileName(string name) {
			if (IsExternal(name))
				return name.Substring(_externalPrefix.Length);
			return name;
		}

		private static bool IsExternal(string name) => name.StartsWith(_externalPrefix, StringComparison.OrdinalIgnoreCase);
	}
}
