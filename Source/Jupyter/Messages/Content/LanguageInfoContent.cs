using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jupyter.Messages
{
    public class LanguageInfoContent : Content
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("mimetype")]
        public string MimeType { get; set; }

        private string _extension;
        /// <summary>
        /// Extension including the dot, e.g. '.py'
        /// </summary>
        [JsonProperty("file_extension")]
        public string FileExtension {
            get {
                return _extension;
            }
            set {
                if (!value.StartsWith("."))
                {
                    _extension = "." + value;
                }
                else
                {
                    _extension = value;
                }
            }
        }

        [JsonProperty("pygments_lexer")]
        public string PygmentsLexer { get; set; }

        [JsonProperty("codemirror_mode")]
        public string CodemirrorMode { get; set; }

        [JsonProperty("nbconvert_exporter")]
        public string NbConvertExporter { get; set; }
    }
}
