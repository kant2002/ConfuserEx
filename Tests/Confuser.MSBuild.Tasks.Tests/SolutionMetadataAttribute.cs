using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confuser.MSBuild.Tasks.Tests {
	internal class SolutionMetadataAttribute : Attribute {
		public string SourceRoot { get; }
		public string ArtifactsRoot { get; }
		public string VersionPrefix { get; }

		public SolutionMetadataAttribute(string sourceRoot, string artifactsRoot, string versionPrefix) {
			SourceRoot = sourceRoot;
			ArtifactsRoot = artifactsRoot;
			VersionPrefix = versionPrefix;
		}
	}
}
