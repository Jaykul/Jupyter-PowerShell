
namespace Jupyter.PowerShell.Commands
{
    using Jupyter.Messages;
    using Jupyter.Server;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;

    [Cmdlet(VerbsCommunications.Write, "Jupyter", DefaultParameterSetName = "Automatic")]
    public class WriteJupyterCommand : PSCmdlet
    {
        // MIME Types implemented in the base Jupyter notebook
        readonly IReadOnlyDictionary<string, string> types = new Dictionary<string, string>()
        {
            {"text", "text/plain"},
            {"html", "text/html"},
            {"markdown","text/markdown"},
            {"latex","text/latex"},
            {"json","application/json"},
            {"javascript","application/javascript"},
            {"png","image/png"},
            {"jpeg","image/jpeg"},
            {"svg","image/svg+xml"}
        };

        [Parameter(ValueFromPipeline =true, Mandatory = true)]
        public PSObject InputObject { get; set; }

        [Parameter(ParameterSetName = "IdDisplay")]
        public string Id { get; set; }

        [Parameter(ParameterSetName = "UpdateDisplay")]
        public string Update { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var data = new Dictionary<string,object>();
            var isJupyterData = false;
            foreach(var property in  InputObject.Properties)
            {
                var name = property.Name.ToLower();
                if (types.Keys.Contains(name))
                {
                    isJupyterData = true;
                    data.Add(types[name], property.Value);
                }
                else if(name.Contains('/'))
                {
                    isJupyterData = true;
                    data.Add(name, property.Value);
                }                
            }

            if(!isJupyterData)
            {
                if (InputObject.BaseObject is IDictionary dictionary)
                {
                    foreach (var property in dictionary.Keys)
                    {
                        var name = property.ToString().ToLower();
                        if (types.Keys.Contains(name))
                        {
                            isJupyterData = true;
                            data.Add(types[name], dictionary[property]);
                        }
                        else if (name.Contains('/'))
                        {
                            isJupyterData = true;
                            data.Add(name, dictionary[property]);
                        }
                    }
                }
            }

            if (!isJupyterData)
            {
                if (InputObject.BaseObject is string)
                {
                    data.Add("text/plain", InputObject.BaseObject);
                }
                else
                {
                    data.Add("application/json", InputObject.BaseObject);
                }
            }
            var content = new DisplayDataContent(data);

            var type = MessageType.DisplayData;
            
            if (!string.IsNullOrEmpty(Id))
            {
                content.Transient.Add("display_id", Update);
            }
            if (!string.IsNullOrEmpty(Update))
            {
                type = MessageType.UpdateDisplayData;
                content.Transient.Add("display_id", Update);
            }

            var session = SessionState.PSVariable.GetValue("JupyterSession") as Session;

            Message message = new Message(type, content, new Header(type, null));

            session.PublishSocket.SendMessage(message);
        }
    }
}
