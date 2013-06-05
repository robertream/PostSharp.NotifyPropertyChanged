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
            var notify = new NotifyDependentProperty();
            var propertiesThatChanged = notify.ObservePropertyChanges();

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
