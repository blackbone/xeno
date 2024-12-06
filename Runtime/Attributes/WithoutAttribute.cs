using System;

namespace Xeno
{
    [AttributeUsage(AttributeTargets.Method)]
    public class WithoutAttribute : Attribute
    {
        public WithoutAttribute(params uint[] indices) {

        }
    }
}
