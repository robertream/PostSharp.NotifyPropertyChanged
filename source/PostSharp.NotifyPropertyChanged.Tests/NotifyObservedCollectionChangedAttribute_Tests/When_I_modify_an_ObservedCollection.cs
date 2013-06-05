using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyObservedCollectionChangedAttribute_Tests
    {
        [Test]
        public static void When_I_modify_an_ObservedCollection()
        {
            var observingClass = new CollectionObservingClass();
            var observedCollection = observingClass.ObservedCollection;
            var propertiesThatChanged = observingClass.ObservePropertyChanges();

            observedCollection.Add(1.0M);

            "it should only notify once that ObservingProperty, ObservingCalulatedProperty and ObservingMethodCallCalculatedProperty on the ObservingClass have changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "ObservingProperty", "ObservingCalculatedProperty", "ObservingMethodCallCalculatedProperty" }));

            "it should update the backing field for the ObservedCollection property"
                .AssertThat(observingClass.ObservingProperty, Is.EqualTo(observedCollection.Sum()));
        }

        [NotifyPropertyChanged]
        [NotifyObservedReferenceChanged]
        [NotifyObservedCollectionChanged]
        public class CollectionObservingClass : INotifyPropertyChanged
        {
            public CollectionObservingClass()
            {
                ObservedCollection = new ObservableCollection<decimal>();
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public ObservableCollection<decimal> ObservedCollection { get; set; }

            public decimal ObservingProperty { get { return ObservedCollection.Sum(); } }
            public int ObservingCalculatedProperty { get { return CalculateMethod() + 1; } }
            public bool ObservingMethodCallCalculatedProperty { get { return ObservedCollection.Contains(2.0M); } }
            private int CalculateMethod() { return CalculatedProperty + 1; }
            protected int CalculatedProperty { get { return ObservedCollection.Count; } }

            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }
    }
}
