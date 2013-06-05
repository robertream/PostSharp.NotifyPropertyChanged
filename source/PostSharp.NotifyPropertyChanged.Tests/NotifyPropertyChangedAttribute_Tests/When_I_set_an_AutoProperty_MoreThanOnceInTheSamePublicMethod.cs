using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static partial class NotifyPropertyChangedAttribute_Tests
    {
        [Test]
        public static void When_I_set_an_AutoProperty_MoreThanOnceInTheSamePublicMethod()
        {
            var notify = new NotifyMoreThanOnceInTheSamePublicMethod();
            var propertiesThatChanged = notify.ObservePropertyChanges();

            notify.MoreThanOnceInTheSamePublicMethod();

            "it should only notify ONCE that the Property changed"
                .AssertThat(propertiesThatChanged, Is.EquivalentTo(new[] { "AutoProperty" }));
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

            public void StopTheCompilerFromComplaining() { PropertyChanged(null, null); }
        }
    }
}
