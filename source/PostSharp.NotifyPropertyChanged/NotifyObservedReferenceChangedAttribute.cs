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
    [AspectTypeDependency(AspectDependencyAction.Require, AspectDependencyPosition.After, typeof(NotifyPropertyChangedAttribute))]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Multicast)]
    public sealed class NotifyObservedReferenceChangedAttribute : InstanceLevelAspect
    {
        public Dictionary<string, Dictionary<string, string[]>> ObservedPropertyMap;
        public Dictionary<string, PropertyChangedHandler> ObservedPropertyHandlers;

        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);
            ObservedPropertyMap = ObservedReferenceProperties(type).ToDictionary(property => property.Name, PropertyDependencyMap.ForProperty);
        }

        public override void RuntimeInitializeInstance()
        {
            base.RuntimeInitializeInstance();
            ObservedPropertyHandlers = ObservedPropertyMap.ToDictionary(entry => entry.Key, PropertyChangedHandlerForProperty);
        }

        [ImportMember("NotifyChanges", IsRequired = true, Order = ImportMemberOrder.AfterIntroductions)]
        public Action<string[], bool> NotifyChangesMethod;

        [OnLocationSetValueAdvice, MethodPointcut("ObservedReferenceProperties")]
        public void OnObservedReferencePropertySet(LocationInterceptionArgs args)
        {
            var oldInstance = args.GetCurrentValue() as INotifyPropertyChanged;
            var newInstance = args.Value as INotifyPropertyChanged;
            if (oldInstance != null)
                oldInstance.PropertyChanged -= ObservedPropertyHandlers[args.LocationName].Invoke;
            if (newInstance != null)
                newInstance.PropertyChanged += ObservedPropertyHandlers[args.LocationName].Invoke;
            args.ProceedSetValue();
        }

        public IEnumerable<PropertyInfo> ObservedReferenceProperties(Type target)
        {
            return target.SelectInstencePropertiesOf<INotifyPropertyChanged>();
        }

        private PropertyChangedHandler PropertyChangedHandlerForProperty(KeyValuePair<string, Dictionary<string, string[]>> entry)
        {
            return new PropertyChangedHandler { NotifyChanges = NotifyChangesMethod, Map = entry.Value, };
        }

        public class PropertyChangedHandler
        {
            public Action<string[], bool> NotifyChanges;
            public Dictionary<string, string[]> Map = new Dictionary<string, string[]>();

            public void Invoke(object sender, PropertyChangedEventArgs eventArgs)
            {
                string[] strings;
                if (Map.TryGetValue(eventArgs.PropertyName, out strings))
                    NotifyChanges(strings, false);
            }
        }
    }
}