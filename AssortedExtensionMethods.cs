using System;
using System.Text;

namespace LagrangianDesign.LgTvControl.AssortedExtensions {
    public static class AssortedExtensionMethods {
        public static String Quoted(this String s) {
            if (s is null) {
                return null;
            }
            var output = new StringBuilder(s.Length + 2);
            output.Append('"');
            foreach (var c in s) {
                switch (c) {
                    case '"':
                        output.Append("\\\"");
                        break;
                    case '\\':
                        output.Append("\\\\");
                        break;
                    case '\b':
                        output.Append("\\b");
                        break;
                    case '\f':
                        output.Append("\\f");
                        break;
                    case '\n':
                        output.Append("\\n");
                        break;
                    case '\r':
                        output.Append("\\r");
                        break;
                    case '\t':
                        output.Append("\\t");
                        break;
                    default:
                        if (c <= 0x1f) {
                            output.AppendFormat("\\u{0:x4}", (Int16)c);
                        } else {
                            output.Append(c);
                        }
                        break;
                }
            }
            output.Append('"');
            return output.ToString();
        }
    }
}
