using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_a_NonPublicAutoProperty()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NonPublicAutoProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.SetNonPublicAutoProperty();

            "it should NOT notify that a property changed"
                .AssertThat(propertiesThatChanged, Is.Empty);
        }

        [NotifyPropertyChanged]
        public class NonPublicAutoProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void SetNonPublicAutoProperty() { StaticAutoProperty = PrivateAutoProperty = ProtectedAutoProperty = 42; }
            public static int StaticAutoProperty { get; set; }
            protected int ProtectedAutoProperty { get; set; }
            private int PrivateAutoProperty { get; set; }

            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }
    }
}
