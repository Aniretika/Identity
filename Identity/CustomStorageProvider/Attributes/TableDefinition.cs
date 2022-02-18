using System;

namespace CustomIdentity.CustomStorageProvider.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
    public class TableDefinition : Attribute
    {
        public string ColumnTitle { get; set; }
    }
}