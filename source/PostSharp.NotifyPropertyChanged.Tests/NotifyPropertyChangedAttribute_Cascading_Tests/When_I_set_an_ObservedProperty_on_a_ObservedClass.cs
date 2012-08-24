using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Cascading_Tests
    {
        [Test]
        public static void When_I_set_an_ObservedProperty_on_a_ObservedClass()
        {
            var propertiesThatChanged = new List<string>();
            var observedClass = new ObservedClass();
            var observingClass = new ObservingClass { ObservedClass = observedClass };
            observingClass.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            observedClass.ObservedProperty = 1.0M;

            "it should only notify that the ObservingProperty on the ObservingClass has changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "ObservingProperty" }));
        }

        [NotifyPropertyChanged]
        public class ObservedClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public decimal ObservedProperty { get; set; }

            public decimal NonObservedProperty { get; set; }
        }

        [NotifyPropertyChanged]
        [Cascadeing]
        public class ObservingClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public ObservedClass ObservedClass { get; set; }

            public ObservedClass NonObservedClass { get; set; }

            public decimal ObservingProperty
            {
                get { return ObservedClass.ObservedProperty; }
            }

            public string NonObservingProperty
            {
                get { return ObservedClass.ToString(); }
            }
        }
    }
}