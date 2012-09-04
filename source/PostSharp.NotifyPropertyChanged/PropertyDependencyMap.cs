using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PostSharp.NotifyPropertyChanged
{
    public class PropertyDependencyMap
    {
        public static Dictionary<string, string[]> For(Type type)
        {
            var publicInstanceProperties = type.SelectPublicInstanceProperties();
            var methodDependencies = type.SelectMethodDependencies();

            var propertyDependencyMap =
                from property in publicInstanceProperties
                let dependencies = from dependentProperty in publicInstanceProperties
                                   where methodDependencies[property.GetGetMethod()].Contains(dependentProperty.GetGetMethod())
                                   select dependentProperty.Name
                select new { Name = property.GetNotificationName(), Dependencies = dependencies.Distinct() };

            return propertyDependencyMap.ToDictionary(property => property.Name, property => property.Dependencies.ToArray());
        }

        public static Dictionary<string, string[]> ForProperty(PropertyInfo referenceProperty)
        {
            var methodDependencies = referenceProperty.ReflectedType.SelectMethodDependencies();
            var publicInstanceProperties = referenceProperty.ReflectedType.SelectPublicInstanceProperties();
            var referencePropertyUses = referenceProperty.ReflectedType.SelectAllInstanceMethods().SelectUsesOf(referenceProperty.GetGetMethod());
            
            var propertyDependencyMap =
                from property in referenceProperty.PropertyType.SelectPublicInstanceProperties()
                let dependencies = from usingMethod in referencePropertyUses.SelectUsesOf(property.GetGetMethod())
                                   from dependentMethod in methodDependencies[usingMethod].Concat(new[] { usingMethod })
                                   join instanceProperty in publicInstanceProperties on dependentMethod equals instanceProperty.GetGetMethod()
                                   select instanceProperty.Name
                select new { Name = property.GetNotificationName(), Dependencies = dependencies.Distinct() };

            return propertyDependencyMap.ToDictionary(property => property.Name, property => property.Dependencies.ToArray());
        }
    }
}