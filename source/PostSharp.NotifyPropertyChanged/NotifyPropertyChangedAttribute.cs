using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Extensibility;
using PostSharp.Reflection;

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
            PropertyDependecyMap = PropertyDependencyMap.For(type);
        }

        public Dictionary<string, string[]> PropertyDependecyMap;
        public int NumberOfPublicMethodsExecuting { get; set; }
        public HashSet<string> PropertiesThatHaveChanged { get; set; }

        public IEnumerable<string> DependenciesFor(string property)
        {
            string[] dependencies;
            if (PropertyDependecyMap.TryGetValue(property, out dependencies))
                foreach (var dependency in dependencies)
                    yield return dependency;
            yield return property;
        }

        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public event PropertyChangedEventHandler PropertyChanged;

        [IntroduceMember(Visibility = Visibility.Family, IsVirtual = true, OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(Instance, new PropertyChangedEventArgs(propertyName));
        }

        [ImportMember("OnPropertyChanged", IsRequired = false, Order = ImportMemberOrder.AfterIntroductions)]
        public Action<string> OnPropertyChangedMethod;

        [OnLocationSetValueAdvice, MulticastPointcut(Targets = MulticastTargets.Property, Attributes = MulticastAttributes.Public | MulticastAttributes.Instance | MulticastAttributes.NonAbstract)]
        public void OnPropertySet(LocationInterceptionArgs args)
        {
            if (args.Value == args.GetCurrentValue())
                return;
            args.ProceedSetValue();
            NotifyChangesFor(args.LocationName);
        }

        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrFail)]
        public void NotifyChangesFor(string propertyName)
        {
            foreach (var dependency in DependenciesFor(propertyName))
            {
                if (NumberOfPublicMethodsExecuting > 0)
                    PropertiesThatHaveChanged.Add(dependency);
                else
                    OnPropertyChangedMethod(dependency);
            }
        }

        [OnMethodEntryAdvice, MethodPointcut("PublicInstanceMethods")]
        public void OnMethodEntry(MethodExecutionArgs args)
        {
            NumberOfPublicMethodsExecuting++;
            if (NumberOfPublicMethodsExecuting == 1 && PropertiesThatHaveChanged == null)
                PropertiesThatHaveChanged = new HashSet<string>();
        }

        public IEnumerable<MethodInfo> PublicInstanceMethods(Type target)
        {
            return target.SelectPublicInstanceMethods();
        }

        [OnMethodExitAdvice(Master = "OnMethodEntry")]
        public void OnMethodExit(MethodExecutionArgs args)
        {
            NumberOfPublicMethodsExecuting--;
            if (NumberOfPublicMethodsExecuting > 0 || PropertiesThatHaveChanged.Count == 0)
                return;            
            foreach (var propertyName in PropertiesThatHaveChanged)
                OnPropertyChangedMethod(propertyName);
            PropertiesThatHaveChanged.Clear();
        }
    }
}