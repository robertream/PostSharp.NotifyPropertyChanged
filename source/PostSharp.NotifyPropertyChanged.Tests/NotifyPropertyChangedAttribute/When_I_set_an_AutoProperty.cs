using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_an_AutoProperty()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyAutoProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.AutoProperty = "Test";

            "it should notify that the AutoProperty has changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] {"AutoProperty"}));
        }

        [NotifyPropertyChanged]
        public class NotifyAutoProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string AutoProperty { get; set; }
        }
    }
}
