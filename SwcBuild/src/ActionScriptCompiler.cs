using System;
using System.IO;

namespace SwcBuild
{
	using static Platform;
	using static Utilities;

    internal class ActionScriptCompiler : CompilerProcess
	{
		public static ActionScriptCompiler ASC1 => new ActionScriptCompiler(ASCVersions.ASC1);

		public static ActionScriptCompiler ASC2 => new ActionScriptCompiler(ASCVersions.ASC2);

        private ActionScriptCompiler(ASCVersions v) : base(true, true)
		{
			Version = v;
		}

		public Version SdkVersion { get; set; }

		public Version TargetPlayer { get; set; }

		public ASCVersions Version { get; }

		public bool BuildArguments(BuildOptions options, bool debug, bool incremental, out ExitCodes exitCode)
		{
			if (string.IsNullOrEmpty(options.Path))
				return ErrorHelper.InvalidPathToOutput(out exitCode);

			Output = FixOutputPath(options.Path, debug);

			string configname = GetConfigname(options.Platform);
			//string defaultConfig = Path.Combine(CompilerDirectory, "frameworks", $"{configname}-config.xml");

			string[] additional = ParseArguments(options.Additional);
			bool isConfignameDefined = false;
			bool isSwfVersionDefined = false;

			foreach (string argument in additional)
			{
				if (!isConfignameDefined && argument.Length > 12 && argument.Substring(0, 12) == "+configname=")
				{
					isConfignameDefined = true;
					if (isSwfVersionDefined) break;
					continue;
				}
				if (!isSwfVersionDefined && argument.Length > 13 && argument.Substring(0, 13) == "-swf-version=")
				{
					isSwfVersionDefined = true;
					if (isConfignameDefined) break;
				}
			}

			if (!isConfignameDefined && configname != "flex")
				Arguments.Configname = configname;

			//if (File.Exists(defaultConfig))
			//	Arguments.LoadConfig = Q(defaultConfig);

			if (File.Exists(options.LoadConfig))
				Arguments.LoadConfig = Q(options.LoadConfig);

			Arguments.LoadConfig = Q(ConfigFile);

			if (debug)
				Arguments.Debug = true;

			switch (Version)
			{
				case ASCVersions.ASC1:
					if (incremental)
						Arguments.Incremental = true;
					break;

				case ASCVersions.ASC2:
					if (options.AdvancedTelemetry)
					{
						Arguments.AdvancedTelemetry = true;

						if (!string.IsNullOrEmpty(options.AdvancedTelemetryPassword))
							Arguments.AdvancedTelemetryPassword = Q(options.AdvancedTelemetryPassword);
					}

					if (options.Inline)
						Arguments.Inline = true;
					break;
			}

			if (!string.IsNullOrEmpty(options.LinkReport))
				Arguments.LinkReport = options.LinkReport;

			if (!string.IsNullOrEmpty(options.LoadExterns))
				Arguments.LoadExterns = options.LoadExterns;

			if (!isSwfVersionDefined)
			{
				int swfVersion = ResolveSwfVersion(TargetPlayer, options.Platform != FlashPlayer);

				if (swfVersion != -1)
					Arguments.SwfVersion = swfVersion;
			}

			if (options.Classpaths != null)
				foreach (string classpath in options.Classpaths)
					Arguments.Sources = Q(classpath);

			foreach (string arg in additional)
				Arguments.Additionals = arg;

			Arguments.Output = Q(Output);

			exitCode = 0;
			return false;
		}

		public bool Run(out ExitCodes exitCode)
		{
			try
			{
				Run(Arguments.CompcArgs);

				exitCode = ExitCode == 0 ? 0 : ExitCodes.ErrorRunningCompiler;
				return false;
			}
			catch (Exception e)
			{
				return ErrorHelper.ErrorRunningCompiler(out exitCode, e);
			}
		}

		public string GetAsDocPath()
		{
            string path = Path.Combine(Path.GetDirectoryName(CompilerPath), String.AsDoc);

			switch (Version)
			{
				case ASCVersions.ASC1:
					return $"{path}{String.Exe}";
				case ASCVersions.ASC2:
					return $"{path}{String.Bat}";
				default:
					return null;
			}
		}

	    private static string Q(string text) => WrapWithQuotes(text);
	}

    internal enum ASCVersions
	{
		None,
		ASC1,
		ASC2,
	}
}
