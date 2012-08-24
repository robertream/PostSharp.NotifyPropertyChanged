using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;

namespace PostSharp.NotifyPropertyChanged
{
    [Serializable]
    [AspectTypeDependency(AspectDependencyAction.Require, typeof(NotifyPropertyChangedAttribute))]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Multicast)]
    public sealed class CascadeingAttribute : InstanceLevelAspect
    {
        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);

            ObservedPropertyMap = type.SelectInstencePropertiesOf<INotifyPropertyChanged>().ToDictionary(property => property.Name, PropertyDependencyMap.For);
        }

        public override void RuntimeInitializeInstance()
        {
            base.RuntimeInitializeInstance();

            ObservedPropertyHandlers = ObservedPropertyMap.ToDictionary(entry => entry.Key, entry => new PropertyChangedHandler { NotifyChangesFor = NotifyChangesForMethod, Map = entry.Value, });
        }

        [ImportMember("NotifyChangesFor", IsRequired = true, Order = ImportMemberOrder.AfterIntroductions)]
        public Action<string> NotifyChangesForMethod;

        [OnLocationSetValueAdvice, MethodPointcut("ObservedInstanceProperties")]
        public void OnPropertySet(LocationInterceptionArgs args)
        {
            var oldInstance = args.GetCurrentValue() as INotifyPropertyChanged;
            var newInstance = args.Value as INotifyPropertyChanged;
            if (oldInstance != null)
                oldInstance.PropertyChanged -= ObservedPropertyHandlers[args.LocationName].Invoke;
            if (newInstance != null)
                newInstance.PropertyChanged += ObservedPropertyHandlers[args.LocationName].Invoke;
        }

        public IEnumerable<PropertyInfo> ObservedInstanceProperties(Type target)
        {
            return target.SelectInstencePropertiesOf<INotifyPropertyChanged>();
        }

        public Dictionary<string, Dictionary<string, string[]>> ObservedPropertyMap;
        public Dictionary<string, PropertyChangedHandler> ObservedPropertyHandlers = new Dictionary<string, PropertyChangedHandler>();
        public class PropertyChangedHandler
        {
            public Action<string> NotifyChangesFor;
            public Dictionary<string, string[]> Map = new Dictionary<string, string[]>();

            public void Invoke(object sender, PropertyChangedEventArgs eventArgs)
            {
                foreach (var propertyName in Map[eventArgs.PropertyName])
                    NotifyChangesFor(propertyName);
            }
        }
    }
}