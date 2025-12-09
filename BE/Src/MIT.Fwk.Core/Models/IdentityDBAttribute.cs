using System;

namespace MIT.Fwk.Core.Models
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]

    public class IdentityDBAttribute : Attribute
    {
        public IdentityDBAttribute() { }
    }
}
