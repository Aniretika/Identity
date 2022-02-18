using System;

namespace CustomIdentity.CustomStorageProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FKRelationshipAttribute : Attribute
    {
        public FKRelationshipAttribute(Type type)
        {
            this.ForeignKeyType = type;
        }

        public string ColumnTitle { get; set; }

        public Type ForeignKeyType { get; set; }
    }
}