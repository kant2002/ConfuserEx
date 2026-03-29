using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TruePath;
using Xunit;

namespace Confuser.MSBuild.Tasks.Tests {
	internal class DotNetCliHelper {

		public static async Task<IReadOnlyDictionary<string, string>> EvaluateMSBuildProperties(
			ITestOutputHelper output,
			string projectPath,
			IReadOnlyDictionary<string, string>? env = null,
			params string[] propertyNames) {
			if (!propertyNames.Any())
				return new Dictionary<string, string>();

			var result = await ExecUtil.Run(
				output,
				ExecUtil.DotNetHost,
				AbsolutePath.CurrentWorkingDirectory,
				["msbuild", $"\"{projectPath}\"", $"-getProperty:{string.Join(",", propertyNames)}"],
				null,
				additionalEnvironment: env);
			var resultString = result.StandardOutput;
			if (propertyNames.Length == 1)
				return new Dictionary<string, string> { { propertyNames[0], resultString } };

			var resultJson = JsonDocument.Parse(resultString);
			var propertiesJson = resultJson.RootElement.GetProperty("Properties").EnumerateObject().ToArray();

			return propertiesJson
				.ToDictionary(property => property.Name, property => property.Value.GetString() ?? string.Empty);
		}

		public static async Task<IEnumerable<(string identity, string? fullPath)>> EvaluateMSBuildItem(
			ITestOutputHelper output,
			string projectPath,
			string itemName,
			IReadOnlyDictionary<string, string>? env = null) {
			var result = await ExecUtil.Run(
				output,
				ExecUtil.DotNetHost,
				AbsolutePath.CurrentWorkingDirectory,
				["msbuild", $"\"{projectPath}\"", $"-getItem:{itemName}"],
				null,
				additionalEnvironment: env);
			var resultString = result.StandardOutput;
			var resultJson = JsonDocument.Parse(resultString);
			var itemsJson = resultJson.RootElement.GetProperty("Items").EnumerateObject().ToArray();
			var itemsDict = itemsJson.ToDictionary(item => item.Name, item => item.Value.EnumerateArray());

			return itemsDict[itemName].Select(meta => (meta.GetProperty("Identity").GetString()!, meta.GetProperty("FullPath").GetString()));
		}
	}
}
