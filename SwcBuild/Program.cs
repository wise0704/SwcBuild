using System;
using System.Reflection;

namespace SwcBuild
{
    using static Attribute;
    using static Console;

    internal static class Program
    {
        private static int Main(string[] args)
        {
            using (var swcBuild = new SwcBuild())
            {
                int exitCode = (int) swcBuild.Run(args);
                WriteLine($"{Assembly.Description} exited with code: {exitCode}");

                return exitCode;
            }
        }

        internal static class Assembly
        {
            private static readonly System.Reflection.Assembly assembly = typeof(Program).Assembly;

            public static string Title { get; } = Get<AssemblyTitleAttribute>().Title;
            public static string Description { get; } = Get<AssemblyDescriptionAttribute>().Description;
            public static string Configuration { get; } = Get<AssemblyConfigurationAttribute>().Configuration;
            public static string Company { get; } = Get<AssemblyCompanyAttribute>().Company;
            public static string Product { get; } = Get<AssemblyProductAttribute>().Product;
            public static string Copyright { get; } = Get<AssemblyCopyrightAttribute>().Copyright;
            public static string Trademark { get; } = Get<AssemblyTrademarkAttribute>().Trademark;
            public static Version Version { get; } = assembly.GetName().Version;

            private static T Get<T>() where T : class => GetCustomAttribute(assembly, typeof(T)) as T;
        }
    }
}
