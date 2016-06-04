using System;
using System.Diagnostics;

namespace SwcBuild
{
	using static Console;

    internal abstract class CompilerProcess : IDisposable
	{
	    private Process process;

		protected CompilerProcess(bool error, bool output)
		{
			process = new Process();

			if (error)
			{
				ErrorDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
						Error.WriteLine($"::  {e.Data}");
				};
			}

			if (output)
			{
				OutputDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
						Out.WriteLine($"::  {e.Data}");
				};
			}
		}

		public CompilerArguments Arguments { get; set; }

		public string CompilerPath
		{
			get { return process.StartInfo.FileName; }
			set { process.StartInfo.FileName = value; }
		}

		public string ConfigFile { get; set; }

		public string CurrentDirectory
		{
			get { return process.StartInfo.WorkingDirectory; }
			set { process.StartInfo.WorkingDirectory = value; }
		}

		public int ExitCode => process.ExitCode;

		public string Output { get; set; }

		public event DataReceivedEventHandler OutputDataReceived
		{
			add { process.OutputDataReceived += value; }
			remove { process.OutputDataReceived -= value; }
		}

		public event DataReceivedEventHandler ErrorDataReceived
		{
			add { process.ErrorDataReceived += value; }
			remove { process.ErrorDataReceived -= value; }
		}

		protected void Run(string arguments)
		{
			process.StartInfo.Arguments = arguments;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;

			process.Start();
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			process.WaitForExit();
		}

		void IDisposable.Dispose() => ((IDisposable) process)?.Dispose();
	}
}
