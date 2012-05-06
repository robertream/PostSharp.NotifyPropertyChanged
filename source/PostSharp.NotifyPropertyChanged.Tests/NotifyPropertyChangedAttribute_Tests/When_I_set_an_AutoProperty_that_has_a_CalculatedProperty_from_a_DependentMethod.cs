using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_an_AutoProperty_that_has_a_CalculatedProperty_from_a_NestedDependentMethod()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyCalculatedProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.AutoProperty = 1.0M;

            "it should notify that both the AutoProperty and the CalculatedProperty have changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty", "CalculatedProperty" }));
        }

        [NotifyPropertyChanged]
        public class NotifyCalculatedProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public decimal AutoProperty { get; set; }
            public decimal CalculatedProperty { get { return PublicDependentMethod(); } }
            public decimal PublicDependentMethod() { return ProtectedDependentMethod(); }
            protected decimal ProtectedDependentMethod() { return NestedDependentMethod(); }
            private decimal NestedDependentMethod() { return AutoProperty; }
        }
    }
}
