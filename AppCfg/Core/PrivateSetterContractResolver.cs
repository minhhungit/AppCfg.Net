﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace AppCfg
{
    /// <summary>
    /// This contract resolver will serialize and deserialize:
    /// <para>
    /// - public and nonpublic properties
    /// </para>
    /// <para>
    /// - getter-only auto-properties
    /// </para>
    /// </summary>
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        // https://github.com/TheInsaneBro/PrivateSetterContractResolver

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);
            if (!(member is PropertyInfo property) || jProperty.IsModifiable())
            {
                // Not a property or writeable and readable by default, use default behaviour
                return jProperty;
            }

            // Test for property with private setter
            jProperty.Readable = property.CanRead;
            jProperty.Writable = property.CanWrite;
            if (jProperty.IsModifiable())
            {
                return jProperty;
            }

            // Getter-only OR setter-only Property. 
            // Getter-only property. Try accessing the auto generated backing field.
            // Todo: make more generic. Would not work in VB.Net, F# or when the compiler is changed
            var backingField = property.DeclaringType.GetField($"<{property.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

            // Test if backing field is there or if it is compiler generated
            if (backingField?.IsDefined(typeof(CompilerGeneratedAttribute), true) == true)
            {
                // Backing field okay, return the backing field serialization info
                return FromBackingField(jProperty, property, backingField);
            }

            // No autogenerated backing field found. Probably a setter-only property.
            // Return as not supported
            return jProperty;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            // Json.Net handles dictionary and enumerable types different so that it needs initialization
            if (!objectType.IsDictionaryType() && !typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                // Skip all initialization
                contract.DefaultCreator = () => FormatterServices.GetUninitializedObject(objectType);
            }

            return contract;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return objectType.GetAllPropertiesRecursive().Select(p => p as MemberInfo).ToList();
        }

        private static JsonProperty FromBackingField(JsonProperty originalProperty, PropertyInfo property, FieldInfo field)
        {
            return new JsonProperty()
            {
                DeclaringType = property.DeclaringType,
                PropertyType = property.PropertyType,
                PropertyName = originalProperty.PropertyName,
                ValueProvider = new ReflectionValueProvider(field),
                Readable = true,
                Writable = true,
            };
        }
    }
}
