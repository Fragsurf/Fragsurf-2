using ModTool.Editor;
using SuperUnityBuild.BuildTool;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExporterCreatorBuildAction : BuildAction, IPostBuildPerPlatformAction
{

    public override void PerBuildExecute(BuildReleaseType releaseType, BuildPlatform platform, BuildArchitecture architecture, BuildDistribution distribution, System.DateTime buildTime, ref BuildOptions options, string configKey, string buildPath)
    {
        base.PerBuildExecute(releaseType, platform, architecture, distribution, buildTime, ref options, configKey, buildPath);

        ExporterCreator.CreateExporterPostBuild(BuildTarget.NoTarget, buildPath);
    }

}
