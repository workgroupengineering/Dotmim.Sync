﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Dotmim.Sync.Manager
{
    public abstract class DbMetadata
    {
        /// <summary>
        /// Validate if a column definition is actualy supported by the provider
        /// </summary>
        public abstract bool IsValid(SyncColumn columnDefinition);

        /// <summary>
        /// Get the datastore type name from a DbType for generating scripts
        /// </summary>
        public abstract string GetStringFromDbType(DbType dbType, int maxLength);

        /// <summary>
        /// Get the datastore type name from a provider dbType for generating scripts
        /// </summary>
        public abstract string GetStringFromOwnerDbType(object ownerType);

        /// <summary>
        /// Get the datastore precision / scale / length string for generating scripts
        /// </summary>
        public abstract string GetPrecisionStringFromDbType(DbType dbType, int maxLength, byte precision, byte scale);

        /// <summary>
        /// Get the datastore precision / scale / length string for generating scripts
        /// </summary>
        public abstract string GetPrecisionStringFromOwnerDbType(object dbType, int maxLength, byte precision, byte scale);

        /// <summary>
        /// Get the datastore precision / scale / length for DbParameter
        /// </summary>
        public abstract (byte precision, byte scale) GetPrecisionFromOwnerDbType(object dbType, byte precision, byte scale);

        /// <summary>
        /// Get the datastore precision / scale for DbParameter
        /// </summary>
        public abstract (byte precision, byte scale) GetPrecisionFromDbType(DbType dbType, byte precision, byte scale);

        /// <summary>
        /// Get the datastore MaxLength for DbParameter
        /// </summary>
        public abstract Int32 GetMaxLengthFromDbType(DbType dbType, Int32 maxLength);

        /// <summary>
        /// Get the datastore maxLength for DbParameter
        /// </summary>
        public abstract Int32 GetMaxLengthFromOwnerDbType(object dbType, Int32 maxLength);

        /// <summary>
        /// Get a DbType from a datastore type name
        /// </summary>
        public abstract DbType ValidateDbType(string typeName, bool isUnsigned, bool isUnicode, long maxLength);

        /// <summary>
        /// Get a datastore DbType from a datastore type name
        /// </summary>
        public abstract object ValidateOwnerDbType(string typeName, bool isUnsigned, bool isUnicode, long maxLength);

        /// <summary>
        /// Gets and validate a max length issued from the database definition
        /// </summary>
        public abstract int ValidateMaxLength(string typeName, bool isUnsigned, bool isUnicode, long maxLength);

        /// <summary>
        /// Gets the corresponding DbType from a owner dbtype
        /// </summary>
        public abstract object GetOwnerDbTypeFromDbType(DbType dbType);

        /// <summary>
        /// Get a managed type from a datastore dbType
        /// </summary>
        public abstract Type ValidateType(object ownerType);

        /// <summary>
        /// Check if a type name is a numeric type
        /// </summary>
        public abstract bool IsNumericType(string typeName);

        /// <summary>
        /// Check if a type name is a text type
        /// </summary>
        public abstract bool IsTextType(string typeName);

        /// <summary>
        /// Check if a type name support scale
        /// </summary>
        public abstract bool SupportScale(string typeName);

        /// <summary>
        /// Get precision and scale from a SchemaColumn
        /// </summary>
        public abstract (byte precision, byte scale) ValidatePrecisionAndScale(SyncColumn columnDefinition);

        /// <summary>
        /// Get precision if supported (MySql supports int(10))
        /// </summary>
        public abstract byte ValidatePrecision(SyncColumn columnDefinition);


        /// <summary>
        /// Validate if a column is readonly or not
        /// </summary>
        /// <param name="columnDefinition"></param>
        /// <returns></returns>
        public abstract bool ValidateIsReadonly(SyncColumn columnDefinition);

        /// <summary>
        /// Returns the corresponding Owner DbType. Because it could be lower case, we should handle it
        /// </summary>
        public object TryGetOwnerDbType(string ownerDbType, DbType fallbackDbType, bool isUnsigned, bool isUnicode, long maxLength, string fromProviderType, string ownerProviderType)
        {
            // We MUST check if we are from the same provider (if it's mysql or oracle, we fallback on dbtype
            if (!string.IsNullOrEmpty(ownerDbType) && fromProviderType == ownerProviderType)
                return ValidateOwnerDbType(ownerDbType, isUnsigned, isUnicode, maxLength);

            // if it's not the same provider, fallback on DbType instead.
            return GetOwnerDbTypeFromDbType(fallbackDbType);
        }

        public string TryGetOwnerDbTypeString(string originalDbType, DbType fallbackDbType, bool isUnsigned, bool isUnicode, long maxLength, string fromProviderType, string ownerProviderType)
        {
            // We MUST check if we are from the same provider (if it's mysql or oracle, we fallback on dbtype
            if (!String.IsNullOrEmpty(originalDbType) && fromProviderType == ownerProviderType)
            {
                Object ownedDbType = ValidateOwnerDbType(originalDbType, isUnsigned, isUnicode, maxLength);
                return GetStringFromOwnerDbType(ownedDbType);
            }

            // if it's not the same provider, fallback on DbType instead.
            return GetStringFromDbType(fallbackDbType, Convert.ToInt32(maxLength));
        }

        public string TryGetOwnerDbTypePrecision(string originalDbType, DbType fallbackDbType, bool isUnsigned, bool isUnicode, int maxLength, byte precision, byte scale, string fromProviderType, string ownerProviderType)
        {
            // We MUST check if we are from the same provider (if it's mysql or oracle, we fallback on dbtype
            if (!String.IsNullOrEmpty(originalDbType) && fromProviderType == ownerProviderType)
            {
                object ownedDbType = ValidateOwnerDbType(originalDbType, isUnsigned, isUnicode, maxLength);
                return GetPrecisionStringFromOwnerDbType(ownedDbType, maxLength, precision, scale);
            }

            // if it's not the same provider, fallback on DbType instead.
            return GetPrecisionStringFromDbType(fallbackDbType, maxLength, precision, scale);
        }

        public (byte precision, byte scale) TryGetOwnerPrecisionAndScale(string originalDbType, DbType fallbackDbType, bool isUnsigned, bool isUnicode, long maxLength, byte precision, byte scale, string fromProviderType, string ownerProviderType)
        {
            // We MUST check if we are from the same provider (if it's mysql or oracle, we fallback on dbtype
            if (!String.IsNullOrEmpty(originalDbType) && fromProviderType == ownerProviderType)
            {
                Object ownedDbType = ValidateOwnerDbType(originalDbType, isUnsigned, isUnicode, maxLength);
                return GetPrecisionFromOwnerDbType(ownedDbType, precision, scale);
            }

            // if it's not the same provider, fallback on DbType instead.
            return GetPrecisionFromDbType(fallbackDbType, precision, scale);

        }

        public int TryGetOwnerMaxLength(string originalDbType, DbType fallbackDbType, bool isUnsigned, bool isUnicode, int maxLength, string fromProviderType, string ownerProviderType)
        {
            // We MUST check if we are from the same provider (if it's mysql or oracle, we fallback on dbtype
            if (!String.IsNullOrEmpty(originalDbType) && fromProviderType == ownerProviderType)
            {
                Object ownedDbType = ValidateOwnerDbType(originalDbType, isUnsigned, isUnicode, maxLength);
                return GetMaxLengthFromOwnerDbType(ownedDbType, maxLength);
            }

            // if it's not the same provider, fallback on DbType instead.
            return GetMaxLengthFromDbType(fallbackDbType, maxLength);

        }


    }
}
