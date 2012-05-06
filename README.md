# PostSharp.NotifyPropertyChanged 

PostSharp Auto-Magic INotifyPropertyChanged done right.

## Goals

Absent any tooling (such as this PostSharp aspect) there is *way* too much cruft associated with the most simple view models using INotifyPropertyChanged. 
Try to add any complex dependent calculated properties and the cruft can make your view models incoherent and unwieldy.
Incoherent and unwieldy classes of any kind tend to cause chronic testing and maintenance burdens.
The goal is to make simple as well as more complex view models as concise, yet coherent, as possible.

**PostSharp.NotifyPropertyChanged** will make the headaches of INotifyPropertyChanged go away by:

1. Using just one attribute to configure a class
2. Automatically notifying on property changes (and only if the value has *actually* changed)
3. Automatically notifying that dependent (i.e. calculated) properties changed as well
4. Automatically notifying *once* even though the property was set *more than once* from inside an instance method
5. Allowing for nested dependent properties *and* methods
6. Allowing for properties with backing fields (in case a property's `get` method needs to return a calculated value)

## Example

Given a class defined like so:

	[NotifyPropertyChanged]
	public class NotifyCalculatedProperty : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public decimal AutoProperty { get; set; }
		public decimal BackingField;
		public decimal PropertyWithBackingField
		{
			get { return BackingField * AutoProperty; }
			set { BackingField = value; }
		}
		public decimal CalculatedProperty { get { return ProtectedDependentProperty; } }
		protected decimal ProtectedDependentProperty { return PrivateDependentProperty; }
		private decimal PrivateDependentProperty { return PublicDependentMethod(); }
		public decimal PublicDependentMethod() { return ProtectedDependentMethod(); }
		protected decimal ProtectedDependentMethod() { return NestedDependentMethod(); }
		private decimal NestedDependentMethod() { return AutoProperty; }

		public void SetTheAutoPropertyMoreThanOnce()
		{
			AutoProperty = "once";
			AutoProperty = "twice";
			SomeOtherMethod();
		}

		private void SomeOtherMethod()
		{
			AutoProperty = "thrice";
		}
	}

When `SetTheAutoPropertyMoreThanOnce` is called, the `PropertyChanged` event will get raised three times: 
once for `AutoProperty`, once for `CalculatedProperty`, *and* once for `PropertyWithBackingField`.

## Note

Automatic notification for dependent properties only works with dependent *instance* properties and methods. 
Static properties and methods or instance properties and methods of *other* objects (including anonymous methods and closures) are **not** supported.