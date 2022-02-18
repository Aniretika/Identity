using System;

namespace CustomIdentity.CustomStorageProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DefinitionRequiredAttribute : Attribute
    {
        public string ErrorMessage { get; set; }
    }
}