using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Cascading_Tests
    {
        [Test]
        public static void When_I_set_an_ObservedProperty_on_an_ObservedReference()
        {
            var propertiesThatChanged = new List<string>();
            var observedClass = new ObservedClass();
            var observingClass = new ObservingClass { ObservedReference = observedClass, NonObservedReference = new ObservedClass() };
            observingClass.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            observedClass.ObservedProperty = 1.0M;

            "it should only notify once that the ObservingProperty and the ObservingCalulatedProperty on the ObservingClass has changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "ObservingProperty", "ObservingCalculatedProperty" }));
        }

        [NotifyPropertyChanged]
        public class ObservedClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public decimal ObservedProperty { get; set; }
            public decimal NonObservedProperty { get; set; }

            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }

        [NotifyPropertyChanged]
        [NotifyObservedReferenceChanged]
        public class ObservingClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public ObservedClass ObservedReference { get; set; }
            public ObservedClass NonObservedReference { get; set; }

            public decimal ObservingProperty { get { return ObservedReference.ObservedProperty; } }
            public decimal ObservingCalculatedProperty { get { return CalculateMethod() + 1; } }
            private decimal CalculateMethod() { return CalculatedProperty + 1; }
            protected decimal CalculatedProperty { get { return ObservingProperty + 1; } }
            public string NonObservingProperty1 { get { return ObservedReference.ToString(); } }
            public decimal NonObservingProperty2 { get { return NonObservedReference.ObservedProperty; } }

            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }
    }
}