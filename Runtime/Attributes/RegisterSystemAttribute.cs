using System;

namespace Xeno {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RegisterSystemAttribute : Attribute {
        public RegisterSystemAttribute(Type type, int order = 0, bool bakeQuery = false) {
            BakeQuery = bakeQuery;
        }

        public bool BakeQuery { get; set; }
    }
}
