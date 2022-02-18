using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using CustomIdentity.CustomStorageProvider.Attributes;
using ObjectRelationMapping.Enum;
using ObjectRelationMapping.Interfaces;
using ObjectRelationMapping.Mapping;
using ObjectRelationMapping.SqlCommandBuilder;

namespace ObjectRelationMapping.Repository
{
    public class Repository<T> : IRepository<T>
        where T : IEntityBase
    {
        private readonly SqlConnection context;

        private Validations validations = new();

        private SqlCommandBuilder<T> sqlCommandBuilder = new SqlCommandBuilder<T>();

        public Repository()
        {
        }

        public Repository(SqlConnection context)
        {
            this.context = context;
        }

        public int Add(T item)
        {
            SqlCommand query = new SqlCommand(this.sqlCommandBuilder.Insert(item), this.context);

            object number = query.ExecuteScalar();

            return (int)(decimal)number;
        }

        public int Delete(int id)
        {
            SqlCommand query = new(this.sqlCommandBuilder.Remove(id), this.context);

            int number = query.ExecuteNonQuery();

            return number;
        }

        public int Update(T item)
        {
            SqlCommand query = new(this.sqlCommandBuilder.Update(item), this.context);

            int number = query.ExecuteNonQuery();

            return number;
        }

        public T GetById(int id)
        {
            SqlCommand query = new(this.sqlCommandBuilder.FindById(id), this.context);

            SqlDataReader reader = query.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string typeInheritanceClass = " ";
                    object createdInstance = null;
                    object newObject = null;
                    int? indexOfDicriptor = reader.GetOrdinal("Discriptor");
                    if (indexOfDicriptor != null)
                    {
                        typeInheritanceClass = reader.GetValue((int)indexOfDicriptor).ToString();
                        createdInstance = FromDatabaseToEntityConverter<T>.MapDataToBusinessEntity(typeInheritanceClass);
                    }
                    else
                    {
                        createdInstance = FromDatabaseToEntityConverter<T>.MapDataToBusinessEntity();
                    }

                    newObject = FromDatabaseToEntityConverter<T>.FillObject(createdInstance, reader);
                    reader.Close();
                    return (T)newObject;
                }
            }

            reader.Close();
            try
            {
                return FromDatabaseToEntityConverter<T>.MapDataToBusinessEntity();
            }
            catch
            {
                throw new Exception("The object is not exist.");
            }
        }

        public T Include(T item, Type joinedType)
        {
            SqlCommand query = new SqlCommand(this.sqlCommandBuilder.Include(item, joinedType), this.context);

            object createdInstance = null;
            using (SqlDataReader includeReader = query.ExecuteReader())
            {
                if (includeReader.HasRows)
                {
                    while (includeReader.Read())
                    {
                        var mappedInstance = FromDatabaseToEntityConverter<T>.MapDataToBusinessEntity();
                        createdInstance = FromDatabaseToEntityConverter<T>.FillObject(mappedInstance, includeReader);
                    }
                }
            }

            try
            {
                return this.GetObject(createdInstance, joinedType);
            }
            catch
            {
                return FromDatabaseToEntityConverter<T>.MapDataToBusinessEntity();
            }
        }

        public override bool Equals(object obj)
        {
            bool isEqual = obj is Repository<T> repository &&
                   EqualityComparer<SqlConnection>.Default.Equals(this.context, repository.context);
            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.context);
        }

        private T GetObject(object createdInstance, Type joinedType)
        {
            if (createdInstance != null)
            {
                foreach (var propertyInfo in createdInstance.GetType().GetProperties())
                {
                    if (propertyInfo.PropertyType == joinedType
                        || joinedType.IsAssignableTo(propertyInfo.PropertyType)
                        || propertyInfo.PropertyType.GetElementType() == joinedType)
                    {
                        object generatedInstance = null;
                        object relatedObject = null;

                        if (joinedType.IsAssignableTo(propertyInfo.PropertyType))
                        {
                            generatedInstance = this.DynamicCreationRepository(joinedType, EntityType.IsAbstractOrInterface);
                        }
                        else
                        {
                            generatedInstance = this.DynamicCreationRepository(joinedType);
                        }

                        var method = generatedInstance.GetType().GetMethod("GetById");
                        var relatedEntityId = this.GetIdOfRelatedEntity(createdInstance, propertyInfo, joinedType);

                        relatedObject = method.Invoke(generatedInstance, new object[] { relatedEntityId });
                        foreach (var property in createdInstance.GetType().GetProperties())
                        {
                            if (this.validations.IsFieldExist(property, joinedType))
                            {
                                property.SetValue(createdInstance, relatedObject);
                            }
                        }
                    }
                }

                return (T)createdInstance;
            }
            else
            {
                throw new Exception("There is no instance to create.");
            }
        }

        private int? GetIdOfRelatedEntity(object mainInstance, PropertyInfo parentPropertyInfo, Type joinedType)
        {
            object relatedEntityId = null;

            foreach (var property in mainInstance.GetType().GetProperties())
            {
                FKRelationshipAttribute foreignKeyAttributeForProperty = this.validations.CurrentPropertyFkAttribute(property);

                if (foreignKeyAttributeForProperty != null)
                {
                    if ((parentPropertyInfo.PropertyType == joinedType || joinedType.IsAssignableTo(parentPropertyInfo.PropertyType)) &&
                            (joinedType == foreignKeyAttributeForProperty.ForeignKeyType ||
                            joinedType.BaseType == foreignKeyAttributeForProperty.ForeignKeyType))
                    {
                        relatedEntityId = property.GetValue(mainInstance);
                    }
                }
            }

            return (int?)relatedEntityId;
        }

        private object DynamicCreationRepository(Type repositoryType, EntityType entityType = EntityType.Regular)
        {
            Type constructedType;
            if (entityType == EntityType.IsAbstractOrInterface)
            {
                constructedType = typeof(Repository<>).MakeGenericType(repositoryType.BaseType);
            }
            else
            {
                constructedType = typeof(Repository<>).MakeGenericType(repositoryType);
            }

            object objectWithConstructor = Activator.CreateInstance(constructedType);
            ConstructorInfo ctorInstance = objectWithConstructor.GetType().GetConstructor(new[] { this.context.GetType() });
            object generatedInstance = ctorInstance.Invoke(new object[] { this.context });
            return generatedInstance;
        }
    }
}