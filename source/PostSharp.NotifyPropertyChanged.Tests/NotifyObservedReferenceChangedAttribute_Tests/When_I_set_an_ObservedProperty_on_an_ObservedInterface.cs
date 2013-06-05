using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyObservedReferenceChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_an_NonIntercaeProperty_on_an_ObservedInterface()
        {
            var observedClass = new ObservedInstance();
            var observingClass = new InterfaceObservingClass { ObservedReference = observedClass, };
            var propertiesThatChanged = observingClass.ObservePropertyChanges();

            observedClass.NonInterfaceProperty = 1.0M;

            "it should NOT notify of any changes on the InterfaceObservingClass"
                .AssertThat(propertiesThatChanged, Is.Empty);
        }

        public interface ObservedInterface : INotifyPropertyChanged
        {
            decimal ObservedProperty { get; }
        }

        [NotifyPropertyChanged]
        public class ObservedInstance : ObservedInterface
        {
            public decimal ObservedProperty { get; set; }
            public decimal NonInterfaceProperty { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }

        [NotifyPropertyChanged]
        [NotifyObservedReferenceChanged]
        public class InterfaceObservingClass
        {
            public ObservedInterface ObservedReference { get; set; }

            public decimal ObservingProperty { get { return ObservedReference.ObservedProperty; } }
        }
    }
}