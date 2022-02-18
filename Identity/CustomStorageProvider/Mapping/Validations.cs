using System;
using System.Reflection;
using CustomIdentity.CustomStorageProvider.Attributes;

namespace ObjectRelationMapping.Mapping
{
    public class Validations
    {
        public FKRelationshipAttribute CurrentPropertyFkAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;
        }

        public PKRelationshipAttribute CurrentPropertyPkAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute;
        }

        public ColumnDefinition CurrentPropertyColumnAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition;
        }

        public bool IsFieldExist(PropertyInfo propertyInfo, Type joinedType)
        {
            return propertyInfo.PropertyType == joinedType || propertyInfo.PropertyType == joinedType.BaseType;
        }
    }
}