using System;

namespace LagrangianDesign.LgTvControl.AssortedExtensions {
    public static class AssortedExtensionMethods {
        public static String Quoted(this String s)
            => s == null ? null : $"\"{s?.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }
}