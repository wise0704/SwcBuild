namespace SwcBuild
{
    internal enum ExitCodes : byte
    {
        NoError,
        MissingArgument,
        ValueExpected,
        ExpectedArgument,
        UnexpectedArgument,
        EmptyValue,
        IncorrectArgumentValue,
        RepeatedArgument,
        InvalidPath,
        MissingSdkDescription,
        InvalidFormat,
        ErrorWritingConfig,
        ErrorRunningCompiler,
        ErrorRunningAsDoc,
        Unknown = 255,
    }
}
