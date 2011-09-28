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
        public static void When_I_create_a_PropertyDependencyMapFrom_a_simple_cyclic_PropertyDependencyGraph()
        {
            var simpleAcyclicGraph =
                new Dictionary<string, HashSet<string>>
                    {
                        {"property A", new HashSet<string> {"property C"}},
                        {"property B", new HashSet<string> {"property A"}},
                        {"property C", new HashSet<string> {"property B"}},
                    };

            var actualMap = new PropertyDependency().MapFrom(simpleAcyclicGraph);

            "it should return all the other properties in the cycle for all properties in the cyclic graph"
                .AssertThat(actualMap["property A"].ToArray(), Is.EquivalentTo(new[] {"property B", "property C"}))
                .AssertThat(actualMap["property B"].ToArray(), Is.EquivalentTo(new[] {"property A", "property C"}))
                .AssertThat(actualMap["property C"].ToArray(), Is.EquivalentTo(new[] {"property A", "property B"}));
        }
    }
}
