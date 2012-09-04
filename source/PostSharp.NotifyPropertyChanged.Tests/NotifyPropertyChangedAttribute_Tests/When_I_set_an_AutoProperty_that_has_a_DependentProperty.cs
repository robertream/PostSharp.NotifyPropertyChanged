using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_an_AutoProperty_that_has_a_DependentProperty()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyDependentProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.AutoProperty = 1.0M;

            "it should notify that both the AutoProperty and the DependentProperty have changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty", "DependentProperty" }));
        }

        [NotifyPropertyChanged]
        public class NotifyDependentProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public decimal AutoProperty { get; set; }
            public decimal DependentProperty { get { return AutoProperty; } }

            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }
    }
}
