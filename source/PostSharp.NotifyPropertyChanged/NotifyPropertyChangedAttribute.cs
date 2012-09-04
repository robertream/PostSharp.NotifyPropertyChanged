using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public event PropertyChangedEventHandler PropertyChanged;

        [ImportMember("OnPropertyChanged", IsRequired = false, Order = ImportMemberOrder.AfterIntroductions)]
        public Action<string> OnPropertyChangedMethod;

        [IntroduceMember(Visibility = Visibility.Family, IsVirtual = true, OverrideAction = MemberOverrideAction.OverrideOrIgnore)]
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(Instance, new PropertyChangedEventArgs(propertyName));
        }

        [IntroduceMember(OverrideAction = MemberOverrideAction.OverrideOrFail)]
        public void NotifyChanges(string[] propertiesWithChanges, bool notifyDependentPropertiesOnly = false)
        {
            var propertyNames = notifyDependentPropertiesOnly ? DependenciesFor(propertiesWithChanges) : propertiesWithChanges.Concat(DependenciesFor(propertiesWithChanges));
            foreach (var propertyName in propertyNames.Distinct())
            {
                if (NumberOfPublicMethodsExecuting > 0)
                    PropertiesThatHaveChanged.Add(propertyName);
                else
                    OnPropertyChangedMethod(propertyName);
            }
        }

        [OnLocationSetValueAdvice, MulticastPointcut(Targets = MulticastTargets.Property, Attributes = MulticastAttributes.Public | MulticastAttributes.Instance | MulticastAttributes.NonAbstract)]
        public void OnPropertySet(LocationInterceptionArgs args)
        {
            if (args.Value == args.GetCurrentValue())
                return;
            args.ProceedSetValue();
            NotifyChanges(new[] { args.LocationName });
        }

        [OnMethodEntryAdvice, MethodPointcut("PublicInstanceMethods")]
        public void OnMethodEntry(MethodExecutionArgs args) { EnterMethod(); }

        [OnMethodExitAdvice(Master = "OnMethodEntry")]
        public void OnMethodExit(MethodExecutionArgs args) { ExitMethod(); }

        public IEnumerable<MethodInfo> PublicInstanceMethods(Type target)
        {
            return target.SelectPublicInstanceMethods();
        }

        public IEnumerable<string> DependenciesFor(params string[] properties)
        {
            var dependencies = new string[0];
            return from property in properties
                   where PropertyDependecyMap.TryGetValue(property, out dependencies)
                   from dependency in dependencies
                   select dependency;
        }

        private void EnterMethod()
        {
            NumberOfPublicMethodsExecuting++;
            if (NumberOfPublicMethodsExecuting == 1 && PropertiesThatHaveChanged == null)
                PropertiesThatHaveChanged = new HashSet<string>();
        }

        private void ExitMethod()
        {
            NumberOfPublicMethodsExecuting--;
            if (NumberOfPublicMethodsExecuting > 0 || PropertiesThatHaveChanged.Count == 0)
                return;

            var propertiesToNotify = PropertiesThatHaveChanged.ToArray();
            PropertiesThatHaveChanged.Clear();
            foreach (var propertyName in propertiesToNotify)
                OnPropertyChangedMethod(propertyName);
        }
    }
}