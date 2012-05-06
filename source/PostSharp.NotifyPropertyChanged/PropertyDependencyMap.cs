using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Reflection;
using System.Reflection;

namespace PostSharp.NotifyPropertyChanged
{
    public class PropertyDependencyMap
    {
        public static Dictionary<string, HashSet<string>> For(Type type)
        {
            var instanceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToArray();
            var methodDependencyGraph = instanceMethods.ToDictionary(method => method, GetMethodDependenciesIn(instanceMethods));
            var methodDependencyMap = FindAllReachableMethodsIn(methodDependencyGraph);
            var instanceProperties =
                type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(property => property.CanRead && property.GetGetMethod() != null)
                    .ToArray();

            var propertyDependenies =
                from property in instanceProperties
                let dependencies = methodDependencyMap[property.GetGetMethod()]
                let dependentProperties = 
                    from instanceProperty in instanceProperties 
                    where dependencies.Contains(instanceProperty.GetGetMethod())
                    select instanceProperty.Name
                select new { property.Name, Dependencies = new HashSet<string>(dependentProperties) };

            return propertyDependenies.ToDictionary(property => property.Name, property => property.Dependencies);
        }

        private static Func<MethodInfo, HashSet<MethodInfo>> GetMethodDependenciesIn(MethodInfo[] methods)
        {
            return delegate (MethodInfo method)
                {
                    var dependecies =
                        from dependecy in ReflectionSearch.GetMethodsUsingDeclaration(method)
                        join dependentMethod in methods
                            on dependecy.UsingMethod equals dependentMethod
                        select dependentMethod;

                    return new HashSet<MethodInfo>(dependecies);
                };
        }

        public static Dictionary<T, HashSet<T>> FindAllReachableMethodsIn<T>(Dictionary<T, HashSet<T>> propertyDependencyGraph)
        {
            var propertyDependecyMap = new Dictionary<T, HashSet<T>>(propertyDependencyGraph);
            var propertyStack = new Stack<T>();
            foreach (var property in propertyDependecyMap)
            {
                var propertyName = property.Key;
                var propertyDependencies = property.Value;
                foreach (var dependentPropertyName in propertyDependencies)
                    propertyStack.Push(dependentPropertyName);

                while (propertyStack.Count > 0)
                {
                    var dependentProperty = propertyStack.Pop();
                    if (dependentProperty.Equals(propertyName) || !propertyDependecyMap.ContainsKey(dependentProperty))
                        continue;

                    var dependenciesToAdd =
                        from indirectDependentProperty in propertyDependecyMap[dependentProperty]
                        where !indirectDependentProperty.Equals(propertyName) && !propertyDependencies.Contains(indirectDependentProperty)
                        select indirectDependentProperty;

                    foreach (var newDependency in dependenciesToAdd)
                    {
                        propertyDependencies.Add(newDependency);
                        propertyStack.Push(newDependency);
                    }
                }
            }
            return propertyDependecyMap;
        }
    }
}