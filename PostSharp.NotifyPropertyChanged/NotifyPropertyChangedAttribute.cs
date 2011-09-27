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
        public Dictionary<string, HashSet<string>> PropertyDependecyMap;

        public IEnumerable<string> DependenciesFor(string property)
        {
            HashSet<string> dependencies;
            if (PropertyDependecyMap.TryGetValue(property, out dependencies))
                foreach (var dependency in dependencies)
                    yield return dependency;

            yield return property;
        }

        public override void CompileTimeInitialize(Type type, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(type, aspectInfo);

            PropertyDependecyMap = new PropertyDependency().MapFor(type);
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

        public int NumberOfPublicMethodsExecuting { get; set; }
        public HashSet<string> PropertiesThatHaveChanged { get; set; }

        [OnLocationSetValueAdvice, MulticastPointcut(Targets = MulticastTargets.Property, Attributes = MulticastAttributes.Public | MulticastAttributes.Instance | MulticastAttributes.NonAbstract)]
        public void OnPropertySet(LocationInterceptionArgs args)
        {
            if (args.Value == args.GetCurrentValue())
                return;

            args.ProceedSetValue();

            foreach (var dependency in DependenciesFor(args.LocationName))
            {
                if (NumberOfPublicMethodsExecuting > 0)
                    PropertiesThatHaveChanged.Add(dependency);
                else
                    OnPropertyChangedMethod(dependency);
            }

        }

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
            if (NumberOfPublicMethodsExecuting == 1 && PropertiesThatHaveChanged == null)
                PropertiesThatHaveChanged = new HashSet<string>();
        }

        [OnMethodExitAdvice(Master = "OnMethodEntry")]
        public void OnMethodExit(MethodExecutionArgs args)
        {
            NumberOfPublicMethodsExecuting--;
            if (NumberOfPublicMethodsExecuting == 0 && PropertiesThatHaveChanged.Count > 0)
            {
                foreach (var propertyName in PropertiesThatHaveChanged)
                    OnPropertyChangedMethod(propertyName);
                PropertiesThatHaveChanged.Clear();
            }
        }
    }
}
