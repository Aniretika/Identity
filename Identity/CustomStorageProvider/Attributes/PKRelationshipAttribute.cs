using System;

namespace CustomIdentity.CustomStorageProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PKRelationshipAttribute : Attribute
    {
        public string ColumnTitle { get; set; }
    }
}