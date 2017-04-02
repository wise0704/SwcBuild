namespace SwcBuild
{
    internal static class Argument
    {
        public const string Asdoc = "asdoc", AsdocAlias = "a";
        public const string Compiler = "compiler", CompilerAlias = "c";
        public const string Debug = "debug", DebugAlias = "d";
        public const string KeepAsdoc = "keep-asdoc", KeepAsdocAlias = "k";
        public const string Help = "help", HelpAlias = "h";
        public const string Info = "info", InfoAlias = "i";
        public const string Library = "library", LibraryAlias = "l";
        public const string Version = "version", VersionAlias = "v";
    }

    internal static class Value
    {
        public const string Empty = "";
        public const string True = "true";
        public const string False = "false";
        public const string Debug = "debug";
        public const string Release = "release";
    }

    internal static class String
    {
        public const string As3Proj = ".as3proj";
        public const string AsDoc = "asdoc";
        public const string Bat = ".bat";
        public const string Bin = "bin";
        public const string CompcBat = "compc.bat";
        public const string CompcExe = "compc.exe";
        public const string Exe = ".exe";
        public const string Swc = ".swc";
    }

    internal static class BuildOptionGroups
    {
        public const string Output = "output";
        public const string Classpaths = "classpaths";
        public const string Build = "build";
        public const string IncludeLibraries = "includeLibraries";
        public const string LibraryPaths = "libraryPaths";
        public const string ExternalLibraryPaths = "externalLibraryPaths";
        public const string RslPaths = "rslPaths";
    }

    internal static class BuildOptionElements
    {
        public const string Movie = "movie";
        public const string Class = "class";
        public const string Option = "option";
        public const string Element = "element";
    }

    internal static class BuildOptionAttributes
    {
        public const string OutputType = "outputType";
        public const string Input = "input";
        public const string Path = "path";
        public const string Fps = "fps";
        public const string Width = "width";
        public const string Height = "height";
        public const string Version = "version";
        public const string MinorVersion = "minorVersion";
        public const string Platform = "platform";
        public const string Background = "background";

        public const string Accessible = "accessible";
        public const string AdvancedTelemetry = "advancedTelemetry";
        public const string AdvancedTelemetryPassword = "advancedTelemetryPassword";
        public const string AllowSourcePathOverlap = "allowSourcePathOverlap";
        public const string Benchmark = "benchmark";
        public const string Es = "es";
        public const string Inline = "inline";
        public const string Locale = "locale";
        public const string LoadConfig = "loadConfig";
        public const string Optimize = "optimize";
        public const string OmitTraces = "omitTraces";
        public const string ShowActionScriptWarnings = "showActionScriptWarnings";
        public const string ShowBindingWarnings = "showBindingWarnings";
        public const string ShowInvalidCss = "showInvalidCss";
        public const string ShowDeprecationWarnings = "showDeprecationWarnings";
        public const string ShowUnusedTypeSelectorWarnings = "showUnusedTypeSelectorWarnings";
        public const string Strict = "strict";
        public const string UseNetwork = "useNetwork";
        public const string UseResourceBundleMetadata = "useResourceBundleMetadata";
        public const string Warnings = "warnings";
        public const string VerboseStackTraces = "verboseStackTraces";
        public const string StaticLinkRSL = "staticLinkRSL";
        public const string Additional = "additional";
        public const string CompilerConstants = "compilerConstants";
    }

    internal static class Platform
    {
        public const string AirDesktop = "AIR";
        public const string AirMobile = "AIR Mobile";
        public const string FlashPlayer = "Flash Player";
    }
}
