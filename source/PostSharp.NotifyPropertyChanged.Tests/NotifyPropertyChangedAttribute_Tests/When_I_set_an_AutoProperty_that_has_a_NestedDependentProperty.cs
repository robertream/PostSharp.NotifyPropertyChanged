using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_an_AutoProperty_that_has_a_NestedDependentProperty()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyNestedDependentProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.AutoProperty = 1.0M;

            "it should notify that both the AutoProperty and the DependentProperty have changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty", "DependentProperty", "NestedDependentProperty" }));
        }

        [NotifyPropertyChanged]
        public class NotifyNestedDependentProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public decimal AutoProperty { get; set; }
            public decimal DependentProperty { get { return AutoProperty + NestedDependentProperty; } }
            public decimal NestedDependentProperty { get { return ProtectedNestedDependentProperty; } }
            protected decimal ProtectedNestedDependentProperty { get { return PrivateNestedDependentProperty; } }
            private decimal PrivateNestedDependentProperty { get { return DependentProperty; } }
        }
    }
}
