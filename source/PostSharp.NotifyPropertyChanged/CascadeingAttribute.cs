using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public string[] ObservedCollections;
        public Dictionary<string, NotifyCollectionChangedEventHandler> ObservedCollectionHandlers;

        public Dictionary<string, Dictionary<string, string[]>> ObservedPropertyMap;
        public Dictionary<string, PropertyChangedHandler> ObservedPropertyHandlers;

        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);

            ObservedPropertyMap = ObservedViewModelProperties(type).ToDictionary(property => property.Name, PropertyDependencyMap.ForProperty);
            ObservedCollections = ObservedCollectionProperties(type).Select(property => property.Name).ToArray();
        }

        public override void RuntimeInitializeInstance()
        {
            base.RuntimeInitializeInstance();

            ObservedPropertyHandlers = ObservedPropertyMap.ToDictionary(entry => entry.Key, PropertyChangedHandlerForProperty);
            ObservedCollectionHandlers = ObservedCollections.ToDictionary(propertyName => propertyName, CollectionHandlerForProperty);
        }

        [ImportMember("NotifyChanges", IsRequired = true, Order = ImportMemberOrder.AfterIntroductions)]
        public Action<string[], bool> NotifyChangesMethod;

        [OnLocationSetValueAdvice, MethodPointcut("ObservedCollectionProperties")]
        public void OnObservedCollectionPropertySet(LocationInterceptionArgs args)
        {
            var oldInstance = args.GetCurrentValue() as INotifyCollectionChanged;
            var newInstance = args.Value as INotifyCollectionChanged;
            if (oldInstance != null)
                oldInstance.CollectionChanged -= ObservedCollectionHandlers[args.LocationName];
            if (newInstance != null)
                newInstance.CollectionChanged += ObservedCollectionHandlers[args.LocationName];
        }

        public IEnumerable<PropertyInfo> ObservedCollectionProperties(Type target)
        {
            return target.SelectInstencePropertiesOf<INotifyCollectionChanged>();
        }

        public NotifyCollectionChangedEventHandler CollectionHandlerForProperty(string propertyName)
        {
            return (@object, @event) => NotifyChangesMethod.Invoke(new[] { propertyName }, true);
        }

        [OnLocationSetValueAdvice, MethodPointcut("ObservedViewModelProperties")]
        public void OnObservedViewModelPropertySet(LocationInterceptionArgs args)
        {
            var oldInstance = args.GetCurrentValue() as INotifyPropertyChanged;
            var newInstance = args.Value as INotifyPropertyChanged;
            if (oldInstance != null)
                oldInstance.PropertyChanged -= ObservedPropertyHandlers[args.LocationName].Invoke;
            if (newInstance != null)
                newInstance.PropertyChanged += ObservedPropertyHandlers[args.LocationName].Invoke;
        }

        public IEnumerable<PropertyInfo> ObservedViewModelProperties(Type target)
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
                NotifyChanges(Map[eventArgs.PropertyName], false);
            }
        }
    }
}