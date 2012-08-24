using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PostSharp.Reflection;

namespace PostSharp.NotifyPropertyChanged
{
    public static class ReflectionExtensions
    {
        public static MethodInfo[] SelectAllInstanceMethods(this Type @this)
        {
            return @this.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static IEnumerable<PropertyInfo> SelectAllInstanceProperties(this Type @this)
        {
            return @this.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(property => property.CanRead && property.GetGetMethod() != null);
        }

        public static IEnumerable<PropertyInfo> SelectPublicInstanceProperties(this Type @this)
        {
            return @this.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.CanRead && property.GetGetMethod() != null);
        }

        public static IEnumerable<PropertyInfo> SelectInstencePropertiesOf<T>(this Type @this)
        {
            return @this.SelectPublicInstanceProperties().Where(property => typeof(T).IsAssignableFrom(property.PropertyType));
        }

        public static IEnumerable<MethodInfo> SelectPublicInstanceMethods(this Type @this)
        {
            return from method in @this.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                   where method.DeclaringType != typeof(object)
                   where !method.IsSpecialName
                   select method;
        }

        public static IEnumerable<MethodInfo> SelectUsesOf(this IEnumerable<MethodInfo> @this, MemberInfo member)
        {
            return from usingDeclaration in ReflectionSearch.GetMethodsUsingDeclaration(member)
                   join method in @this on usingDeclaration.UsingMethod equals method
                   select method;
        }

        public static IEnumerable<MethodInfo> SelectUsesOf(this HashSet<MethodInfo> @this, MemberInfo member)
        {
            return from usingDeclaration in ReflectionSearch.GetMethodsUsingDeclaration(member)
                   let usingMethod = usingDeclaration.UsingMethod as MethodInfo
                   where @this.Contains(usingMethod)
                   select usingMethod;
        }

        public static Dictionary<MethodInfo, HashSet<MethodInfo>> SelectInstanceMethodDependencyGraph(this Type @this)
        {
            var instanceMethods = @this.SelectAllInstanceMethods();
            return instanceMethods.ToDictionary(method => method, method => new HashSet<MethodInfo>(instanceMethods.SelectUsesOf(method)));
        }

        public static Dictionary<MethodInfo, HashSet<MethodInfo>> SelectMethodDependenciesFor(this Type @this)
        {
            return @this.SelectInstanceMethodDependencyGraph().FindAllReachableNodes();
        }

        public static Dictionary<T, HashSet<T>> FindAllReachableNodes<T>(this Dictionary<T, HashSet<T>> @this)
        {
            var graph = new Dictionary<T, HashSet<T>>(@this);
            var nodeStack = new Stack<T>();
            foreach (var node in graph)
            {
                var nodeId = node.Key;
                var reachableNodes = node.Value;
                foreach (var reachableNode in reachableNodes)
                    nodeStack.Push(reachableNode);

                while (nodeStack.Count > 0)
                {
                    var nextNode = nodeStack.Pop();
                    if (nextNode.Equals(nodeId) || !graph.ContainsKey(nextNode))
                        continue;

                    var nodesToAdd =
                        from nodeToAdd in graph[nextNode]
                        where !nodeToAdd.Equals(nodeId) && !reachableNodes.Contains(nodeToAdd)
                        select nodeToAdd;

                    foreach (var nodeToAdd in nodesToAdd)
                    {
                        reachableNodes.Add(nodeToAdd);
                        nodeStack.Push(nodeToAdd);
                    }
                }
            }
            return graph;
        }
    }
}