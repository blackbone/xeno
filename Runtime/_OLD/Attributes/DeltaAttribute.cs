using System;

namespace Xeno {
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DeltaAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class UseAttribute : Attribute {
        public string FieldName { get; }
        public UseAttribute(string fieldName) {
            FieldName = fieldName;
        }
    }
}