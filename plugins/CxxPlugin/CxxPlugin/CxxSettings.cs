namespace CxxPlugin
{
    /// <summary>
    /// cxx settings
    /// </summary>
    public class CxxSettings
    {
        public string CxxVersion { get; set; } = string.Empty;

        public string VeraEnvironment { get; set; } = string.Empty;

        public string VeraExecutable { get; set; } = string.Empty;

        public string VeraArguments { get; set; } = string.Empty;

        public string RatsEnvironment { get; set; } = string.Empty;

        public string RatsExecutable { get; set; } = string.Empty;

        public string RatsArguments { get; set; } = string.Empty;

        public string PcLintEnvironment { get; set; } = string.Empty;

        public string PcLintExecutable { get; set; } = string.Empty;

        public string PcLintArguments { get; set; } = string.Empty;

        public string LinterProp { get; set; } = string.Empty;

        public string CustomEnvironment { get; set; } = string.Empty;

        public string CustomExecutable { get; set; } = string.Empty;

        public string CustomArguments { get; set; } = string.Empty;

        public string CppCheckEnvironment { get; set; } = string.Empty;

        public string CppCheckExecutable { get; set; } = string.Empty;

        public string CppCheckArguments { get; set; } = string.Empty;

        public string ClangExecutable { get; set; } = string.Empty;

        public string CustomSensorKey { get; set; } = string.Empty;
    }
}
