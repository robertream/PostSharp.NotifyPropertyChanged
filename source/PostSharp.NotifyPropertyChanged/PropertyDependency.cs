using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Reflection;

namespace PostSharp.NotifyPropertyChanged
{
    public class PropertyDependency
    {
        public Func<Type, Dictionary<string, HashSet<string>>> MapFor;
        public Func<Type, Dictionary<string, HashSet<string>>> GraphFor;
        public Func<Dictionary<string, HashSet<string>>, Dictionary<string, HashSet<string>>> MapFrom;

        public PropertyDependency()
        {
            MapFor = delegate(Type type)
            {
                return MapFrom(GraphFor(type));
            };

            GraphFor = delegate(Type type)
            {
                var publicProperties =
                    type.GetProperties()
                        .Where(property => property.CanRead && property.GetGetMethod() != null)
                        .ToArray();

                var directDependencies =
                    from property in publicProperties
                    from dependency in ReflectionSearch.GetMethodsUsingDeclaration(property.GetGetMethod())
                    join dependencyProperty in publicProperties 
                        on dependency.UsingMethod equals dependencyProperty.GetGetMethod()
                    let dependentPropertyName = dependencyProperty.Name
                    group dependentPropertyName by property.Name 
                        into propertyDependencies
                    select propertyDependencies;

                return directDependencies.ToDictionary(dependencies => dependencies.Key, dependencies => new HashSet<string>(dependencies));
            };

            MapFrom = delegate(Dictionary<string, HashSet<string>> propertyDependencyGraph)
            {
                var propertyDependecyMap = new Dictionary<string, HashSet<string>>(propertyDependencyGraph);
                var propertyStack = new Stack<string>();
                foreach (var property in propertyDependecyMap)
                {
                    var propertyName = property.Key;
                    var propertyDependencies = property.Value;
                    foreach (var dependentPropertyName in propertyDependencies)
                        propertyStack.Push(dependentPropertyName);

                    while (propertyStack.Count > 0)
                    {
                        var dependentProperty = propertyStack.Pop();
                        if (dependentProperty == propertyName || !propertyDependecyMap.ContainsKey(dependentProperty))
                            continue;

                        var dependenciesToAdd =
                            from indirectDependentProperty in propertyDependecyMap[dependentProperty]
                            where indirectDependentProperty != propertyName && !propertyDependencies.Contains(indirectDependentProperty)
                            select indirectDependentProperty;

                        foreach (var newDependency in dependenciesToAdd)
                        {
                            propertyDependencies.Add(newDependency);
                            propertyStack.Push(newDependency);
                        }
                    }
                }
                return propertyDependecyMap;
            };
        }
    }
}