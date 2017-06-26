using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Jupyter.PowerShell
{
    public static class ScalarHelper
    {
        public static bool IsScalar(Collection<string> typeNames)
        {
            if (typeNames.Count == 0)
            {
                return false;
            }

            // NOTE: we do not use inheritance here, since we deal with
            // value types or with types where inheritance is not a factor for the selection
            string typeName = typeNames[0];

            if (string.IsNullOrEmpty(typeName))
            {
                return false;
            }
            
            if (typeName.StartsWith("Deserialized.", StringComparison.OrdinalIgnoreCase))
            {
                typeName = typeName.Substring("Deserialized.".Length);
                if (string.IsNullOrEmpty(typeName))
                {
                    return false;
                }
            }

            // check if the type is derived from a System.Enum
            if (!(typeNames.Count < 2 || string.IsNullOrEmpty(typeNames[1])) && String.Equals(typeNames[1], "System.Enum", StringComparison.Ordinal))
            {
                return true;
            }

            return s_defaultScalarTypesHash.Contains(typeName);
        }

        static ScalarHelper()
        {
            s_defaultScalarTypesHash.Add("System.String");
            s_defaultScalarTypesHash.Add("System.SByte");
            s_defaultScalarTypesHash.Add("System.Byte");
            s_defaultScalarTypesHash.Add("System.Int16");
            s_defaultScalarTypesHash.Add("System.UInt16");
            s_defaultScalarTypesHash.Add("System.Int32");
            s_defaultScalarTypesHash.Add("System.UInt32");
            s_defaultScalarTypesHash.Add("System.Int64");
            s_defaultScalarTypesHash.Add("System.UInt64");
            s_defaultScalarTypesHash.Add("System.Char");
            s_defaultScalarTypesHash.Add("System.Single");
            s_defaultScalarTypesHash.Add("System.Double");
            s_defaultScalarTypesHash.Add("System.Boolean");
            s_defaultScalarTypesHash.Add("System.Decimal");
            s_defaultScalarTypesHash.Add("System.IntPtr");
            s_defaultScalarTypesHash.Add("System.Security.SecureString");
            s_defaultScalarTypesHash.Add("System.Numerics.BigInteger");
        }

        private static readonly HashSet<string> s_defaultScalarTypesHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
