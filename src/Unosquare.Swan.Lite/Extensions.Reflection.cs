﻿namespace Unosquare.Swan
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using Attributes;

    /// <summary>
    /// Provides various extension methods for Reflection and Types.
    /// </summary>
    public static class ReflectionExtensions
    {
        private static readonly Lazy<Dictionary<PropertyInfo, MethodInfo>> CacheGetMethods =
            new Lazy<Dictionary<PropertyInfo, MethodInfo>>(() => new Dictionary<PropertyInfo, MethodInfo>());

        private static readonly Lazy<Dictionary<PropertyInfo, MethodInfo>> CacheSetMethods =
            new Lazy<Dictionary<PropertyInfo, MethodInfo>>(() => new Dictionary<PropertyInfo, MethodInfo>());

        #region Assembly Extensions

        /// <summary>
        /// Gets all types within an assembly in a safe manner.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// Array of Type objects representing the types specified by an assembly.
        /// </returns>
        /// <exception cref="ArgumentNullException">assembly.</exception>
        public static Type[] GetAllTypes(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Type[] assemblyTypes;

            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (System.IO.FileNotFoundException)
            {
                assemblyTypes = new Type[] { };
            }
            catch (NotSupportedException)
            {
                assemblyTypes = new Type[] { };
            }
            catch (ReflectionTypeLoadException e)
            {
                assemblyTypes = e.Types.Where(t => t != null).ToArray();
            }

            return assemblyTypes;
        }

        #endregion

        #region Type Extensions

        /// <summary>
        /// The closest programmatic equivalent of default(T).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Default value of this type.
        /// </returns>
        /// <exception cref="ArgumentNullException">type.</exception>
        public static object GetDefault(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsValueType() ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Determines whether this type is compatible with ICollection.
        /// </summary>
        /// <param name="sourceType">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified source type is collection; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">sourceType.</exception>
        public static bool IsCollection(this Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            return sourceType != typeof(string) &&
                   typeof(IEnumerable).IsAssignableFrom(sourceType);
        }

        /// <summary>
        /// Gets a method from a type given the method name, binding flags, generic types and parameter types.
        /// </summary>
        /// <param name="type">Type of the source.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="genericTypes">The generic types.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>
        /// An object that represents the method with the specified name.
        /// </returns>
        /// <exception cref="System.Reflection.AmbiguousMatchException">
        /// The exception that is thrown when binding to a member results in more than one member matching the 
        /// binding criteria. This class cannot be inherited.
        /// </exception>
        public static MethodInfo GetMethod(
            this Type type,
            BindingFlags bindingFlags,
            string methodName,
            Type[] genericTypes,
            Type[] parameterTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            if (genericTypes == null)
                throw new ArgumentNullException(nameof(genericTypes));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var methods = type
                    .GetMethods(bindingFlags)
                    .Where(mi => string.Equals(methodName, mi.Name, StringComparison.Ordinal))
                    .Where(mi => mi.ContainsGenericParameters)
                    .Where(mi => mi.GetGenericArguments().Length == genericTypes.Length)
                    .Where(mi => mi.GetParameters().Length == parameterTypes.Length)
                    .Select(mi => mi.MakeGenericMethod(genericTypes))
                    .Where(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameterTypes))
                    .ToList();

            return methods.Count > 1 ? throw new AmbiguousMatchException() : methods.FirstOrDefault();
        }

        /// <summary>
        /// Determines whether this instance is class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is class; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsClass(this Type type) => type.GetTypeInfo().IsClass;

        /// <summary>
        /// Determines whether this instance is abstract.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is abstract; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAbstract(this Type type) => type.GetTypeInfo().IsAbstract;

        /// <summary>
        /// Determines whether this instance is interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is interface; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInterface(this Type type) => type.GetTypeInfo().IsInterface;

        /// <summary>
        /// Determines whether this instance is primitive.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is primitive; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrimitive(this Type type) => type.GetTypeInfo().IsPrimitive;

        /// <summary>
        /// Determines whether [is value type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is value type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValueType(this Type type) => type.GetTypeInfo().IsValueType;

        /// <summary>
        /// Determines whether [is generic type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is generic type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericType(this Type type) => type.GetTypeInfo().IsGenericType;

        /// <summary>
        /// Determines whether [is generic parameter].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is generic parameter] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericParameter(this Type type) => type.IsGenericParameter;

        /// <summary>
        /// Determines whether the specified attribute type is defined.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>
        ///   <c>true</c> if the specified attribute type is defined; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDefined(this Type type, Type attributeType, bool inherit) =>
            type.GetTypeInfo().IsDefined(attributeType, inherit);

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>
        /// Attributes associated with the property represented by this PropertyInfo object.
        /// </returns>
        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit) =>
            type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).Cast<Attribute>().ToArray();

        /// <summary>
        /// Determines whether [is generic type definition].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is generic type definition] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericTypeDefinition(this Type type) => type.GetTypeInfo().IsGenericTypeDefinition;

        /// <summary>
        /// Bases the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>returns a type of data.</returns>
        public static Type BaseType(this Type type) => type.GetTypeInfo().BaseType;

        /// <summary>
        /// Assemblies the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>returns an Assembly object.</returns>
        public static Assembly Assembly(this Type type) => type.GetTypeInfo().Assembly;

        /// <summary>
        /// Determines whether [is i enumerable request].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is i enumerable request] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">type.</exception>
        public static bool IsIEnumerable(this Type type)
        {
            return type == null
                ? throw new ArgumentNullException(nameof(type))
                : type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        #endregion

        /// <summary>
        /// Tries to parse using the basic types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///  <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParseBasicType(this Type type, object value, out object result)
            => TryParseBasicType(type, value.ToStringInvariant(), out result);

        /// <summary>
        /// Tries to parse using the basic types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///  <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParseBasicType(this Type type, string value, out object result)
        {
            if (Definitions.BasicTypesInfo.ContainsKey(type))
                return Definitions.BasicTypesInfo[type].TryParse(value, out result);

            result = null;
            return false;
        }

        /// <summary>
        /// Tries the type of the set basic value to a property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///  <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TrySetBasicType(this PropertyInfo property, object value, object obj)
        {
            try
            {
                if (property.PropertyType.TryParseBasicType(value, out var propertyValue))
                {
                    property.SetValue(obj, propertyValue);
                    return true;
                }
            }
            catch
            {
                // swallow
            }

            return false;
        }

        /// <summary>
        /// Tries the type of the set to an array a basic type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///  <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TrySetArrayBasicType(this Type type, object value, Array array, int index)
        {
            try
            {
                if (value == null)
                {
                    array.SetValue(null, index);
                    return true;
                }

                if (type.TryParseBasicType(value, out var propertyValue))
                {
                    array.SetValue(propertyValue, index);
                    return true;
                }
            }
            catch
            {
                // swallow
            }

            return false;
        }

        /// <summary>
        /// Tries to set a property array with another array.
        /// </summary>
        /// <param name="propertyInfo">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TrySetArray(this PropertyInfo propertyInfo, object[] value, object obj)
        {
            var elementType = propertyInfo.PropertyType.GetElementType();

            if (elementType == null)
                return false;

            var targetArray = Array.CreateInstance(elementType, value.Length);

            var i = 0;
            foreach (var sourceElement in value)
            {
                elementType.TrySetArrayBasicType(sourceElement, targetArray, i++);
            }

            propertyInfo.SetValue(obj, targetArray);

            return true;
        }

        /// <summary>
        /// Gets property value or null.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="obj">The object.</param>
        /// <returns>The property value or null.</returns>
        public static object GetValueOrNull(this PropertyInfo propertyInfo, object obj)
        {
            try
            {
                var value = propertyInfo.GetValue(obj);
                var attr = Runtime.AttributeCache.RetrieveOne<PropertyDisplayAttribute>(propertyInfo);

                if (attr == null) return value;
                if (value == null) return attr.NullValue;

                return string.IsNullOrEmpty(attr.Format)
                    ? value
                    : ConvertObjectAndFormat(propertyInfo, value, attr.Format);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a MethodInfo from a Property Get method.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="nonPublic">if set to <c>true</c> [non public].</param>
        /// <returns>
        /// The cached MethodInfo.
        /// </returns>
        public static MethodInfo GetCacheGetMethod(this PropertyInfo propertyInfo, bool nonPublic = false)
            => GetMethodInfoCache(propertyInfo, nonPublic, CacheGetMethods.Value, true);

        /// <summary>
        /// Gets a MethodInfo from a Property Set method.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="nonPublic">if set to <c>true</c> [non public].</param>
        /// <returns>
        /// The cached MethodInfo.
        /// </returns>
        public static MethodInfo GetCacheSetMethod(this PropertyInfo propertyInfo, bool nonPublic = false)
            => GetMethodInfoCache(propertyInfo, nonPublic, CacheSetMethods.Value, false);

        private static MethodInfo GetMethodInfoCache(
            PropertyInfo propertyInfo,
            bool nonPublic,
            Dictionary<PropertyInfo, MethodInfo> cache,
            bool isGet)
        {
            MethodInfo methodInfo;

            if (!cache.ContainsKey(propertyInfo))
            {
                methodInfo = isGet ? propertyInfo.GetGetMethod(true) : propertyInfo.GetSetMethod(true);
                cache[propertyInfo] = methodInfo;
            }
            else
            {
                methodInfo = cache[propertyInfo];
            }

            return methodInfo?.IsPublic != false ? methodInfo : (nonPublic ? methodInfo : null);
        }

        private static object ConvertObjectAndFormat(PropertyInfo propertyInfo, object value, string format)
        {
            if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
                return Convert.ToDateTime(value).ToString(format);
            if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
                return Convert.ToInt32(value).ToString(format);
            if (propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(decimal?))
                return Convert.ToDecimal(value).ToString(format);
            if (propertyInfo.PropertyType == typeof(double) || propertyInfo.PropertyType == typeof(double?))
                return Convert.ToDouble(value).ToString(format);
            if (propertyInfo.PropertyType == typeof(byte) || propertyInfo.PropertyType == typeof(byte?))
                return Convert.ToByte(value).ToString(format);

            return value;
        }
    }
}