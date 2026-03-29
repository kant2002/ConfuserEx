using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Medallion.Shell;
using TruePath;
using Xunit;

namespace Confuser.MSBuild.Tasks.Tests {
	internal class ExecUtil {
		public static readonly LocalPath DotNetHost = new("dotnet");

		public static async Task<CommandResult> Run(
			ITestOutputHelper? output,
			LocalPath executable,
			AbsolutePath workingDirectory,
			string[] args,
			string? inputContent = null,
			IReadOnlyDictionary<string, string>? additionalEnvironment = null) {
			output?.WriteLine($"$ {executable} {string.Join(" ", args)}");
			var command = Command.Run(executable.Value, args, o =>
			{
				o.WorkingDirectory(workingDirectory.Value);
				if (inputContent is { }) {
					o.StartInfo(_ => _.RedirectStandardInput = true);
				}

				if (additionalEnvironment != null) {
					foreach (var (key, value) in additionalEnvironment) {
						o.EnvironmentVariable(key, value);
					}
				}
			});
			if (inputContent is { }) {
				command.StandardInput.Write(inputContent);
				command.StandardInput.Close();
			}

			var result = await command.Task;
			foreach (var s in result.StandardOutput.Split("\n"))
				output?.WriteLine(s.TrimEnd());
			if (result.StandardError.Trim() != "") {
				foreach (var s in result.StandardError.Split("\n"))
					output?.WriteLine($"[ERR] {s.TrimEnd()}");
			}

			output?.WriteLine($"Command exit code: {result.ExitCode}");
			return result;
		}
	}
}
