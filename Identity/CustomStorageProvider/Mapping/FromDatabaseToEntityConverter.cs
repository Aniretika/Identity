using System;
using System.Data;
using System.Reflection;
using CustomIdentity.CustomStorageProvider.Attributes;

namespace ObjectRelationMapping.Mapping
{
    public static class FromDatabaseToEntityConverter<T>
    {
        public static T MapDataToBusinessEntity(string typeInheritanceClass = " ")
        {
            Type businessEntityType = typeof(T);
            string assemblyName = businessEntityType.Assembly.GetName().Name;
            string fullNameInheritanceClass = $"{businessEntityType.Namespace}.{typeInheritanceClass}";
            T newObject;

            if (businessEntityType.IsAbstract)
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, fullNameInheritanceClass);
                newObject = (T)newGeneratedObjectReference.Unwrap();
            }
            else
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, businessEntityType.FullName.ToString());
                newObject = (T)newGeneratedObjectReference.Unwrap();
            }

            return newObject;
        }

        public static T FillObject(object newObject, IDataReader dr)
        {
            PropertyInfo[] properties = newObject.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string dbFieldName = ConvertFiledToDatabaseLayer(property);
                if (dbFieldName == null)
                {
                    continue;
                }
                else
                {
                    var propertyValue = dr[dbFieldName];
                    if (propertyValue != null)
                    {
                        property.SetValue(newObject, propertyValue);
                    }
                }
            }

            return (T)newObject;
        }

        public static string ConvertFiledToDatabaseLayer(PropertyInfo property)
        {
            if ((property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute) != null)
            {
                var pkAtrtribute = property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute;

                return pkAtrtribute.ColumnTitle;
            }
            else if ((property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute) != null)
            {
                var fkAtrtribute = property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;

                return fkAtrtribute.ColumnTitle;
            }
            else if ((property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition) != null)
            {
                var columnAtrtribute = property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition;

                return columnAtrtribute.ColumnTitle;
            }

            return null;
        }
    }
}