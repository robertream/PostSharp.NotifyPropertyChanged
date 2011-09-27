using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System.Collections.Generic;

namespace PostSharp.NotifyPropertyChanged
{
    [Serializable]
    [IntroduceInterface(typeof(INotifyPropertyChanged), OverrideAction = InterfaceOverrideAction.Ignore)]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Multicast)]
    public sealed class NotifyPropertyChangedAttribute : InstanceLevelAspect, INotifyPropertyChanged
    {
        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);

            var publicProperties =
                type.GetProperties()
                    .Where(property=> property.CanRead && 
                                      property.GetGetMethod() != null)
                    .ToArray();

            var directDependencies =
                from property in publicProperties
                from dependency in ReflectionSearch.GetMethodsUsingDeclaration(property.GetGetMethod())
                join dependencyProperty in publicProperties on dependency.UsingMethod equals dependencyProperty.GetGetMethod()
                let dependentPropertyName = dependencyProperty.Name
                group dependentPropertyName by property.Name into propertyDependencies
                select propertyDependencies;

            PropertyDependecies = directDependencies.ToDictionary(dependencies => dependencies.Key, dependencies => new HashSet<string>(dependencies));

            var stack = new Stack<string>();
            foreach (var dependencies in PropertyDependecies)
            {
                var propertyDependencies = dependencies.Value;
                foreach (var dependency in dependencies.Value)
                    stack.Push(dependency);
                while (stack.Count > 0)
                {
                    var dependentProperty = stack.Pop();
                    if (!PropertyDependecies.ContainsKey(dependentProperty))
                        continue;
                    var propertiesRead = PropertyDependecies[dependentProperty];
                    foreach (var property in propertiesRead)
                    {
                        if (propertyDependencies.Contains(property))
                            continue;

                        propertyDependencies.Add(property);
                        stack.Push(property);
                    }
                }
            }
        }

        public Dictionary<string, HashSet<string>> PropertyDependecies;

        [ImportMember("OnPropertyChanged", IsRequired = false, Order = ImportMemberOrder.AfterIntroductions)]
        public Action<string> OnPropertyChangedMethod;

        [IntroduceMember(Visibility = Visibility.Family, IsVirtual = true, OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this.Instance, new PropertyChangedEventArgs(propertyName));
        }

        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public event PropertyChangedEventHandler PropertyChanged;

        private IEnumerable<string> _DependenciesFor(string property)
        {
            HashSet<string> dependencies;
            if (PropertyDependecies.TryGetValue(property, out dependencies))
                foreach (var dependency in dependencies)
                    yield return dependency;
            yield return property;
        }

        [OnLocationSetValueAdvice, MulticastPointcut(Targets = MulticastTargets.Property, Attributes = MulticastAttributes.Public | MulticastAttributes.Instance | MulticastAttributes.NonAbstract)]
        public void OnPropertySet(LocationInterceptionArgs args)
        {
            if (args.Value == args.GetCurrentValue())
                return;

            args.ProceedSetValue();
            foreach (var dependency in _DependenciesFor(args.LocationName))
                if (NumberOfPublicMethodsExecuting > 0)
                    PropertiesThatHaveChanged.Add(dependency);
                else
                    OnPropertyChangedMethod(dependency);

        }

        public HashSet<string> PropertiesThatHaveChanged { get; set; }
        public int NumberOfPublicMethodsExecuting { get; set; }

        static public IEnumerable<MethodInfo> PublicInstanceMethods(Type target)
        {
            return from method in target.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                   where method.DeclaringType != typeof(object)
                   where !method.IsSpecialName
                   select method;
        }

        [OnMethodEntryAdvice, MethodPointcut("PublicInstanceMethods")]
        public void OnMethodEntry(MethodExecutionArgs args)
        {
            NumberOfPublicMethodsExecuting++;
            if (NumberOfPublicMethodsExecuting == 1)
                if (PropertiesThatHaveChanged == null)
                    PropertiesThatHaveChanged = new HashSet<string>();
        }

        [OnMethodExitAdvice(Master = "OnMethodEntry")]
        public void OnMethodExit(MethodExecutionArgs args)
        {
            NumberOfPublicMethodsExecuting--;
            if (NumberOfPublicMethodsExecuting == 0 && PropertiesThatHaveChanged.Count > 0)
            {
                foreach(var propertyName in PropertiesThatHaveChanged)
                    OnPropertyChangedMethod(propertyName);
                PropertiesThatHaveChanged.Clear();
           }
        }
    }
}
