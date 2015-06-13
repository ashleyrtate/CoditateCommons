using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Property match options for use when copying property values between objects.
    /// </summary>
    public enum PropertyMatch
    {
        /// <summary>
        /// Destination object must contain an exactly matching property
        /// for all properties found on source object.
        /// </summary>
        RequireExact,
        /// <summary>
        /// Ignore source object properties not found on destination object.
        /// </summary>
        IgnoreMissing
    }

    /// <summary>
    /// PropertyUtils contains methods for working with Properties and nested properties.
    /// </summary>
    public static class PropertyUtils
    {
        private static readonly char[] PropertyDelimiter = {'.'};

        /// <summary>
        /// Determines whether the specified obj type has property.
        /// </summary>
        /// <param name="objType">Type of the obj.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// 	<c>true</c> if the specified obj type has property; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasProperty(Type objType, string propertyName)
        {
            Arg.CheckNull("objType", objType);
            Arg.CheckNullOrEmpty("propertyName", propertyName);

            return objType.GetProperty(propertyName) != null;
        }

        /// <summary>
        /// Gets the value of the specified property.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static object GetValue(object obj, string propertyName)
        {
            Arg.CheckNull("obj", obj);
            Arg.CheckNull("propertyName", propertyName);

            PropertyInfo propInfo = obj.GetType().GetProperty(propertyName);
            if (propInfo == null)
            {
                throw new ArgumentException("Propery '" + propertyName + "' not found on type '" +
                                            obj.GetType() + "'");
            }
            return propInfo.GetValue(obj, null);
        }

        /// <summary>
        /// Finds a matching read/write property on the specified type if one exists.
        /// </summary>
        /// <param name="p">The property to match.</param>
        /// <param name="t">The type to search for a matching property.</param>
        /// <returns></returns>
        public static PropertyInfo FindMatching(PropertyInfo p, Type t)
        {
            PropertyInfo match = t.GetProperty(p.Name);
            if (match == null || !match.CanRead || !match.CanWrite)
            {
                return null;
            }
            Type destType = match.PropertyType;
            destType = Nullable.GetUnderlyingType(destType) ?? destType;
            Type sourceType = p.PropertyType;
            sourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
            if (destType != sourceType)
            {
                return null;
            }
            return match;
        }

        /// <summary>
        /// Copies all properties from the first object to the second.
        /// </summary>
        /// <param name="from">The object to copy from.</param>
        /// <param name="to">The object to copy to.</param>
        /// <param name="excludedProperties">The properties to exclude when copying.</param>
        /// <remarks>
        /// Property values are copied if a writable property of the same name and
        /// type is found on the "to" object. If the property type implements ICollection it is ignored.
        /// </remarks>
        /// <exception cref="InvalidOperationException">If the destination object is 
        /// missing one or more properties found on the source object.</exception>
        public static void CopyProperties(object from, object to, params string[] excludedProperties)
        {
            CopyProperties(from, to, PropertyMatch.RequireExact, excludedProperties);
        }

        /// <summary>
        /// Copies all properties from the first object to the second.
        /// </summary>
        /// <param name="from">The object to copy from.</param>
        /// <param name="to">The object to copy to.</param>
        /// <param name="matchType">The property matching type to use.</param>
        /// <param name="excludedProperties">The properties to exclude when copying.</param>
        /// <remarks>
        /// Property values are copied if a writable property of the same name and
        /// type is found on the "to" object. If the property type implements ICollection it is ignored.
        /// </remarks>
        /// <exception cref="InvalidOperationException">If the destination object is 
        /// missing one or more required properties found on the source object.</exception>
        public static void CopyProperties(object from, object to, PropertyMatch matchType, params string[] excludedProperties)
        {
            Arg.CheckNull("from", from);
            Arg.CheckNull("to", to);
            Arg.CheckNull("excludedProperties", excludedProperties);

            foreach (PropertyInfo pFrom in GetProperties(from.GetType(), true, false))
            {
                if (excludedProperties.Contains(pFrom.Name))
                {
                    continue;
                }
                PropertyInfo pTo = FindMatching(pFrom, to.GetType());
                if (pTo == null)
                {
                    if (matchType == PropertyMatch.RequireExact)
                    {
                        string message = string.Format("Expected property '{0}' not found on type '{1}'", pFrom.Name,
                                                       to.GetType().FullName);
                        throw new InvalidOperationException(message);
                    }
                    continue;
                }
                object value = pFrom.GetValue(from, null);
                pTo.SetValue(to, value, null);
            }
        }

        /// <summary>
        /// Finds all acceptable properties on the specified type.
        /// </summary>
        /// <returns></returns>
        public static List<PropertyInfo> GetProperties(Type t, bool mustRead, bool mustWrite)
        {
            Arg.CheckNull("t", t);

            return t.GetProperties().Where(p => IsAcceptableProperty(p, mustRead, mustWrite)).ToList();
        }

        /// <summary>
        /// Determines whether a property is acceptable for copy and comparision operations
        /// using property utils.
        /// </summary>
        /// <param name="p">The property to check.</param>
        /// <param name="mustRead">if set to <c>true</c> the property must be readable.</param>
        /// <param name="mustWrite">if set to <c>true</c> the property must be writable.</param>
        /// <returns>
        /// 	<c>true</c> if the property is acceptable; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method always returns false for the following types of properties:
        /// * Indexed properties
        /// * Properties with a property type that implements <see cref="ICollection"/>
        /// </remarks>
        public static bool IsAcceptableProperty(PropertyInfo p, bool mustRead, bool mustWrite)
        {
            Arg.CheckNull("p", p);

            if (typeof (ICollection).IsAssignableFrom(p.PropertyType))
            {
                return false;
            }
            if (p.GetIndexParameters().Length > 0)
            {
                return false;
            }
            if (mustRead && !p.CanRead)
            {
                return false;
            }
            if (mustWrite && !p.CanWrite)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds a property of an object using a dot-delimited
        /// property path. For example, passing a propertyPath value
        /// of "Child.Name" searches for the value of the property obj.Child.Name. 
        /// <P>The query is not case sensitive.</P>
        /// </summary>
        /// <param name="obj">The root object from which to search. Passing null
        /// results in a null return value.
        /// </param>
        /// <param name="propertyPath">The property path to search. Passing a null or empty string
        /// results in the obj parameter itself being returned.
        /// </param>
        /// <returns>The object instance at the specified propertyPath or null if any
        /// property on the path is null.</returns>
        /// <exception cref="System.ArgumentException">Thrown when any property 
        /// refereced in the propertyPath does not exist.
        /// </exception>
        public static object FindValue(object obj, string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath))
            {
                return obj;
            }

            string[] propNames = propertyPath.Split(PropertyDelimiter);

            object propValue = obj;
            foreach (string propName in propNames)
            {
                if (propValue == null)
                {
                    break;
                }
                propValue = GetValue(propValue, propName);
            }
            return propValue;
        }

        /// <summary>
        /// Examines the nested properties of an object to determine whether
        /// the specified propertyPath path is valid. This method follows the same
        /// path syntax rules as <see cref="FindValue"/>.
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="propertyPath"></param>
        /// <returns>true if the nested property described by the propertyPath 
        /// parameter exists, false otherwise</returns>
        public static bool IsPathValid(Type objType, string propertyPath)
        {
            Arg.CheckNull("objType", objType);

            if (string.IsNullOrEmpty(propertyPath))
            {
                return true;
            }

            string[] propNames = propertyPath.Split(PropertyDelimiter);

            Type propType = objType;
            foreach (string propName in propNames)
            {
                if (propType == null)
                {
                    break;
                }
                PropertyInfo propInfo = objType.GetProperty(propName);
                if (propInfo == null)
                {
                    propType = null;
                    break;
                }
                propType = propInfo.PropertyType;
            }
            return propType != null;
        }
    }
}