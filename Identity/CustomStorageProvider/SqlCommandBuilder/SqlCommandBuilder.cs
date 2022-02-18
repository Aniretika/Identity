using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomIdentity.CustomStorageProvider.Attributes;
using ObjectRelationMapping.Interfaces;
using ObjectRelationMapping.Mapping;

namespace ObjectRelationMapping.SqlCommandBuilder
{
    public class SqlCommandBuilder<T>
        where T : IEntityBase
    {
        private readonly DataSourceTransormation<T> dataSource = new();

        private Validations validations = new();

        public string Insert(object item) =>
            this.InsertQueryPreparer(item);

        public string Update(object item) =>
            this.UpdateQueryPreparer(item);

        public string Include(T item, Type joinedType)
        {
            if (this.JoinRelationChecker(item, joinedType))
            {
                string stringQuery =
                   $"SELECT * FROM {this.dataSource.GetTableName()}" +
                   $" WHERE {this.dataSource.GetPkField()} = {this.dataSource.GetObjectId(item)} ";
                return stringQuery;
            }
            else
            {
                throw new Exception("Join relation cannot executed, becase of lack of relation keys.");
            }
        }

        public string FindById(int id)
        {
            string stringQuery = $"SELECT * FROM {this.dataSource.GetTableName()} WHERE {this.dataSource.GetPkField()} = {id}";

            return stringQuery;
        }

        public string Remove(int id)
        {
            string stringQuery = $"DELETE FROM {this.dataSource.GetTableName()} WHERE {this.dataSource.GetPkField()} = {id}";

            return stringQuery;
        }

        private string InsertQueryPreparer(object objectForInserting)
        {
            StringBuilder stringQuery = new StringBuilder();

            stringQuery.Append(this.InsertWrapper(objectForInserting));
            stringQuery.Append(" SELECT SCOPE_IDENTITY() ");

            return stringQuery.ToString();
        }

        private bool JoinRelationChecker(T item, Type joinedType)
        {
            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                if (this.validations.IsFieldExist(propertyInfo, joinedType))
                {
                    return true;
                }
            }

            return false;
        }

        private string UpdateQueryPreparer(object objectForUpdating)
        {
            StringBuilder stringQuery = new StringBuilder();

            stringQuery.Append(this.UpdateWrapper(objectForUpdating));

            return stringQuery.ToString();
        }

        private string InsertWrapper(object insertingInstance)
        {
            var queryPreparer = this.dataSource.GetDataForInsertQuery(insertingInstance);

            string stringQuery = $"INSERT INTO {this.dataSource.GetTableName(insertingInstance.GetType())} ({string.Join(", ", queryPreparer.Keys)}) " +
    $"VALUES ({string.Join(", ", queryPreparer.Values)}) ";
            return stringQuery;
        }

        private string UpdateWrapper(object updatingInstance)
        {
            var queryPreparer = this.dataSource.GetDataForInsertQuery(updatingInstance);

            string updateSetContainer = string.Join(", ", queryPreparer.Zip(queryPreparer, (tableField, tableData) => tableField.Key + " = " + tableData.Value));
            var instanceType = updatingInstance.GetType();

            string stringQuery = $"UPDATE {this.dataSource.GetTableName(instanceType)} " +
                $"SET {updateSetContainer} " +
                $"WHERE {this.dataSource.GetPkField(instanceType)} = {this.dataSource.GetObjectId(updatingInstance)} ";
            stringQuery += this.UpdateQueryDependedEntity(updatingInstance);

            return stringQuery;
        }

        private string UpdateQueryDependedEntity(object objectInstance)
        {
            object relatedObject;
            string stringQuery = " ";
            foreach (var propertyMainObjectInfo in objectInstance.GetType().GetProperties())
            {
                Type oneToManyCaseTypeObject = propertyMainObjectInfo.PropertyType.GetElementType();
                if (oneToManyCaseTypeObject != null && oneToManyCaseTypeObject.CustomAttributes.Any(attr => attr.AttributeType == typeof(TableDefinition)))
                {
                    relatedObject = propertyMainObjectInfo.GetValue(objectInstance);
                    stringQuery += this.OneToManyFkQueryData(objectInstance, relatedObject);
                }
                else if (propertyMainObjectInfo.PropertyType.CustomAttributes.Any(attr => attr.AttributeType == typeof(TableDefinition)) && !propertyMainObjectInfo.GetIndexParameters().Any())
                {
                    relatedObject = propertyMainObjectInfo.GetValue(objectInstance);
                    stringQuery += this.OneToOneFkQueryData(objectInstance, relatedObject);
                }
            }

            return stringQuery;
        }

        private string OneToOneFkQueryData(object principalEntity, object dependedEntity)
        {
            StringBuilder stringQuery = new StringBuilder();
            string fkColumnTitle = this.dataSource.GetFkColumnTitleDependedEntity(principalEntity, dependedEntity);
            if (fkColumnTitle == null)
            {
                fkColumnTitle = this.dataSource.GetFkColumnTitleDependedEntity(dependedEntity, principalEntity);
                return this.OneToOneFkQueryData(dependedEntity, principalEntity);
            }

            var pkPrincipalEntity = this.dataSource.GetPrimaryKeyValue(principalEntity);
            string tableName = this.dataSource.GetTableName(dependedEntity.GetType());

            Dictionary<string, object> queryPreparer = this.dataSource.GetDataForInsertQuery(dependedEntity);
            queryPreparer.Add(fkColumnTitle, pkPrincipalEntity);

            string setQueryContainer = string.Join(", ", queryPreparer.Zip(queryPreparer, (tableField, tableData) => tableField.Key + " = " + tableData.Value));

            stringQuery.Append($"UPDATE {tableName} " +
            $"SET {setQueryContainer} " +
            $"WHERE {this.dataSource.GetPkField(dependedEntity.GetType())} = {this.dataSource.GetObjectId(dependedEntity)} ");
            return stringQuery.ToString();
        }

        private string OneToManyFkQueryData(object principalEntity, object dependedEntity)
        {
            StringBuilder stringQuery = new();
            if (dependedEntity.GetType().IsArray || dependedEntity.GetType() is IEnumerable)
            {
                var array = dependedEntity as IEnumerable;

                foreach (var arrayItem in array)
                {
                    string fkColumnTitle = this.dataSource.GetFkColumnTitleDependedEntity(principalEntity, arrayItem);
                    var pkPrincipalEntity = this.dataSource.GetPrimaryKeyValue(principalEntity);
                    string tableName = this.dataSource.GetTableName(arrayItem.GetType());

                    Dictionary<string, object> queryPreparer = this.dataSource.GetDataForInsertQuery(arrayItem);
                    queryPreparer.Add(fkColumnTitle, pkPrincipalEntity);

                    string setQueryContainer = string.Join(", ", queryPreparer.Zip(queryPreparer, (tableField, tableData) => tableField.Key + " = " + tableData.Value));

                    stringQuery.Append($"UPDATE {tableName} " +
                    $"SET {setQueryContainer} " +
                    $"WHERE {this.dataSource.GetPkField(arrayItem.GetType())} = {this.dataSource.GetObjectId(arrayItem)} ");
                }
            }

            return stringQuery.ToString();
        }
    }
}