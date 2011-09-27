using System.Collections;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PostSharp.Aspects;
using PostSharp.Reflection;
using System.Collections.Generic;
using System;
using PostSharp.Extensibility;

namespace PostSharp.NotifyPropertyChanged.Tests
{
    [TestFixture]
    public class NotifyPropertyChangedAttribute_Tests
    {
        public interface Interface
        {
            void PublicInterfaceMethod();
        }

        public class BaseClass
        {
            public void PublicBaseClassMethod() { }
        }

        public class Methods : BaseClass, Interface
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string AutoProperty { get; set; }
            public void PublicInstanceMethod() { }
            public void PublicInterfaceMethod() { }
            private void PrivateInstanceMethod() { }
            public static void StaticInstanceMethod() { }
        }

        [Test]
        public void PublicInstanceMethods()
        {
            var type = typeof(Methods);

            var p = NotifyPropertyChangedAttribute.PublicInstanceMethods(type);

            var methodNames = from method in p select method.Name;
            "it should only return the public instance methods for the type"
                .AssertThat(methodNames.ToArray(), Is.EquivalentTo(new[] { "PublicInstanceMethod", "PublicInterfaceMethod", "PublicBaseClassMethod" }));
        }

        [NotifyPropertyChanged]
        public class NotifyAutoProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string AutoProperty { get; set; }
        }

        [Test]
        public void When_I_set_an_AutoProperty()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyAutoProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.AutoProperty = "Test";

            "it should notify that the AutoProperty has changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty" }));
        }

        [NotifyPropertyChanged]
        public class NotifyDependentProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public decimal AutoProperty { get; set; }
            public decimal DependentProperty { get { return AutoProperty; } }
        }

        [Test]
        public void When_I_set_an_AutoProperty_that_has_a_DependentProperty()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyDependentProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.AutoProperty = 1.0M;

            "it should notify that both the AutoProperty and the DependentProperty have changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty", "DependentProperty" }));
        }

        [NotifyPropertyChanged]
        public class NotifyNestedDependentProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public decimal AutoProperty { get; set; }
            public decimal DependentProperty { get { return AutoProperty + NestedDependentProperty; } }
            public decimal NestedDependentProperty { get { return DependentProperty; } }
        }

        [Test]
        public void When_I_set_an_AutoProperty_that_has_a_NestedDependentProperty()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyNestedDependentProperty();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.AutoProperty = 1.0M;

            "it should notify that both the AutoProperty and the DependentProperty have changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty", "DependentProperty", "NestedDependentProperty" }));
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
        }

        [Test]
        public void When_I_set_a_PropertyWithBackingField()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyPropertyWithBackingField();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.PropertyWithBackingField = 1;

            "it should notify that the PropertyWithBackingField has changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "PropertyWithBackingField" }));
        }

        [NotifyPropertyChanged]
        public class NotifyMoreThanOnceInTheSamePublicMethod : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string AutoProperty { get; set; }

            public void MoreThanOnceInTheSamePublicMethod()
            {
                AutoProperty = "once";
                AutoProperty = "twice";
            }
        }

        [Test]
        public void When_I_set_a_Property_MoreThanOnceInTheSamePublicMethod()
        {
            var propertiesThatChanged = new List<string>();
            var notify = new NotifyMoreThanOnceInTheSamePublicMethod();
            notify.PropertyChanged += (@object, @event) => propertiesThatChanged.Add(@event.PropertyName);

            notify.MoreThanOnceInTheSamePublicMethod();

            "it should only notify ONCE that the Property changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty", }));
        }
    }
}
