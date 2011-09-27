using NUnit.Framework;

namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static class NUnitAssertionExtensions
    {
        public static string AssertThat<T>(this string message, T expected, NUnit.Framework.Constraints.IResolveConstraint constraint)
        {
            Assert.That(expected, constraint, message);
            return message;
        }
    }

}
