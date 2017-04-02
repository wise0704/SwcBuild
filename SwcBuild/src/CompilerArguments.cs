using System.Text;

namespace SwcBuild
{
    internal class CompilerArguments
    {
        private StringBuilder compcArgs;
        private StringBuilder asdocArgs;

        public CompilerArguments()
        {
            compcArgs = new StringBuilder();
            asdocArgs = new StringBuilder();
        }

        public string Configname
        {
            set
            {
                compcArgs.Append($"+configname={value} ");
                asdocArgs.Append($"+configname={value} ");
            }
        }

        public string LoadConfig
        {
            set
            {
                compcArgs.Append($"-load-config+={value} ");
                asdocArgs.Append($"-load-config+={value} ");
            }
        }

        public bool Debug
        {
            set
            {
                compcArgs.Append($"-debug={Flag(value)} ");
                asdocArgs.Append($"-debug={Flag(value)} ");
            }
        }

        public bool Incremental { set => compcArgs.Append($"-incremental={Flag(value)} "); }

        public bool AdvancedTelemetry { set => compcArgs.Append($"-advanced-telemetry={Flag(value)} "); }

        public string AdvancedTelemetryPassword { set => compcArgs.Append($"-advanced-telemetry-password={value} "); }

        public bool Inline { set => compcArgs.Append($"-inline={Flag(value)} "); }

        public string LinkReport { set => compcArgs.Append($"-link-report={value} "); }

        public string LoadExterns { set => compcArgs.Append($"-load-externs={value} "); }

        public int SwfVersion { set => compcArgs.Append($"-swf-version={value} "); }

        public string Sources
        {
            set
            {
                compcArgs.Append($"-include-sources+={value} ");
                asdocArgs.Append($"-doc-sources+={value} ");
            }
        }

        public string Additionals { set => compcArgs.Append(value); }

        public string Output { set => compcArgs.Append($"-output={value}"); }

        public bool Lenient { set => asdocArgs.Append($"-lenient={Flag(value)} "); }

        public bool KeepXml { set => asdocArgs.Append($"-keep-xml={Flag(value)} "); }

        public bool SkipXsl { set => asdocArgs.Append($"-skip-xsl={Flag(value)} "); }

        public string CompcArgs => compcArgs.ToString();

        public string AsdocArgs => asdocArgs.ToString();

        private static string Flag(bool value) => ConvertHelper.ToString(value);
    }
}
