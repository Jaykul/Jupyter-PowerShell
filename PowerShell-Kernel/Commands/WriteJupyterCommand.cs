
namespace Jupyter.PowerShell.Commands
{
    using Jupyter.Messages;
    using Jupyter.Server;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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
        [ValidateNotNullOrEmpty()]
        public PSObject InputObject { get; set; }

        [Parameter(ParameterSetName = "IdDisplay")]
        public string Id { get; set; }

        [Parameter(ParameterSetName = "UpdateDisplay")]
        public string Update { get; set; }

        [Parameter()]
        public Hashtable Metadata { get; set; }

        private object Normalize(object value, string mimetype)
        {
            if(value is PSObject)
            {
                value = ((PSObject)value).BaseObject;
            }

            if (!(value is string))
            {

                if (value is IEnumerable<string>)
                {
                    value = string.Join("\r\n", value);
                }
                else if (value is IEnumerable<object> && ((IEnumerable<object>)value).All(o => o is string || o is PSObject && ((PSObject)o).BaseObject is string))
                {
                    value = string.Join("\r\n", ((IEnumerable<object>)value).Select(o => o.ToString()));
                }
            }

            if (mimetype.ToLowerInvariant().EndsWith("json"))
            {
                try
                {
                    return JToken.Parse(value.ToString());
                }
                catch
                {
                    return value;
                }
            }
            return value;
        }

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
                    data.Add(types[name], Normalize(property.Value, types[name]));
                }
                else if(name.Contains('/'))
                {
                    isJupyterData = true;
                    data.Add(name, Normalize(property.Value, name));
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
                            data.Add(types[name], Normalize(dictionary[property], types[name]));
                        }
                        else if (name.Contains('/'))
                        {
                            isJupyterData = true;
                            data.Add(name, Normalize(dictionary[property], name));
                        }
                    }
                }
            }

            if (!isJupyterData)
            {
                if (InputObject.BaseObject is string)
                {
                    data.Add("text/plain", Normalize(InputObject.BaseObject, "text/plain"));
                }
                else
                {
                    data.Add("application/json", Normalize(InputObject.BaseObject, "application/json"));
                }
            }
            var content = new DisplayDataContent(data);

            if (Metadata != null)
            {
                foreach (var key in Metadata.Keys)
                {
                    content.MetaData.Add(key.ToString(), Metadata[key]);
                }
            }

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
