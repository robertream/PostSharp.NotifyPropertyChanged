using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.Before, typeof(NotifyObservedReferenceChangedAttribute))]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Multicast)]
    public sealed class NotifyObservedCollectionChangedAttribute : InstanceLevelAspect
    {
        public string[] ObservedCollections;
        public Dictionary<string, NotifyCollectionChangedEventHandler> ObservedCollectionHandlers;

        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);
            ObservedCollections = ObservedCollectionProperties(type).Select(property => property.Name).ToArray();
        }

        public override void RuntimeInitializeInstance()
        {
            base.RuntimeInitializeInstance();
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
    }
}