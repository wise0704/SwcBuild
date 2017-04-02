using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SwcBuild
{
    using static FileAccess;
    using static FileMode;
    using static Platform;
    using static Regex;

    internal static class Utilities
    {
        public static bool AreFilesEqual(string file1, string file2)
        {
            if (file1.Equals(file2))
                return true;

            if (!File.Exists(file1) || !File.Exists(file2))
                return false;

            using (var stream1 = new FileStream(file1, Open, Read))
            using (var stream2 = new FileStream(file2, Open, Read))
            {
                if (stream1.Length != stream2.Length)
                    return false;

                int byte1;

                do
                {
                    byte1 = stream1.ReadByte();

                    if (byte1 != stream2.ReadByte())
                        return false;
                }
                while (!byte1.Equals(-1));
            }

            return true;
        }

        public static string FixOutputPath(string path, bool debug)
        {
            if (debug)
                return path.Replace("{Build}", "Debug");

            path = path.Replace("{Build}", "Release");
            return Replace(path, @"(\S)[-_.][Dd]ebug([.\\/])", "$1$2");
        }

        public static string GetConfigname(string platform)
        {
            switch (platform)
            {
                case AirDesktop: return "air";
                case AirMobile: return "airmobile";
                case FlashPlayer: return "flex";
                default: return "flex";
            }
        }

        public static string[] ParseArguments(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new string[0];

            var matches = Matches(data, @"(^|&#xA;)\s*(?<arg>[+\-][A-Za-z.\-]+\+?=(\S*|"".*""))");
            var args = new string[matches.Count];

            for (int i = 0; i < args.Length; i++)
                args[i] = matches[i].Groups["arg"].Value;

            return args;
        }

        public static string[] ParseConstants(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new string[0];

            var matches = Matches(data, @"(^|&#xA)\s*(?<const>(\S+::)?\S+,\S)");
            var constants = new string[matches.Count];

            for (int i = 0; i < constants.Length; i++)
                constants[i] = matches[i].Groups["const"].Value;

            return constants;
        }

        public static int ResolveSwfVersion(Version targetPlayer, bool air) =>
            air ?
            MapAirVersionToSwfVersion(targetPlayer.Major, targetPlayer.Minor)
            : MapFlashPlayerVersionToSwfVersion(targetPlayer.Major, targetPlayer.Minor);

        public static string WrapWithQuotes(string text) => text.Contains(" ") ? $"\"{text}\"" : text;

        private static int MapAirVersionToSwfVersion(int major, int minor)
        {
            const int majorIncremental = 13;
            const int majorIncrementalOffset = 11;
            const int majorFixed = 4;
            const int minorIncremental = 3;
            const int minorIncrementalOffset = 13;
            
            if (major >= majorIncremental)
                return major + majorIncrementalOffset;

            if (major.Equals(majorFixed))
                return 10 + minorIncrementalOffset;

            if (major.Equals(minorIncremental))
                return minor + minorIncrementalOffset;

            if (major.Equals(2))
            {
                if (minor > 6) return 12;
                if (minor.Equals(6)) return 11;
            }

            return 10;
        }

        private static int MapFlashPlayerVersionToSwfVersion(int major, int minor)
        {
            const int majorIncremental = 12;
            const int majorIncrementalOffset = 11;
            const int minorIncremental = 11;
            const int minorIncrementalOffset = 13;
            const int inconsistentSwfVersion = 10;
            
            if (major >= majorIncremental)
                return major + majorIncrementalOffset;

            if (major.Equals(minorIncremental))
                return minor + minorIncrementalOffset;

            if (major.Equals(inconsistentSwfVersion))
            {
                if (minor > 2) return 12;
                if (minor.Equals(2)) return 11;
                return 10;
            }

            return 9;
        }
    }
}
