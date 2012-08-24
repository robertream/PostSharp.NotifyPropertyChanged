using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class PropertyDependencyMap_Tests
    {
        [Test]
        public static void When_I_FindAllReachableNodes_in_a_cyclic_Graph()
        {
            var graph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"node A", new HashSet<string> {"node C"}},
                        {"node B", new HashSet<string> {"node A"}},
                        {"node C", new HashSet<string> {"node B"}},
                    };

            var reachableNodessFor = graph.FindAllReachableNodes();

            "should return all the other nodes in the cycle for all nodes in the cyclic Graph"
                .AssertThat(reachableNodessFor["node A"].ToArray(), Is.EquivalentTo(new[] { "node B", "node C" }))
                .AssertThat(reachableNodessFor["node B"].ToArray(), Is.EquivalentTo(new[] { "node A", "node C" }))
                .AssertThat(reachableNodessFor["node C"].ToArray(), Is.EquivalentTo(new[] { "node A", "node B" }));
        }

        [Test]
        public static void When_I_FindAllReachableNodes_in_an_acyclic_Graph()
        {
            var graph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"grand parent", new HashSet<string> {"parent"}},
                        {"parent", new HashSet<string> {"child"}},
                        {"child", new HashSet<string>()},
                    };

            var reachableNodessFor = graph.FindAllReachableNodes();

            "it should return a map of the direct AND indirect dependencies for all nodes in the Graph"
                .AssertThat(reachableNodessFor["child"].ToArray(), Is.EquivalentTo(new string[0]))
                .AssertThat(reachableNodessFor["parent"].ToArray(), Is.EquivalentTo(new[] { "child" }))
                .AssertThat(reachableNodessFor["grand parent"].ToArray(), Is.EquivalentTo(new[] { "parent", "child" }));
        }

        [Test]
        public static void When_I_FindAllReachableNodes_in_a_Graph_with_an_node_that_is_not_in_the_Graph()
        {
            var graph = new Dictionary<string, HashSet<string>> { {"parent", new HashSet<string> {"child"}}, };

            var reachableNodessFor = graph.FindAllReachableNodes();

            "it should only return reachable nodes for the nodes in the Graph"
                .AssertThat(reachableNodessFor["parent"].ToArray(), Is.EquivalentTo(new[] { "child" }));
        }
    }
}