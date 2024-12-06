using System;

namespace Xeno {
    [AttributeUsage(AttributeTargets.Parameter)]
    public class UniformAttribute : Attribute {
        public UniformAttribute(bool isDelta) { }
        public UniformAttribute(string name) { }
    }
}
