using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSonarPlugins
{
    public enum ReturnCode
    {
        Ok,
        Timeout,
        NokAppSpecific
    }

    public interface IVSSonarQubeCmdExecutor
    {
        List<string> GetStdOut();
        List<string> GetStdError();
        ReturnCode GetErrorCode();
        ReturnCode CancelExecution();
        ReturnCode CancelExecutionAndSpanProcess(string[] names);

        void ResetData();

        // no redirection of output
        int ExecuteCommand(string program, string args, Dictionary<string, string> env, string workDir);
        int ExecuteCommandWait(string program, string args, Dictionary<string, string> env, string workDir);

        // with redirection of output
        List<string> ExecuteCommand(string program, string args);
    }
}
