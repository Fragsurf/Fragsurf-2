using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mediatonic.Tools
{
	public class PackageExtractor : EditorWindow
	{
		private const string _defaultOutputPath = "./";
		private const string _defaultPackagePath = "./";

		private string _outputPath = "./";
		private string _packagePath = "./";

		[MenuItem("Window/Extract Package")]
		private static void ExtractHardcodedPackage()
		{
			GetWindow<PackageExtractor>().Show();
		}

		private void Awake()
		{
			// Get canonical path to avoid showing relative paths in window
			_outputPath = Path.GetFullPath(_defaultOutputPath);
			_packagePath = Path.GetFullPath(_defaultPackagePath);
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = false;
			EditorGUILayout.TextField("Unity package", _packagePath);
			GUI.enabled = true;
			if (GUILayout.Button("..."))
			{
				_packagePath = EditorUtility.OpenFilePanel("Find Unity Package", "./", "unitypackage");
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUI.enabled = false;
			EditorGUILayout.TextField("Destination", _outputPath);
			GUI.enabled = true;
			if (GUILayout.Button("..."))
			{
				_outputPath = EditorUtility.OpenFolderPanel("Output location", _outputPath, "");
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Extract"))
			{
				try
				{
					ExtractPackage(_packagePath, _outputPath);
				}
				finally
				{
					// If any I/O etc fails ensure the progress bar is still cleaned up
					EditorUtility.ClearProgressBar();
				}
			}
		}

		public static void ExtractPackage(string packagePath, string outPath = null)
		{
			outPath = GetFullOutPath(packagePath, outPath);

			string workingDir = ExtractToWorkingDirectory(packagePath, outPath);

			FixFolderStructure(outPath, workingDir);

			CleanUp(workingDir);
		}

		private static string GetFullOutPath(string packagePath, string outPath)
		{
			string name = Path.GetFileNameWithoutExtension(packagePath);

			if (string.IsNullOrEmpty(outPath))
			{
				outPath = Application.dataPath;
			}

			outPath = Path.Combine(outPath, name);
			if (Directory.Exists(outPath))
			{
				throw new Exception($"Output path {outPath} already exists");
			}

			return outPath;
		}

		// Extract the archive using the archive's folder structure (one directory per-asset with meta data indicating where the asset should finally live)
		private static string ExtractToWorkingDirectory(string packagePath, string outPath)
		{
			string workingDir = Path.Combine(outPath, ".working");

			if (Directory.Exists(workingDir))
			{
				Directory.Delete(workingDir, true);
			}
			Directory.CreateDirectory(workingDir);

			EditorUtility.DisplayProgressBar("Extracting", "Extracting package", 0.0f);
			var inStream = File.OpenRead(packagePath);
			var gzipStream = new GZipInputStream(inStream);
			//var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
			var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, System.Text.Encoding.ASCII);
			tarArchive.ExtractContents(workingDir);
			tarArchive.Close();
			gzipStream.Close();
			inStream.Close();
			EditorUtility.ClearProgressBar();
			return workingDir;
		}

		// Iterate over the individual assets' directories and move them to their target location from the "pathname" file
		private static void FixFolderStructure(string outPath, string workingDir)
		{
			var dirs = Directory.GetDirectories(workingDir);
			for (int i = 0; i < dirs.Length; ++i)
			{
				string dir = dirs[i];
				string assetPath = Path.Combine(dir, "asset");
				EditorUtility.DisplayProgressBar("Extracting", $"Moving {assetPath} to output folder", (float)i / dirs.Length);

				string pathnamePath = Path.Combine(dir, "pathname");
				string metaPath = Path.Combine(dir, "asset.meta");

				// something wrong?
                if (!File.Exists(pathnamePath))
                {
                    continue;
                }

				using var pathnameFile = new StreamReader(pathnamePath);
				var pathnameContents = pathnameFile.ReadLine();

                if (File.Exists(metaPath))
                {
					var metaTargetPath = Path.Combine(outPath, pathnameContents + ".meta");
					var metaTargetPathDir = Path.GetDirectoryName(metaTargetPath);
					if (!Directory.Exists(metaTargetPathDir))
					{
						Directory.CreateDirectory(metaTargetPathDir);
					}
					File.Move(metaPath, metaTargetPath);
                }

                if (File.Exists(assetPath))
                {
					var assetTargetPath = Path.Combine(outPath, pathnameContents);
					var assetTargetPathDir = Path.GetDirectoryName(assetTargetPath);
                    if (!Directory.Exists(assetTargetPathDir))
                    {
						Directory.CreateDirectory(assetTargetPathDir);
					}
					File.Move(assetPath, assetTargetPath);
				}				
			}

			EditorUtility.ClearProgressBar();
		}

		// Delete the working directory when we're done
		private static void CleanUp(string workingDir)
		{
			Directory.Delete(workingDir, true);
		}
	}
}