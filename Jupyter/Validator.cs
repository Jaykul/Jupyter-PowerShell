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
            _logger = logger;
            _encoder = new UTF8Encoding();
            algorithm = algorithm.Replace("-", "").ToUpperInvariant();
            _logger.LogDebug(algorithm + ": '" + key + "'");
            try
            {
                switch(algorithm)
                {
                    case "HMACSHA256":
                        _signatureGenerator = new HMACSHA256();
                        break;

                    default:
                        _signatureGenerator = HMAC.Create(algorithm);
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
        public string CreateSignature(params string[] messages)
        {
            byte[] sourceBytes;
            _signatureGenerator.Initialize();
            // For all items update the signature
            var last = messages.Length - 1;
            for (var i = 0; i < last; i++)
            {
                sourceBytes = this._encoder.GetBytes(messages[i]);
                _signatureGenerator.TransformBlock(sourceBytes, 0, sourceBytes.Length, null, 0);
            }

            sourceBytes = _encoder.GetBytes(messages[last]);
            _signatureGenerator.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length);

            // Calculate the digest and remove -
            return BitConverter.ToString(_signatureGenerator.Hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Determines whether this instance is valid signature the specified message.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="message">Message.</param>
        public bool IsValidSignature(string hash, params string[] messages)
        {
            string calculatedSignature = this.CreateSignature(messages);
            this._logger.LogInformation(string.Format("Expected Signature: {0}", calculatedSignature));
            return string.Equals(hash, calculatedSignature, StringComparison.OrdinalIgnoreCase);
        }
    }
}

