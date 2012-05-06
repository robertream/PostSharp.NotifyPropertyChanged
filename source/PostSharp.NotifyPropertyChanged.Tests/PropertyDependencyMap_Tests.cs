using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class PropertyDependencyMap_Tests
    {
        [Test]
        public static void When_I_FindAllReachableMethodsIn_a_cyclic_MethodDependencyGraph()
        {
            var methodDependencyGraph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"method A", new HashSet<string> {"method C"}},
                        {"method B", new HashSet<string> {"method A"}},
                        {"method C", new HashSet<string> {"method B"}},
                    };

            var reachableMethodsFor = PropertyDependencyMap.FindAllReachableMethodsIn(methodDependencyGraph);

            "should return all the other methods in the cycle for all methods in the cyclic graph"
                .AssertThat(reachableMethodsFor["method A"].ToArray(), Is.EquivalentTo(new[] { "method B", "method C" }))
                .AssertThat(reachableMethodsFor["method B"].ToArray(), Is.EquivalentTo(new[] { "method A", "method C" }))
                .AssertThat(reachableMethodsFor["method C"].ToArray(), Is.EquivalentTo(new[] { "method A", "method B" }));
        }

        [Test]
        public static void When_I_FindAllReachableMethods_an_acyclic_MethodDependencyGraph()
        {
            var methodDependencyGraph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"grand parent", new HashSet<string> {"parent"}},
                        {"parent", new HashSet<string> {"child"}},
                        {"child", new HashSet<string>()},
                    };

            var reachableMethodsFor = PropertyDependencyMap.FindAllReachableMethodsIn(methodDependencyGraph);

            "it should return a map of the direct AND indirect dependencies for all methods in the graph"
                .AssertThat(reachableMethodsFor["child"].ToArray(), Is.EquivalentTo(new string[0]))
                .AssertThat(reachableMethodsFor["parent"].ToArray(), Is.EquivalentTo(new[] {"child"}))
                .AssertThat(reachableMethodsFor["grand parent"].ToArray(), Is.EquivalentTo(new[] {"parent", "child"}));
        }

        [Test]
        public static void When_I_FindAllReachableMethodsIn_an_acyclic_MethodDependencyGraph()
        {
            var methodDependencyGraph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"grand parent", new HashSet<string> {"parent"}},
                        {"parent", new HashSet<string> {"child"}},
                        {"child", new HashSet<string>()},
                    };

            var reachableMethodsFor = PropertyDependencyMap.FindAllReachableMethodsIn(methodDependencyGraph);

            "it should return a map of the direct AND indirect dependencies for all methods in the graph"
                .AssertThat(reachableMethodsFor["child"].ToArray(), Is.EquivalentTo(new string[0]))
                .AssertThat(reachableMethodsFor["parent"].ToArray(), Is.EquivalentTo(new[] { "child" }))
                .AssertThat(reachableMethodsFor["grand parent"].ToArray(), Is.EquivalentTo(new[] { "parent", "child" }));
        }

        [Test]
        public static void When_I_FindAllReachableMethodsIn_a_MethodDependencyGraph_with_an_dependent_method_that_is_not_in_the_graph()
        {
            var methodDependencyGraph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"parent", new HashSet<string> {"child"}},
                    };

            var reachableMethodsFor = PropertyDependencyMap.FindAllReachableMethodsIn(methodDependencyGraph);

            "it should only return dependencies for the methods in the property dependency graph"
                .AssertThat(reachableMethodsFor["parent"].ToArray(), Is.EquivalentTo(new[] { "child" }));
        }
    }
}