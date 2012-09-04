using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Cascading_Tests
    {
        [Test]
        public static void When_I_modify_an_ObservedCollection()
        {
            var propertiesThatChanged = new List<string>();
            var observedCollection = new ObservableCollection<decimal>();
            var observingClass = new CollectionObservingClass { ObservedCollection = observedCollection };
            observingClass.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            observedCollection.Add(1.0M);

            "it should only notify once that ObservingProperty, ObservingCalulatedProperty and ObservingMethodCallCalculatedProperty on the ObservingClass have changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "ObservingProperty", "ObservingCalculatedProperty", "ObservingMethodCallCalculatedProperty" }));
        }

        [NotifyPropertyChanged]
        [Cascadeing]
        public class CollectionObservingClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public ObservableCollection<decimal> ObservedCollection { get; set; }

            public decimal ObservingProperty { get { return ObservedCollection.Sum(); } }
            public int ObservingCalculatedProperty { get { return CalculateMethod() + 1; } }
            public bool ObservingMethodCallCalculatedProperty { get { return ObservedCollection.Contains(2.0M); } }
            private int CalculateMethod() { return CalculatedProperty + 1; }
            protected int CalculatedProperty { get { return ObservedCollection.Count; } }
        }
    }
}
