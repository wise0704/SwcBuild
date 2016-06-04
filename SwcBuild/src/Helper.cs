using System;

namespace SwcBuild
{
	using static Console;
	using static StringComparison;

    internal static class ErrorHelper
	{
		public static bool MissingProjectPath(out ExitCodes exitCode)
		{
			Error.WriteLine("Error: Missing project file path.");
			exitCode = ExitCodes.MissingArgument;
			return true;
		}

		public static bool MissingCompilerDir(out ExitCodes exitCode)
		{
			Error.WriteLine("Error: Missing compiler directory.");
			exitCode = ExitCodes.MissingArgument;
			return true;
		}

		public static bool ValueExpected(out ExitCodes exitCode, string key)
		{
			Error.WriteLine($"Error: Value is expected for argument \"{key}\".");
			Error.WriteLine("For details of arguments, use -help");
			exitCode = ExitCodes.ValueExpected;
			return true;
		}

		public static bool ExpectedArgument(out ExitCodes exitCode)
		{
			Error.WriteLine("Error: Expected an argument prefixed with \'-\'.");
			Error.WriteLine("For details of arguments, use -help");
			exitCode = ExitCodes.ExpectedArgument;
			return true;
		}

		public static bool UnexpectedArgument(out ExitCodes exitCode, string key)
		{
			Error.WriteLine($"Error: Unexpected argument: \"{key}\"");
			Error.WriteLine("For details of arguments, use -help");
			exitCode = ExitCodes.UnexpectedArgument;
			return true;
		}

		public static bool EmptyValue(out ExitCodes exitCode, string key)
		{
			Error.WriteLine($"Error: Value must not be empty for argument \"{key}\".");
			exitCode = ExitCodes.EmptyValue;
			return true;
		}

		public static bool IncorrectArgumentValue(out ExitCodes exitCode, string key, string value)
		{
			Error.WriteLine($"Error: Incorrect value for argument \"{key}\": {value}");
			exitCode = ExitCodes.IncorrectArgumentValue;
			return true;
		}

		public static bool RepeatedArgument(out ExitCodes exitCode, string key)
		{
			Error.WriteLine($"Error: Argument \"{key}\" is defined multiple times.");
			exitCode = ExitCodes.RepeatedArgument;
			return true;
		}

		public static bool InvalidPathToProjectFile(out ExitCodes exitCode, string path)
		{
			Error.WriteLine($"Error: Incorrect path to project file: \"{path}\"");
			exitCode = ExitCodes.InvalidPath;
			return true;
		}

		public static bool InvalidPathToCompiler(out ExitCodes exitCode, string path)
		{
			Error.WriteLine($"Error: Incorrect path to compiler: \"{path}\"");
			exitCode = ExitCodes.InvalidPath;
			return true;
		}

		public static bool InvalidPathToLibrary(out ExitCodes exitCode, string path)
		{
			Error.WriteLine($"Error: Incorrect path to library: \"{path}\"");
			exitCode = ExitCodes.InvalidPath;
			return true;
		}

		public static bool InvalidPathToOutput(out ExitCodes exitCode)
		{
			Error.WriteLine("Error: Specified output filename is empty.");
			exitCode = ExitCodes.InvalidPath;
			return true;
		}

		public static bool MissingSdkDescription(out ExitCodes exitCode)
		{
			Error.WriteLine("Error: Could not find the sdk description XML file.");
			exitCode = ExitCodes.MissingSdkDescription;
			return true;
		}

		public static bool InvalidFormat(out ExitCodes exitCode, string key, string value)
		{
			Error.WriteLine($"Error: Input is in invalid format. \"{key}\": {value}");
			exitCode = ExitCodes.InvalidFormat;
			return true;
		}

		public static bool ErrorLoadingSdkDescription(out ExitCodes exitCode, string filename, Exception e)
		{
			Error.WriteLine($"Error: Failed to read SDK description: {filename}\n  {e.Message}");
			exitCode = ExitCodes.InvalidFormat;
			return true;
		}

		public static bool ErrorLoadingProjectFile(out ExitCodes exitCode, Exception e)
		{
			Error.WriteLine($"Error: Error loading the project file:\n  {e.Message}");
			exitCode = ExitCodes.InvalidFormat;
			return true;
		}

	    public static bool ErrorParsingProjectFile(out ExitCodes exitCode, Exception e)
	    {
			Error.WriteLine($"Error: Error parsing the project file:\n  {e.Message}");
			exitCode = ExitCodes.InvalidFormat;
			return true;
		}

		public static bool ErrorWritingConfig(out ExitCodes exitCode, Exception e)
		{
			Error.WriteLine($"Error: An error occurred while writing the configuration file.\n  {e.Message}");
			exitCode = ExitCodes.ErrorWritingConfig;
			return true;
		}

		public static bool ErrorRunningCompiler(out ExitCodes exitCode, Exception e)
		{
			Error.WriteLine($"Error: An error occurred while running the compiler.\n  {e.Message}");
			exitCode = ExitCodes.ErrorRunningCompiler;
			return true;
		}

		public static bool ErrorRunningAsDoc(out ExitCodes exitCode, Exception e)
		{
			Error.WriteLine($"Error: An error occurred while running asdoc.\n  {e.Message}");
			exitCode = ExitCodes.ErrorRunningAsDoc;
			return true;
		}
	}

    internal static class ConvertHelper
	{
	    private const string True = "true";
	    private const string False = "false";

		public static bool ToBoolean(string value)
		{
			if (True.Equals(value, OrdinalIgnoreCase))
				return true;
			if (False.Equals(value, OrdinalIgnoreCase))
				return false;
			throw new FormatException("Format_BadBoolean");
		}

		public static string ToString(bool value) => value ? True : False;
	}
}
