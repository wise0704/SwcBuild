using System;
using System.Xml;

namespace SwcBuild
{
    using A = BuildOptionAttributes;
    using E = BuildOptionElements;
    using G = BuildOptionGroups;
    using list = XmlNodeList;
    using xml = XmlElement;

    internal class BuildOptions
    {
        public string OutputType = null;
        public string Input = null;
        public string Path = null;
        public string Fps = null;
        public string Width = null;
        public string Height = null;
        public string Version = null;
        public string MinorVersion = null;
        public string Platform = null;
        public string Background = null;

        public bool Accessible = false;
        public bool AdvancedTelemetry = false;
        public bool AllowSourcePathOverlap = false;
        public bool Benchmark = false;
        public bool Es = false;
        public bool Inline = false;
        public bool Optimize = false;
        public bool OmitTraces = false;
        public bool ShowActionScriptWarnings = true;
        public bool ShowBindingWarnings = true;
        public bool ShowDeprecationWarnings = true;
        public bool ShowInvalidCss = true;
        public bool ShowUnusedTypeSelectorWarnings = true;
        public bool StaticLinkRSL = false;
        public bool Strict = true;
        public bool UseNetwork = true;
        public bool UseResourceBundleMetadata = true;
        public bool VerboseStackTraces = false;
        public bool Warnings = true;

        public string AdvancedTelemetryPassword = null;
        public string Locale = null;
        public string LoadConfig = null;
        public string LinkReport = null;
        public string LoadExterns = null;
        public string Additional = null;
        public string MinorVersion2 = null;
        public string CompilerConstants = null;

        public string[] Classpaths = null;
        public string[] ExternalLibraryPaths = null;
        public string[] IncludeLibraries = null;
        public string[] LibraryPaths = null;
        public string[] RslPaths = null;

        public BuildOptions() { }

        public bool Parse(xml data, out ExitCodes exitCode)
        {
            try
            {
                // <output><movie>
                var group = data[G.Output];
                var elements = group?.GetElementsByTagName(E.Movie);
                ParseOutputOptions(elements);

                // <build><option>
                group = data[G.Build];
                elements = group?.GetElementsByTagName(E.Option);
                ParseBuildOptions(elements);

                // <classpaths><class>
                group = data[G.Classpaths];
                elements = group?.GetElementsByTagName(E.Class);
                ParseClasspaths(elements);

                // <includeLibraries><element>
                group = data[G.IncludeLibraries];
                elements = group?.GetElementsByTagName(E.Element);
                ParseIncludeLibraries(elements);

                // <libraryPaths><element>
                group = data[G.LibraryPaths];
                elements = group?.GetElementsByTagName(E.Element);
                ParseLibraryPaths(elements);

                // <externalLibraryPaths><element>
                group = data[G.ExternalLibraryPaths];
                elements = group?.GetElementsByTagName(E.Element);
                ParseExternalLibraryPaths(elements);

                // <externalLibraryPaths><element>
                group = data[G.RslPaths];
                elements = group?.GetElementsByTagName(E.Element);
                ParseRslPaths(elements);

                exitCode = 0;
                return false;
            }
            catch (FormatException e)
            {
                return ErrorHelper.ErrorParsingProjectFile(out exitCode, e);
            }
        }

        private void ParseOutputOptions(list elements)
        {
            if (elements == null) return;

            foreach (xml element in elements)
            {
                var attribute = element.Attributes[0];
                string value = attribute.Value;

                switch (attribute.Name)
                {
                    case A.OutputType: OutputType = value; break;
                    case A.Input: Input = value; break;
                    case A.Path: Path = value; break;
                    case A.Fps: Fps = value; break;
                    case A.Width: Width = value; break;
                    case A.Height: Height = value; break;
                    case A.Version: Version = value; break;
                    case A.MinorVersion: MinorVersion = value; break;
                    case A.Platform: Platform = value; break;
                    case A.Background: Background = value; break;
                }
            }
        }

        private void ParseBuildOptions(list elements)
        {
            if (elements == null) return;

            foreach (xml element in elements)
            {
                var attribute = element.Attributes[0];
                string value = attribute.Value;

                switch (attribute.Name)
                {
                    case A.Accessible: Accessible = Flag(value); break;
                    case A.AdvancedTelemetry: AdvancedTelemetry = Flag(value); break;
                    case A.AllowSourcePathOverlap: AllowSourcePathOverlap = Flag(value); break;
                    case A.Benchmark: Benchmark = Flag(value); break;
                    case A.Es: Es = Flag(value); break;
                    case A.Inline: Inline = Flag(value); break;
                    case A.Optimize: Optimize = Flag(value); break;
                    case A.OmitTraces: OmitTraces = Flag(value); break;
                    case A.ShowActionScriptWarnings: ShowActionScriptWarnings = Flag(value); break;
                    case A.ShowBindingWarnings: ShowBindingWarnings = Flag(value); break;
                    case A.ShowInvalidCss: ShowInvalidCss = Flag(value); break;
                    case A.ShowDeprecationWarnings: ShowDeprecationWarnings = Flag(value); break;
                    case A.ShowUnusedTypeSelectorWarnings: ShowUnusedTypeSelectorWarnings = Flag(value); break;
                    case A.Strict: Strict = Flag(value); break;
                    case A.UseNetwork: UseNetwork = Flag(value); break;
                    case A.UseResourceBundleMetadata: UseResourceBundleMetadata = Flag(value); break;
                    case A.Warnings: Warnings = Flag(value); break;
                    case A.VerboseStackTraces: VerboseStackTraces = Flag(value); break;
                    case A.StaticLinkRSL: StaticLinkRSL = Flag(value); break;

                    case A.AdvancedTelemetryPassword: AdvancedTelemetryPassword = value; break;
                    case A.Additional: Additional = value; break;
                    case A.CompilerConstants: CompilerConstants = value; break;
                    case A.Locale: Locale = value; break;
                    case A.LoadConfig: LoadConfig = value; break;
                    case A.MinorVersion: MinorVersion2 = value; break;
                }
            }
        }

        private void ParseClasspaths(list elements)
        {
            if (elements == null) return;

            Classpaths = new string[elements.Count];

            for (int i = 0; i < Classpaths.Length; i++)
                Classpaths[i] = elements[i].Attributes[A.Path].Value;
        }

        private void ParseIncludeLibraries(list elements)
        {
            if (elements == null) return;

            IncludeLibraries = new string[elements.Count];

            for (int i = 0; i < IncludeLibraries.Length; i++)
                IncludeLibraries[i] = elements[i].Attributes[A.Path].Value;
        }

        private void ParseLibraryPaths(list elements)
        {
            if (elements == null) return;

            LibraryPaths = new string[elements.Count];

            for (int i = 0; i < LibraryPaths.Length; i++)
                LibraryPaths[i] = elements[i].Attributes[A.Path].Value;
        }

        private void ParseExternalLibraryPaths(list elements)
        {
            if (elements == null) return;

            ExternalLibraryPaths = new string[elements.Count];

            for (int i = 0; i < ExternalLibraryPaths.Length; i++)
                ExternalLibraryPaths[i] = elements[i].Attributes[A.Path].Value;
        }

        private void ParseRslPaths(list elements)
        {
            if (elements == null) return;

            RslPaths = new string[elements.Count];

            for (int i = 0; i < RslPaths.Length; i++)
                RslPaths[i] = elements[i].Attributes[A.Path].Value;
        }

        private static bool Flag(string value) => ConvertHelper.ToBoolean(value);
    }
}
