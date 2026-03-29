// SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
//
// SPDX-License-Identifier: MIT
using System;

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
