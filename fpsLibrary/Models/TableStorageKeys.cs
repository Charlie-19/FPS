using System;
using System.Collections.Generic;
using System.Text;

namespace fpsLibrary.Models
{
    public abstract class TableStorageKeys
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public static void CleanKey(StringBuilder keyBuilder)
        {
            // Strip control code characters.
            // Replace invalid characters, but they need to stay unique since they are part of the key.
            StringBuilder tmpStringBuilder = new StringBuilder(keyBuilder.ToString());
            char charToCheck;

            keyBuilder.Clear();

            for (int i = 0; i < tmpStringBuilder.Length; i++)
            {
                charToCheck = tmpStringBuilder[i];
                if (!Char.IsControl(charToCheck))
                {
                    switch (charToCheck)
                    {
                        case '/':
                            keyBuilder.Append("!F");
                            break;
                        case '#':
                            keyBuilder.Append("!H");
                            break;
                        case '?':
                            keyBuilder.Append("!Q");
                            break;
                        case '%':
                            keyBuilder.Append("!P");
                            break;
                        case '\\':
                            keyBuilder.Append("!B");
                            break;
                        default:
                            keyBuilder.Append(charToCheck);
                            break;
                    }
                }
            }
        }
    }
}
