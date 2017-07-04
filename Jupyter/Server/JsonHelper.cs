using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Jupyter.Server
{
    public class JsonHelper
    {

        private class CustomJsonTextWriter : JsonTextWriter
        {
            public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter) { }

            public int CurrentDepth { get; private set; }

            public override void WriteStartObject()
            {
                CurrentDepth++;
                base.WriteStartObject();
            }

            public override void WriteEndObject()
            {
                CurrentDepth--;
                base.WriteEndObject();
            }
        }

        private class CustomContractResolver : DefaultContractResolver
        {
            private readonly Func<bool> _includeProperty;

            public CustomContractResolver(Func<bool> includeProperty)
            {
                _includeProperty = includeProperty;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                var shouldSerialize = property.ShouldSerialize;
                property.ShouldSerialize = obj => _includeProperty() && (shouldSerialize == null || shouldSerialize(obj));
                return property;
            }
        }

        public static string SerializeObject(object obj, int maxDepth)
        {
            using (var strWriter = new StringWriter())
            {
                using (var jsonWriter = new CustomJsonTextWriter(strWriter))
                {
                    Func<bool> include = () => jsonWriter.CurrentDepth <= maxDepth;
                    var resolver = new CustomContractResolver(include);
                    var serializer = new JsonSerializer { ContractResolver = resolver };
                    serializer.Serialize(jsonWriter, obj);
                }
                return strWriter.ToString();
            }
        }
    }
}
