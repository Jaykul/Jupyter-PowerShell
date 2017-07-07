using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jupyter.PowerShell
{
    public static class SessionHelper
    {
        public  static void LoadCmdlets(this InitialSessionState iss, Assembly core)
        {
            // Load all the Cmdlets that are in this assembly automatically.
            foreach (var t in core.GetTypes())
            {
                if (t.GetCustomAttributes(typeof(CmdletAttribute), false) is CmdletAttribute[] cmdlets)
                {
                    foreach (var cmdlet in cmdlets)
                    {
                        iss.Commands.Add(new SessionStateCmdletEntry($"{cmdlet.VerbName}-{cmdlet.NounName}", t, $"{t.Name}.xml"));
                    }
                }
            }
        }
    }
}
