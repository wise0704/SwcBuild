using System;
using System.IO;
using System.Linq;
using SwcBuild.IO.Compression;

namespace SwcBuild
{
    using static ExitCodes;
    using static FileMode;
    using static ZipArchiveMode;

    internal class AsDocCreator : CompilerProcess
    {
        public AsDocCreator() : base(true, false) { }

        public string[] Exclude { get; set; }

        public void BuildArguments()
        {
            if (File.Exists(ConfigFile))
                Arguments.LoadConfig = ConfigFile;

            Arguments.Lenient = true;
            Arguments.KeepXml = true;
            Arguments.SkipXsl = true;
        }

        public bool Run(out ExitCodes exitCode)
        {
            try
            {
                Run($"{Arguments.AsdocArgs}-output={Output}");

                if (ExitCode == 0)
                {
                    exitCode = 0;
                    return false;
                }

                exitCode = ErrorRunningAsDoc;
                return true;
            }
            catch (Exception e)
            {
                return ErrorHelper.ErrorRunningAsDoc(out exitCode, e);
            }
        }

        public bool AddToSwc(string target, bool keepXml, out ExitCodes exitCode)
        {
            try
            {
                using (var swcFile = new FileStream(target, Open))
                using (var archive = new ZipArchive(swcFile, Update))
                {
                    string directory = Path.Combine(Output, "tempdita");

                    foreach (string file in Directory.GetFiles(directory))
                    {
                        string filename = Path.GetFileName(file);

                        if (filename == null || Exclude.Contains(filename))
                            continue;

                        string destination = Path.Combine("docs", filename);

                        using (var original = File.OpenRead(file))
                        using (var compressed = archive.CreateEntry(destination).Open())
                        {
                            original.CopyTo(compressed);
                        }
                    }
                }

                if (!keepXml)
                    Directory.Delete(Output, true);

                exitCode = 0;
                return false;
            }
            catch (Exception e)
            {
                return ErrorHelper.ErrorRunningAsDoc(out exitCode, e);
            }
        }
    }
}
