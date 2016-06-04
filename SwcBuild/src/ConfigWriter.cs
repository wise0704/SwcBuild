﻿using System;
using System.IO;
using System.Xml;

namespace SwcBuild
{
	using static DateTime;
	using static Platform;
	using static Program;
	using static Utilities;
	using static XmlWriter;

    internal class ConfigWriter : IDisposable
	{
	    private string directory;
	    private BuildOptions options;
	    private XmlWriter output;

		public ConfigWriter(string projectPath, string filename)
		{
			directory = projectPath;
			output = Create(Path.Combine(projectPath, filename), new XmlWriterSettings { Indent = true });
		}

		public void WriteConfig(BuildOptions buildOptions, Version sdkVersion, string targetPlayer, string[] additionalClasspaths, bool asc2, bool debug)
		{
			bool ver4 = sdkVersion >= new Version(4, 0);
			options = buildOptions;

			WriteStartDocument();
			{
				WriteComment($"This compiler configuration file was generated by {Assembly.Description}.");
				WriteComment("Any modifications you make may be lost.");
				WriteStartElement("flex-config");
				{
					AddTargetPlayer(targetPlayer);
					AddBaseOptions(asc2);
					WriteStartElement("compiler");
					{
						AddCompilerConstants(debug);
						AddCompilerOptions(asc2, debug, ver4);
						AddClasspaths(additionalClasspaths);
						AddIncludeLibraries();
						AddExternalLibraryPaths();
						AddLibraryPaths();
					}
					WriteEndElement();
					AddRsls();
					AddMovieOptions();
				}
				WriteEndElement();
			}
			WriteEndDocument();

		}

        private void AddTargetPlayer(string targetPlayer) => WriteElementString("target-player", targetPlayer);

        private void AddBaseOptions(bool asc2)
		{
			//if (asc2)
			//{
			//	if (options.AdvancedTelemetry)
			//		WriteElementString("advanced-telemetry", Value.True);
			//	if (!string.IsNullOrEmpty(options.AdvancedTelemetryPassword))
			//		WriteElementString("advanced-telemetry-password", options.AdvancedTelemetryPassword);
			//}
			if (!asc2)
			{
				WriteElementString("benchmark", options.Benchmark);
				WriteElementString("static-link-runtime-shared-libraries", options.StaticLinkRSL);
			}
			if (!options.UseNetwork)
				WriteElementString("use-network", false);
			if (!options.Warnings)
				WriteElementString("warnings", false);
		}

        private void AddCompilerConstants(bool debug)
		{
			const string configNs = "CONFIG::";

			bool desktop = options.Platform == AirDesktop;
			bool mobile = options.Platform == AirMobile;

			WriteDefine(configNs + "debug", debug);
			WriteDefine(configNs + "release", !debug);
			WriteDefine(configNs + "timeStamp", $"\"{Now.ToString("d")}\"");
			WriteDefine(configNs + "air", desktop || mobile);
			WriteDefine(configNs + "desktop", desktop);
			WriteDefine(configNs + "mobile", mobile);

			if (string.IsNullOrEmpty(options.CompilerConstants)) return;

			string[] constants = ParseConstants(options.CompilerConstants);

			foreach (string constant in constants)
			{
				int index = constant.IndexOf(',');
				WriteDefine(constant.Substring(0, index), constant.Substring(index + 1));
			}
		}

        private void AddCompilerOptions(bool asc2, bool debug, bool ver4)
		{
			if (options.Accessible)
				WriteElementString("accessible", true);
			if (options.AllowSourcePathOverlap)
				WriteElementString("allow-source-path-overlap", true);
			if (options.Es)
			{
				WriteElementString("as3", false);
				WriteElementString("es", true);
			}
			//if (debug)
			//	WriteElementString("debug", Value.True);
			//if (asc2 && options.Inline)
			//	WriteElementString("inline", Value.True);
			if (!string.IsNullOrEmpty(options.Locale))
			{
				WriteStartElement("locale");
				WriteElementString("locale-element", options.Locale);
				WriteEndElement();
			}
			if (!debug)
			{
				if (ver4)
					WriteElementString("omit-trace-statements", options.OmitTraces);
				if (options.Optimize)
					WriteElementString("optimize", true);
			}
			if (!options.ShowActionScriptWarnings)
				WriteElementString("show-actionscript-warnings", false);
			if (!options.ShowBindingWarnings)
				WriteElementString("show-binding-warnings", false);
			if (!options.ShowDeprecationWarnings)
				WriteElementString("show-deprecation-warnings", false);
			if (!options.ShowInvalidCss)
				WriteElementString("show-invalid-css-property-warnings", false);
			if (!options.ShowUnusedTypeSelectorWarnings)
				WriteElementString("show-unused-type-selector-warnings", false);
			if (!options.Strict || options.Es)
				WriteElementString("strict", false);
			if (!asc2 && !options.UseResourceBundleMetadata)
				WriteElementString("use-resource-bundle-metadata", false);
			if (debug || options.VerboseStackTraces)
				WriteElementString("verbose-stacktraces", true);
			else
				WriteElementString("verbose-stacktraces", false);
		}

        private void AddClasspaths(string[] additional)
		{
			if ((options.Classpaths == null || options.Classpaths.Length == 0)
				&& (additional == null || additional.Length == 0))
				return;

			WriteStartElement("source-path");
			WriteAttributeString("append", true);

			if (options.Classpaths != null)
			{
				foreach (string path in options.Classpaths)
				{
					if (path.Trim().Length == 0) continue;

					string classpath = Path.Combine(directory, path);

					if (Directory.Exists(classpath))
						WriteElementString("path-element", classpath);
				}
			}

			if (additional != null)
			{
				foreach (string path in additional)
				{
					if (string.IsNullOrWhiteSpace(path)) continue;

					string classpath = Path.Combine(directory, path.Trim());

					if (Directory.Exists(classpath))
						WriteElementString("path-element", classpath);
				}
			}

			WriteEndElement();
		}

        private void AddIncludeLibraries()
		{
			if (options.IncludeLibraries == null || options.IncludeLibraries.Length == 0)
				return;

			WriteStartElement("include-libraries");

			foreach (string text in options.IncludeLibraries)
			{
				if (text.Trim().Length == 0) continue;

				string path = Path.Combine(directory, text);

				if (File.Exists(path))
				{
					WriteElementString("library", path);
				}
				else if (Directory.Exists(path))
				{
					string[] files = Directory.GetFiles(path, "*.swc");

					foreach (string file in files)
						WriteElementString("library", file);
				}
			}

			WriteEndElement();
		}

        private void AddExternalLibraryPaths()
		{
			if ((options.ExternalLibraryPaths == null || options.ExternalLibraryPaths.Length == 0)
				&& (options.LibraryPaths == null || options.LibraryPaths.Length == 0))
				return;

			WriteStartElement("external-library-path");
			WriteAttributeString("append", true);

			if (options.ExternalLibraryPaths != null && options.ExternalLibraryPaths.Length != 0)
			{
				foreach (string text in options.ExternalLibraryPaths)
				{
					if (text.Trim().Length == 0) continue;

					string path = Path.Combine(directory, text);

					if (File.Exists(path) || Directory.Exists(path))
						WriteElementString("path-element", path);
				}
			}

			if (options.LibraryPaths != null && options.LibraryPaths.Length != 0)
			{
				foreach (string text in options.LibraryPaths)
				{
					if (text.Trim().Length == 0) continue;

					string path = Path.Combine(directory, text);

					if (File.Exists(path) || Directory.Exists(path))
						WriteElementString("path-element", path);
				}
			}

			WriteEndElement();
		}

        private void AddLibraryPaths()
		{
			//if (options.LibraryPaths == null || options.LibraryPaths.Length == 0)
			//	return;

			//WriteStartElement("library-path");
			//WriteAttributeString("append", true);

			//foreach (string text in options.LibraryPaths)
			//{
			//	if (text.Trim().Length == 0) continue;

			//	string path = Path.Combine(directory, text);

			//	if (File.Exists(path) || Directory.Exists(path))
			//		WriteElementString("path-element", path);
			//}

			//WriteEndElement();
		}

        private void AddRsls()
		{
			if (options.RslPaths == null || options.RslPaths.Length == 0)
				return;

			foreach (string text in options.RslPaths)
			{
				string[] list = text.Split(',');

				if (list.Length == 1 || list[0].Trim().Length == 0)
					continue;

				string path = Path.Combine(directory, list[0]);

				if (!File.Exists(path)) continue;

				WriteStartElement("runtime-shared-library-path");
				WriteElementString("path-element", path);
				WriteElementString("rsl-url", list[1]);

				if (list.Length > 2)
					WriteElementString("policy-file-url", list[2]);
				if (list.Length > 3)
					WriteElementString("rsl-url", list[3]);
				if (list.Length > 4)
					WriteElementString("policy-file-url", list[4]);

				WriteEndElement();
			}
		}

        private void AddMovieOptions()
		{
			//WriteElementString("default-background-color", options.Background);
			//WriteElementString("default-frame-rate", options.Fps);
			//WriteStartElement("default-size");
			//WriteElementString("width", options.Width);
			//WriteElementString("height", options.Height);
			//WriteEndElement();
		}

        private void WriteStartDocument() => output.WriteStartDocument();

        private void WriteEndDocument() => output.WriteEndDocument();

        private void WriteStartElement(string localName) => output.WriteStartElement(localName);

        private void WriteEndElement() => output.WriteEndElement();

        private void WriteComment(string text) => output.WriteComment(text);

        private void WriteElementString(string localName, bool value) => WriteElementString(localName, Flag(value));

        private void WriteElementString(string localName, string value) => output.WriteElementString(localName, value);

        private void WriteAttributeString(string localName, bool value) => WriteAttributeString(localName, Flag(value));

        private void WriteAttributeString(string localName, string value) => output.WriteAttributeString(localName, value);

        private void WriteDefine(string name, bool flag) => WriteDefine(name, Flag(flag));

        private void WriteDefine(string name, string value)
		{
			WriteStartElement("define");
			WriteAttributeString("append", true);
			WriteElementString("name", name);
			WriteElementString("value", value);
			WriteEndElement();
		}

        private static string Flag(bool value) => ConvertHelper.ToString(value);

		void IDisposable.Dispose() => ((IDisposable) output)?.Dispose();
	}
}