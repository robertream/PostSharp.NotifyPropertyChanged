using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class PropertyDependency_Tests
    {
        [Test]
        public static void MapFrom_a_simple_acyclic_PropertyDependencyGraph()
        {
            var simpleAcyclicGraph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"grand parent", new HashSet<string> {"parent"}},
                        {"parent", new HashSet<string> {"child"}},
                        {"child", new HashSet<string>()},
                    };

            var actualMap = PropertyDependency.MapFrom(simpleAcyclicGraph);

            "should return a map of the direct and indirect dependencies for all properties in the graph"
                .AssertThat(actualMap["child"].ToArray(), Is.EquivalentTo(new string[0]))
                .AssertThat(actualMap["parent"].ToArray(), Is.EquivalentTo(new[] {"child"}))
                .AssertThat(actualMap["grand parent"].ToArray(), Is.EquivalentTo(new[] {"parent", "child"}));
        }
    }
}
