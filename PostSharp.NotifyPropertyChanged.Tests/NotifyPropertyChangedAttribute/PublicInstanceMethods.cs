using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void PublicInstanceMethods()
        {
            var type = typeof(Methods);

            var p = NotifyPropertyChangedAttribute.PublicInstanceMethods(type);

            var methodNames = from method in p select method.Name;
            "it should only return the public instance methods for the type"
                .AssertThat(methodNames.ToArray(), Is.EquivalentTo(new[] { "PublicInstanceMethod", "PublicInterfaceMethod", "PublicBaseClassMethod" }));
        }

        // ReSharper disable EventNeverInvoked
        // ReSharper disable UnusedMember.Local
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
        // ReSharper restore UnusedMember.Local
        // ReSharper restore EventNeverInvoked
    }
}
