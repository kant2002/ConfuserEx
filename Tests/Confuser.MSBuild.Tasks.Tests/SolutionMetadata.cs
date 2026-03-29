// SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
//
// SPDX-License-Identifier: MIT
using System;
using System.Reflection;
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
