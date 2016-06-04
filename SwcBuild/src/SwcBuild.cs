using System;
using System.IO;
using System.Xml;

namespace SwcBuild
{
	using static Console;
	using static ConsoleColor;
	using static Program;

	internal class SwcBuild : IDisposable
	{
		private const string AirSdkDescription = "air-sdk-description.xml";
		private const string FlexSdkDescription = "flex-sdk-description.xml";
		private const string ObjPath = "obj";

		public ActionScriptCompiler Compiler;

		public bool? Asdoc;
		public string CompilerPath;
		public bool? Debug;
		public bool? KeepAsdoc;
		public string Library;
		public string ProjectPath;

		private BuildOptions buildOptions;
		private string compilerDirectory;
		private string sdkName;
		private Version sdkVersion;

		public SwcBuild()
		{
			Compiler = null;

			Asdoc = new bool?();
			CompilerPath = null;
			Debug = new bool?();
			KeepAsdoc = new bool?();
			Library = null;
			ProjectPath = null;

			compilerDirectory = null;
			sdkName = null;
			sdkVersion = null;
		}

		public ExitCodes Run(string[] args)
		{
			try
			{
				/* 
				 * Start marker
				 */
				WriteLine($"==============={Assembly.Description}===============");
				DisplayVersion();
				WriteLine();

				/* 
				 * Initialize build with arguments
				 */
				var exitCode = ExitCodes.NoError;
				if (CheckNonBuildSyntax(args, out exitCode)) return exitCode;
				if (InitializeArguments(args, out exitCode)) return exitCode;

				WriteLine($"Building {Path.GetFileNameWithoutExtension(ProjectPath)}...");
				WriteLine($"Compiler version: {Compiler.Version}");

				/* 
				 * Initialize the compiler
				 */
				if (GetSdkInfo(out exitCode)) return exitCode;
				Compiler.CompilerPath = CompilerPath;
				Compiler.CurrentDirectory = Directory.GetCurrentDirectory();
				Compiler.SdkVersion = sdkVersion;

				WriteLine($"SDK version: {sdkName} build {sdkVersion.Revision}");

				/* 
				 * Load project file
				 */
				var xmlDoc = new XmlDocument() { PreserveWhitespace = false };
				if (LoadProjectFile(xmlDoc, out exitCode)) return exitCode;

				WriteLine($"Project file loaded.");

				/* 
				 * Build configuration file
				 */
				buildOptions = new BuildOptions();
				bool replaced;
				if (buildOptions.Parse(xmlDoc.DocumentElement, out exitCode)) return exitCode;
				if (GetTargetPlayer(out exitCode)) return exitCode;
				if (WriteConfigXml(ObjPath, out replaced, out exitCode)) return exitCode;

				WriteLineAssert($"Configuration file created: {Compiler.ConfigFile}", replaced);

				/* 
				 * Build compiler arguments
				 */
				var arguments = new CompilerArguments();
				Compiler.Arguments = arguments;
				if (Compiler.BuildArguments(buildOptions, Debug.Value, false, out exitCode)) return exitCode;

				WriteLine($"Compiler arguments: {Compiler.Arguments.CompcArgs}");

				/* 
				 * Run the compiler
				 */
				WriteLine($"Starting {CompilerPath}...");
				if (Compiler.Run(out exitCode)) return exitCode;

				WriteLineAssert($"Build succeeded ({Compiler.ExitCode}).", Compiler.ExitCode == 0);
				WriteLineAssert($"Build failed ({Compiler.ExitCode}).", Compiler.ExitCode != 0);

				if (exitCode != 0 || Debug.Value || !Asdoc.Value) return exitCode;

				/*
				 * Initialize asdoc
				 */
				var asdoc = new AsDocCreator
				{
					Arguments = arguments,
					CompilerPath = Compiler.GetAsDocPath(),
					ConfigFile = Path.Combine(ObjPath, "ASDocConfig.xml"),
					CurrentDirectory = Directory.GetCurrentDirectory(),
					Exclude = new[] { "ASDoc_Config.xml", "overviews.xml" },
					Output = Path.Combine(ObjPath, "TempDoc")
				};
				asdoc.BuildArguments();

				WriteLine($"Creating asdoc: {asdoc.Arguments.AsdocArgs}");

				/*
				 * Run asdoc
				 */
				if (!asdoc.Run(out exitCode))
				{
					WriteLine($"Including asdoc in {Compiler.Output}...");
					if (asdoc.AddToSwc(Compiler.Output, KeepAsdoc.Value, out exitCode)) return exitCode;
				}

				WriteLineAssert($"Build succeeded ({asdoc.ExitCode}).", asdoc.ExitCode == 0);
				WriteLineAssert($"Build failed ({asdoc.ExitCode}).", asdoc.ExitCode != 0);

				return exitCode;
			}
			catch (Exception e)
			{
				/* 
				 * Show error message
				 */
				Error.WriteLine(e);
				WriteLine($"{Assembly.Description} terminating due to error...");

				return ExitCodes.Unknown;
			}
			finally
			{
				/* 
				 * End marker
				 */
				WriteLine("======================================");
			}
		}

		private static void DisplayVersion()
		{
			var ver = Assembly.Version;

			WriteLine($"Version {ver.Major}.{ver.Minor}.{ver.Build} build {ver.Revision}");
			WriteLine(Assembly.Copyright.Replace("©", "(c)"));
		}

		private static void ShowHelp()
		{
			WriteLine("[Syntax]");
			WriteLine();
			WriteColor($"{Assembly.Product}", White);
			WriteArgument(null, null, "project");
			WriteArgument(Argument.Compiler, "=", "path");
			WriteArgument(Argument.Library, "=", "path", true);
			WriteArgument(Argument.Debug, "=", "flag", true, true);
			WriteArgument(Argument.Asdoc, "=", "flag", true, true);
			WriteArgument(Argument.KeepAsdoc, "=", "flag", true, true);
			WriteLine();
			WriteLine("<project>       The path to the .as3project file of the FlashDevelop project.");
			WriteArgumentDesc(Argument.Compiler, Argument.CompilerAlias, "The path to the folder containing the Adobe Flex | Air SDK.");
			WriteArgumentDesc(Argument.Library, Argument.LibraryAlias, "The path to FlashDevelop Library folder.", true);
			WriteArgumentDesc(Argument.Debug, Argument.DebugAlias, "Whether to compile the file as debug mode. Defaults to false.", true, true);
			WriteArgumentDesc(Argument.Asdoc, Argument.AsdocAlias, "Whether to include AS Documentation in the SWC. This option is ignored when debug is set to true. Defaults to false.", true, true);
			WriteArgumentDesc(Argument.KeepAsdoc, Argument.KeepAsdocAlias, "Whether to keep generated asdoc files. Defaults to false.", true, true);
			WriteLine();
			WriteColor($"{Assembly.Product}", White);
			WriteArgument(Argument.Help, null, null);
			WriteColor(" |", White);
			WriteArgument(Argument.Info, " ", "ErrorCode");
			WriteColor(" |", White);
			WriteArgument(Argument.Version, null, null);
			WriteLine();
			WriteArgumentDesc(Argument.Help, Argument.HelpAlias, "Display this help menu.");
			WriteArgumentDesc(Argument.Info, Argument.InfoAlias, "Display the exit code information.");
			WriteArgumentDesc(Argument.Version, Argument.VersionAlias, "Display the program version.");
		}

		private static void WriteArgument(string arg, string del, string value, bool optional = false, bool optionalValue = false)
		{
			Write(" ");
			if (optional) Write("[", White);
			if (!string.IsNullOrEmpty(arg))
			{
				Write("-");
				WriteColor(arg, optional ? DarkCyan : Cyan);
			}
			if (optionalValue) WriteColor("[", White);
			if (!string.IsNullOrEmpty(value))
			{
				Write(del);
				WriteColor("<" + value + ">", optionalValue ? DarkYellow : Green);
			}
			if (optionalValue) WriteColor("]", White);
			if (optional) WriteColor("]", White);
		}

		private static void WriteArgumentDesc(string arg, string alias, string desc, bool optional = false, bool flag = false)
		{
			Write("-" + arg.PadRight(15));
			if (!string.IsNullOrEmpty(alias))
			{
				Write("Alias: ");
				WriteColor("-" + alias, White);
			}
			if (optional) WriteColor(" [Optional]", DarkCyan);
			if (flag) WriteColor(" [Flag]", DarkYellow);
			WriteLine();
			WriteLine("                " + desc);
		}

		private static bool CheckNonBuildSyntax(string[] args, out ExitCodes exitCode)
		{
			if (args.Length > 0)
			{
				string key = args[0];

				if (key.Length > 0 && key[0] == '-')
				{
					switch (key.Substring(1).ToLower())
					{
						case Argument.Help:
						case Argument.HelpAlias:
							ShowHelp();

							exitCode = ExitCodes.NoError;
							return true;

						case Argument.Info:
						case Argument.InfoAlias:
							if (args.Length == 1) return ErrorHelper.ValueExpected(out exitCode, Argument.Info);

							byte code;
							if (!byte.TryParse(args[1], out code) || !Enum.IsDefined(typeof(ExitCodes), code))
								return ErrorHelper.IncorrectArgumentValue(out exitCode, Argument.Info, args[1]);

							WriteLine($"ExitCode ({code}): {nameof(ExitCodes)}.{(ExitCodes) code}");

							exitCode = ExitCodes.NoError;
							return true;

						case Argument.Version:
						case Argument.VersionAlias:
							DisplayVersion();

							exitCode = ExitCodes.NoError;
							return true;
					}
				}
			}

			exitCode = 0;
			return false;
		}

		private bool InitializeArguments(string[] args, out ExitCodes exitCode)
		{
			if (args.Length == 0)
			{
				ErrorHelper.MissingProjectPath(out exitCode);
				WriteLine();
				ShowHelp();
				return true;
			}

			ProjectPath = args[0];

			if (ProjectPath.Length == 0 || ProjectPath[0] == '-')
				return ErrorHelper.MissingProjectPath(out exitCode);

			//if (!Path.IsPathRooted(ProjectPath))
			ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), ProjectPath);

			if (!File.Exists(ProjectPath))
			{
				if (!Path.HasExtension(ProjectPath) && File.Exists(ProjectPath + String.As3Proj))
					ProjectPath += String.As3Proj;
				else
					return ErrorHelper.InvalidPathToProjectFile(out exitCode, ProjectPath);
			}

			Directory.SetCurrentDirectory(Path.GetDirectoryName(ProjectPath));
			ProjectPath = Path.GetFileName(ProjectPath);

			for (int i = 1; i < args.Length; i++)
			{
				string arg = args[i];

				if (arg.Length == 0)
					continue;

				if (arg[0] != '-')
					return ErrorHelper.ExpectedArgument(out exitCode);

				int equalIndex = arg.IndexOf('=');

				if (equalIndex == -1)
				{
					string key = arg.Substring(1).ToLower();

					switch (key)
					{
						case Argument.Asdoc:
						case Argument.AsdocAlias:
							if (Asdoc.HasValue) return ErrorHelper.RepeatedArgument(out exitCode, key);

							Asdoc = new bool?(true);
							break;

						case Argument.Debug:
						case Argument.DebugAlias:
							if (Debug.HasValue) return ErrorHelper.RepeatedArgument(out exitCode, key);

							Debug = new bool?(true);
							break;

						case Argument.KeepAsdoc:
						case Argument.KeepAsdocAlias:
							if (KeepAsdoc.HasValue) return ErrorHelper.RepeatedArgument(out exitCode, key);

							KeepAsdoc = new bool?(true);
							break;

						case Argument.Compiler:
						case Argument.CompilerAlias:
						case Argument.Library:
						case Argument.LibraryAlias:
							return ErrorHelper.ValueExpected(out exitCode, key);

						default:
							return ErrorHelper.UnexpectedArgument(out exitCode, key);
					}
				}
				else
				{
					string key = arg.Substring(1, equalIndex - 1).ToLower();
					string value = arg.Substring(equalIndex + 1);

					switch (key)
					{
						case Argument.Asdoc:
						case Argument.AsdocAlias:
							if (Asdoc.HasValue) return ErrorHelper.RepeatedArgument(out exitCode, key);

							switch (value.ToLower())
							{
								case Value.Empty:
								case Value.False:
									Asdoc = new bool?(false);
									break;
								case Value.True:
									Asdoc = new bool?(true);
									break;
								default:
									return ErrorHelper.IncorrectArgumentValue(out exitCode, key, value);
							}
							break;

						case Argument.Compiler:
						case Argument.CompilerAlias:
							if (CompilerPath != null) return ErrorHelper.RepeatedArgument(out exitCode, key);
							if (value == Value.Empty) return ErrorHelper.EmptyValue(out exitCode, key);

							string tempPath;

							if (File.Exists(tempPath = Path.Combine(value, String.Bin, String.CompcBat)) //$(CompilerPath)
								|| File.Exists(tempPath = Path.Combine(value, String.CompcBat)) //$(CompilerPath)\bin
								|| Path.GetFileName(tempPath = value) == String.CompcBat && File.Exists(value) //$(CompilerPath)\bin\compc.bat
								|| File.Exists(tempPath = Path.ChangeExtension(value, String.Bat))) //$(CompilerPath)\bin\compc.*
								Compiler = ActionScriptCompiler.ASC2;
							else if (File.Exists(tempPath = Path.Combine(value, String.Bin, String.CompcExe)) //$(CompilerPath)
									 || File.Exists(tempPath = Path.Combine(value, String.CompcExe)) //$(CompilerPath)\bin
									 || Path.GetFileName(tempPath = value) == String.CompcExe && File.Exists(value) //$(CompilerPath)\bin\compc.exe
									 || File.Exists(tempPath = Path.ChangeExtension(value, String.Exe))) //$(CompilerPath)\bin\compc.*
								Compiler = ActionScriptCompiler.ASC1;
							else
								return ErrorHelper.InvalidPathToCompiler(out exitCode, value);

							compilerDirectory = Path.GetDirectoryName(Path.GetDirectoryName(tempPath));
							CompilerPath = tempPath;
							break;

						case Argument.Debug:
						case Argument.DebugAlias:
							if (Debug.HasValue) return ErrorHelper.RepeatedArgument(out exitCode, key);

							switch (value.ToLower())
							{
								case Value.Empty:
								case Value.False:
								case Value.Release:
									Debug = new bool?(false);
									break;
								case Value.True:
								case Value.Debug:
									Debug = new bool?(true);
									break;
								default:
									return ErrorHelper.IncorrectArgumentValue(out exitCode, key, value);
							}
							break;

						case Argument.KeepAsdoc:
						case Argument.KeepAsdocAlias:
							if (KeepAsdoc.HasValue) return ErrorHelper.RepeatedArgument(out exitCode, key);

							switch (value.ToLower())
							{
								case Value.Empty:
								case Value.False:
									KeepAsdoc = new bool?(false);
									break;
								case Value.True:
									KeepAsdoc = new bool?(true);
									break;
								default:
									return ErrorHelper.IncorrectArgumentValue(out exitCode, key, value);
							}
							break;

						case Argument.Library:
						case Argument.LibraryAlias:
							if (Library != null) return ErrorHelper.RepeatedArgument(out exitCode, key);
							if (value == Value.Empty) return ErrorHelper.EmptyValue(out exitCode, key);
							if (!Directory.Exists(value))
								return ErrorHelper.InvalidPathToLibrary(out exitCode, value);

							Library = value;
							break;

						default:
							return ErrorHelper.UnexpectedArgument(out exitCode, key);
					}
				}
			}

			if (compilerDirectory == null)
				return ErrorHelper.MissingCompilerDir(out exitCode);

			if (!Asdoc.HasValue)
				Asdoc = new bool?(false);

			if (!Debug.HasValue)
				Debug = new bool?(false);

			if (!KeepAsdoc.HasValue)
				KeepAsdoc = new bool?(false);

			if (!string.IsNullOrEmpty(Library))
				Library = Path.Combine(Library, "AS3", "classes");

			exitCode = 0;
			return false;
		}

		private bool GetSdkInfo(out ExitCodes exitCode)
		{
			string descriptionXml;

			if (!File.Exists(descriptionXml = Path.Combine(compilerDirectory, AirSdkDescription))
				&& !File.Exists(descriptionXml = Path.Combine(compilerDirectory, FlexSdkDescription)))
				return ErrorHelper.MissingSdkDescription(out exitCode);

			try
			{
				var xmlDoc = new XmlDocument() { PreserveWhitespace = false };
				xmlDoc.Load(descriptionXml);

				var doc = xmlDoc.DocumentElement;
				sdkName = doc["name"].FirstChild.Value;
				sdkVersion = new Version($"{doc["version"].FirstChild.Value}.{doc["build"].FirstChild.Value}");

				exitCode = 0;
				return false;
			}
			catch (Exception e)
			{
				return ErrorHelper.ErrorLoadingSdkDescription(out exitCode, descriptionXml, e);
			}
		}

		private bool LoadProjectFile(XmlDocument xmlDoc, out ExitCodes exitCode)
		{
			try
			{
				xmlDoc.Load(ProjectPath);

				exitCode = 0;
				return false;
			}
			catch (Exception e)
			{
				return ErrorHelper.ErrorLoadingProjectFile(out exitCode, e);
			}
		}

		private bool GetTargetPlayer(out ExitCodes exitCode)
		{
			var targetPlayer = default(Version);
			string version = $"{buildOptions.Version.Trim()}.";
			int minorVersion;

			if (int.TryParse(buildOptions.MinorVersion2.Trim(), out minorVersion))
				version += minorVersion;
			else
				version += buildOptions.MinorVersion.Trim();

			if (!Version.TryParse(version, out targetPlayer))
				return ErrorHelper.InvalidFormat(out exitCode, $@"{ProjectPath}\version", version);

			Compiler.TargetPlayer = targetPlayer;

			exitCode = 0;
			return false;
		}

		private bool WriteConfigXml(string directory, out bool replaced, out ExitCodes exitCode)
		{
			const string extOld = "old";
			const string extXml = "xml";
			const string extTmp = "tmp";

			replaced = false;

			try
			{
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				string name = Path.GetFileNameWithoutExtension(ProjectPath).Replace(" ", string.Empty);
				string old = Path.Combine(directory, $"{name}Config.{extOld}");
				string conf = Path.Combine(directory, $"{name}Config.{extXml}");
				string temp = Path.Combine(directory, $"{name}Config.{extTmp}");

				using (var writer = new ConfigWriter(Compiler.CurrentDirectory, temp))
				{
					var options = buildOptions;
					string targetPlayer = Compiler.TargetPlayer.ToString(2);
					string[] libraries = { Library };
					bool asc2 = Compiler.Version == ASCVersions.ASC2;
					bool debug = Debug.Value;

					writer.WriteConfig(options, sdkVersion, targetPlayer, libraries, asc2, debug);
					Compiler.ConfigFile = conf;
				}

				if (File.Exists(conf))
				{
					if (!Utilities.AreFilesEqual(temp, conf))
					{
						File.Copy(conf, old, true);
						File.Copy(temp, conf, true);
						replaced = true;
					}

					File.Delete(temp);
				}
				else
				{
					File.Move(temp, conf);
				}

				exitCode = 0;
				return false;
			}
			catch (Exception e)
			{
				return ErrorHelper.ErrorWritingConfig(out exitCode, e);
			}
		}

		private static void WriteColor(string text, ConsoleColor color)
		{
			var tmpColor = ForegroundColor;
			ForegroundColor = color;
			Write(text);
			ForegroundColor = tmpColor;
		}

		private static void WriteLineAssert(string text, bool condition)
		{
			if (condition) WriteLine(text);
		}

		void IDisposable.Dispose() => ((IDisposable) Compiler)?.Dispose();
	}
}
