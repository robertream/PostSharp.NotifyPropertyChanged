using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_a_PropertyWithBackingField()
        {
            var notify = new NotifyPropertyWithBackingField();
            var propertiesThatChanged = notify.ObservePropertyChanges();

            notify.PropertyWithBackingField = 1;

            "it should notify that the PropertyWithBackingField has changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "PropertyWithBackingField" }));
        }

        [NotifyPropertyChanged]
        public class NotifyPropertyWithBackingField : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public int BackingField;
            public int PropertyWithBackingField
            {
                get { return 2 * BackingField; }
                set { BackingField = value / 2; }
            }

            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }
    }
}
