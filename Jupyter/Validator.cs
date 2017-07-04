namespace Jupyter
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Jupyter.Messages;
    using Newtonsoft.Json;

    public class Validator
    {
        private readonly ILogger _logger;

        private readonly HMAC _signatureGenerator;

        private readonly Encoding _encoder;

        /// <summary>
        /// Initializes a new instance of the <see cref="iCSharp.Kernel.Helpers.Sha256SignatureValidator"/> class.
        /// </summary>
        /// <param name="key">Shared key used to initialize the digest.</param>
        public Validator(ILogger logger, string key, string algorithm)
        {
            this._logger = logger;
            this._encoder = new UTF8Encoding();
            algorithm = algorithm.Replace("-", "").ToUpperInvariant();
            _logger.LogDebug(algorithm + ": '" + key + "'");
            try
            {
                switch(algorithm)
                {
                    case "HMACSHA256":
                        this._signatureGenerator = new HMACSHA256();
                        break;

                    default:
                        this._signatureGenerator = HMAC.Create(algorithm);
                        break;
                }
            }
            catch(Exception ex)
            {
                throw new ArgumentException("Failed to create an algorithm for " + algorithm, "algorithm", ex);
            }
            this._signatureGenerator.Key = this._encoder.GetBytes(key);
        }

        /// <summary>
        /// Creates the signature.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <param name="message">Message.</param>
        public string CreateSignature(Message message)
        {
            this._signatureGenerator.Initialize();

            List<string> messages = this.GetDigestMessages(message);

            // For all items update the signature
            foreach (string item in messages)
            {
                byte[] sourceBytes = this._encoder.GetBytes(item);
                _signatureGenerator.TransformBlock(sourceBytes, 0, sourceBytes.Length, null, 0);
            }

            _signatureGenerator.TransformFinalBlock(new byte[0], 0, 0);

            // Calculate the digest and remove -
            return BitConverter.ToString(_signatureGenerator.Hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Determines whether this instance is valid signature the specified message.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="message">Message.</param>
        public bool IsValidSignature(Message message)
        {
            string calculatedSignature = this.CreateSignature(message);
            this._logger.LogInformation(string.Format("Expected Signature: {0}", calculatedSignature));
            return string.Equals(message.HMac, calculatedSignature, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the text used as the digest for the HMAC signature.
        /// </summary>
        /// <returns>The messages to add for digest.</returns>
        /// <param name="message">Message.</param>
        private List<string> GetDigestMessages(Message message)
        {
            if (message == null)
            {
                return new List<string>();
            }

            return new List<string>(){
                JsonConvert.SerializeObject(message.Header),
                JsonConvert.SerializeObject(message.ParentHeader),
                JsonConvert.SerializeObject(message.MetaData),
                JsonConvert.SerializeObject(message.Content, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore})
            }; 
        }
    }
}

