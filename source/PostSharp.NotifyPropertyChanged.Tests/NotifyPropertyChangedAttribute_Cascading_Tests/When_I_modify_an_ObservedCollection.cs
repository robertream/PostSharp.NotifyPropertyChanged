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

            "it should only notify once that the ObservingProperty and the ObservingCalulatedProperty on the ObservingClass has changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "ObservingProperty", "ObservingCalculatedProperty" }));
        }

        [NotifyPropertyChanged]
        [Cascadeing]
        public class CollectionObservingClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public ObservableCollection<decimal> ObservedCollection { get; set; }

            public decimal ObservingProperty { get { return ObservedCollection.Sum(); } }
            public decimal ObservingCalculatedProperty { get { return CalculateMethod() + 1; } }
            private decimal CalculateMethod() { return CalculatedProperty + 1; }
            protected decimal CalculatedProperty { get { return ObservingProperty + 1; } }
        }
    }
}
