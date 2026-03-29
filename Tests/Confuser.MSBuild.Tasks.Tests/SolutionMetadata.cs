using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TruePath;

namespace Confuser.MSBuild.Tasks.Tests {
	internal class SolutionMetadata {
		public static AbsolutePath SourceRoot => new(ResolvedAttribute.SourceRoot);
		public static AbsolutePath ArtifactsRoot => new(ResolvedAttribute.ArtifactsRoot);
		public static string VersionPrefix => ResolvedAttribute.VersionPrefix;

		private static SolutionMetadataAttribute ResolvedAttribute =>
			typeof(SolutionMetadata).Assembly.GetCustomAttribute<SolutionMetadataAttribute>()
			?? throw new Exception($"Missing {nameof(SolutionMetadataAttribute)} metadata attribute.");

	}
}
