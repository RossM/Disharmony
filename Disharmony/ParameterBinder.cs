using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Disharmony;

internal enum BindingType
{
    /// <summary>
    ///     Access to a method call's argument.
    /// </summary>
    Argument,

    /// <summary>
    ///    Access to a method call's argument list as an array.
    /// </summary>
    ArgumentArray,

    /// <summary>
    ///     Access to a method call's instance parameter.
    /// </summary>
    Instance,

    /// <summary>
    ///     Access to the result of calling the method.
    /// </summary>
    Result,

    /// <summary>
    ///     Access to a local state variable.
    /// </summary>
    State,

    /// <summary>
    ///     Gets a delegate based on a given MethodInfo.
    /// </summary>
    Delegate,

    /// <summary>
    ///     Gets the exception thrown by the method.
    /// </summary>
    Exception,

    /// <summary>
    ///     Gets a static field.
    /// </summary>
    StaticField,

    /// <summary>
    ///     Gets a MemberInfo object.
    /// </summary>
    MemberInfo,
}

/// <summary>
///     A helper class that analyzes patch method parameters and determines how their values should be emitted in code
///     generation.
/// </summary>
/// <remarks>
///     For patches targeting a compiler-generated iterator state machine, <paramref name="target" /> refers to
///     the declared target, while <paramref name="inner" /> refers to the compiler-generated <c>MoveNext</c> method.
/// </remarks>
/// <param name="target">The declared target of the patch.</param>
/// <param name="outer">The outer method being patched.</param>
/// <param name="inner">The inner invocation being patched, or <see cref="EmptyInvocation" /> for an outer patch.</param>
/// <param name="patchKind">The patch type.</param>
/// <param name="stateGroupKey">A string for grouping together <see cref="StateAttribute">state</see> parameters.</param>
internal class ParameterBinder(
    Invocation target,
    Invocation outer,
    Invocation inner,
    PatchKind patchKind,
    PatchOptions options,
    string stateGroupKey)
{
    private bool IsInfix => inner is not EmptyInvocation;
    private bool IsStateMachine => outer != target;

    private bool AlwaysRun => (options & PatchOptions.AlwaysRun) != 0;
    private bool AllowUnsafe => (options & PatchOptions.AllowUnsafe) != 0;

    private const string ReadonlyAttributeName = "System.Runtime.CompilerServices.IsReadOnlyAttribute";
    private const string ThisRegexPattern = "^<>[\\d+]__this$";

    public ParameterBinding Bind(ParameterInfo parameter)
    {
        var parameterName = parameter.Name;

        var parameterAttributes = parameter.GetCustomAttributes().ToList();
        ParameterBindingAttribute? parameterBindingAttribute;
        try
        {
            parameterBindingAttribute = parameterAttributes.OfType<ParameterBindingAttribute>().SingleOrDefault();
        }
        catch (InvalidOperationException e)
        {
            throw new ParameterBindingException(parameterName, "Multiple parameter binding attributes", e);
        }

        Scope scope = (parameterBindingAttribute?.Scope ?? Scope.Any) switch
        {
            Scope.Any => IsInfix ? Scope.Inner : Scope.Outer,
            Scope.Inner => Scope.Inner,
            Scope.Outer => Scope.Outer,
            _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null),
        };
        Invocation invocation = scope switch
        {
            Scope.Inner => inner,
            Scope.Outer => target,
            _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null),
        };

        if (invocation is EmptyInvocation)
            throw new ParameterBindingException(parameterName, "Invalid scope");

        switch (parameterBindingAttribute)
        {
            case ArgumentAttribute { Index: int index }: return BindArgumentByIndex(parameter, invocation, scope, index);

            case ArgumentAttribute { Name: { } name, Scope: var attributeScope }:
                return BindArgumentByName(parameter, name, attributeScope);
            case ArgumentAttribute { Scope: var attributeScope } when parameterName is not null:
                return BindArgumentByName(parameter, parameterName, attributeScope);

            case ArgumentsAttribute: return BindArgumentArray(parameter, scope);

            case InstanceAttribute: return BindInstance(parameter, invocation, scope);

            case ReturnValueAttribute: return BindReturnValue(parameter, invocation, scope);

            case StateAttribute { Key: { } key }: return BindState(parameter, key);
            case StateAttribute when parameterName is not null: return BindState(parameter, parameterName);

            case FieldAttribute { Name: { } name, Scope: var attributeScope, Type: var type }:
                return BindFieldByName(parameter, name, attributeScope, type);
            case FieldAttribute { Scope: var attributeScope, Type: var type } when parameterName is not null:
                return BindFieldByName(parameter, parameterName.StartsWith("___") ? parameterName[3..] : parameterName, attributeScope, type);

            case BaseMethodAttribute: return BindBaseMethod(parameter, invocation, scope);

            case MethodAttribute { Name: { } name, MemberType: var memberType, VirtualCall: var virtualCall, Type: var type }:
                return BindMethod(parameter, invocation, scope, name, memberType, virtualCall, type);
            case MethodAttribute { MemberType: var memberType, VirtualCall: var virtualCall, Type: var type } when parameterName is not null:
                return BindMethod(parameter, invocation, scope, parameterName, memberType, virtualCall, type);

            case ExceptionAttribute: return BindException(parameter);

            case MemberInfoAttribute: return BindMemberInfo(parameter, invocation, scope);

            case null: break;

            default: throw new NotSupportedException();
        }

        switch (parameterName)
        {
            case "__caller":
            {
                if (!IsInfix)
                    throw new ParameterBindingException(parameterName, "Can only be used with inner patches");
                return BindInstance(parameter, outer, Scope.Outer);
            }

            case "__instance": return BindInstance(parameter, invocation, scope);

            case "__result": return BindReturnValue(parameter, invocation, scope);

            case "__state": return BindState(parameter, parameterName);

            case "__base": return BindBaseMethod(parameter, invocation, scope);

            case "__exception": return BindException(parameter);

            case "__args": return BindArgumentArray(parameter, scope);

            case var _ when parameterName?.StartsWith("___") is true: return BindFieldByName(parameter, parameterName[3..], Scope.Any, null);

            case var _ when parameterName?.StartsWith("__") is true:
                throw new ParameterBindingException(parameterName, "Unrecognized special parameter name");

            case not null: return BindArgumentByName(parameter, parameterName, Scope.Any);

            default: throw new ParameterBindingException(null, "Missing parameter name");
        }
    }

    private ParameterBinding BindArgumentByIndex(ParameterInfo parameter, Invocation invocation, Scope scope, int index)
    {
        if (invocation.HasThis)
            index++;

        try
        {
            if (IsStateMachine && scope == Scope.Outer)
                return BindArgumentByName(parameter, target.ParameterNames[index], scope);

            Validate(parameter, invocation.ParameterTypes[index], scope, "parameter");
            return new() { parameter = parameter, bindingType = BindingType.Argument, scope = scope, index = index };
        }
        catch (IndexOutOfRangeException e)
        {
            throw new ParameterBindingException(parameter.Name, "Index is out of range", e);
        }
    }

    private static ParameterBinding BindArgumentArray(ParameterInfo parameter, Scope scope)
    {
        if (parameter.ParameterType.IsByRef)
            throw new ParameterBindingException(parameter.Name, "[Arguments] cannot be bound to a 'ref' parameter");
        ValidateCast(parameter.ParameterType, typeof(object[]), parameter.Name);
        return new() { parameter = parameter, bindingType = BindingType.ArgumentArray, scope = scope };
    }

    private ParameterBinding BindState(ParameterInfo parameter, string key)
    {
        if (IsStateMachine)
            throw new NotSupportedException("Not supported for state machine methods");

        string stateKey = $"{stateGroupKey}#{key}";

        // ValidateCast not needed, the type will be checked in StateBuilder
        return new() { parameter = parameter, bindingType = BindingType.State, scope = Scope.Outer, stateKey = stateKey };
    }

    private ParameterBinding BindBaseMethod(ParameterInfo parameter, Invocation invocation, Scope scope)
    {
        if (IsStateMachine && scope == Scope.Outer)
            throw new ParameterBindingException(parameter.Name, "Not supported for state machine methods");
        if (invocation is not MethodInvocation method || method.IsStatic)
            throw new ParameterBindingException(parameter.Name, "Must be an instance method");

        ValidateCast(typeof(Delegate), parameter.ParameterType, parameter.Name);

        // Validate the delegate type has the right parameter types
        ValidateInvoke(parameter, method.MethodInfo);

        MethodInfo? baseMethod = null;
        for (Type? parent = method.InstanceType.BaseType; parent != typeof(object) && parent != null; parent = parent.BaseType)
        {
            ParameterInfo[] parameters = method.MethodInfo.GetParameters();
            baseMethod = parent.GetMethod(method.MethodInfo.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, parameters.Types(), null);
            if (baseMethod != null)
                break;
        }

        if (baseMethod is null)
            throw new ParameterBindingException(parameter.Name, "Base method not found");
        if (baseMethod.IsAbstract)
            throw new ParameterBindingException(parameter.Name, "Base method is abstract");

        return new() { parameter = parameter, bindingType = BindingType.Delegate, scope = scope, memberInfo = baseMethod };
    }

    private ParameterBinding BindMethod(
        ParameterInfo parameter,
        Invocation invocation,
        Scope scope,
        string name,
        MemberType memberType,
        bool allowVirtual,
        Type? type)
    {
        var instanceType = invocation.InstanceType;

        ValidateCast(typeof(Delegate), parameter.ParameterType, parameter.Name);

        var invoke = parameter.ParameterType.GetMethod("Invoke") ??
                     throw new ParameterBindingException(parameter.Name, "Delegate.Invoke not found");

        Type[]? parameterTypes = memberType switch
        {
            MemberType.Any or MemberType.Constructor or MemberType.Method => ReflectionTools.WrapParameterTypes(invoke),
            _ => null,
        };

        var results = ReflectionTools.GetMembers(type ?? instanceType, name, memberType, parameterTypes,
            null, true);

        if (results is not [MethodInfo methodInfo])
            throw new ParameterBindingException(parameter.Name, "Method not found");

        if (!methodInfo.IsStatic)
        {
            if (invocation.IsStatic)
                throw new ParameterBindingException(parameter.Name, "Instance required");
            if (!methodInfo.DeclaringType!.IsAssignableFrom(instanceType))
                throw new ParameterBindingException(parameter.Name, "Instance type mismatch");
        }

        // Getting the instance for an iterator state machine isn't implemented for BindingType.Delegate
        if (IsStateMachine && scope == Scope.Outer && !methodInfo.IsStatic)
            throw new ParameterBindingException(parameter.Name, "[Method] is not supported for iterator state machines");

        bool isReadonly = methodInfo.CustomAttributes.Any(a => a.AttributeType.FullName == ReadonlyAttributeName) ||
                          instanceType.CustomAttributes.Any(a => a.AttributeType.FullName == ReadonlyAttributeName);

        // Calling a struct method through a delegate requires boxing the struct, which means that any writes
        // by the method won't affect the original struct.
        if (instanceType.IsValueType && !methodInfo.IsStatic && !isReadonly && !AllowUnsafe)
            throw new ParameterBindingException(parameter.Name, "[Method] is not supported for non-static methods on structs");

        ValidateInvoke(parameter, methodInfo);

        return new()
        {
            parameter = parameter, bindingType = BindingType.Delegate, scope = scope, memberInfo = methodInfo,
            useVirtualDispatch = methodInfo.IsVirtual && allowVirtual,
        };
    }

    private static void ValidateInvoke(ParameterInfo parameter, MethodInfo methodInfo)
    {
        var delegateInvoke = parameter.ParameterType.GetMethod("Invoke") ??
                             throw new ParameterBindingException(parameter.Name, "Delegate.Invoke not found");
        if (!delegateInvoke.GetParameters().Types().SequenceEqual(methodInfo.GetParameters().Types()))
            throw new ParameterBindingException(parameter.Name, "Parameter type mismatch");
        if (delegateInvoke.ReturnType != methodInfo.ReturnType)
            throw new ParameterBindingException(parameter.Name, "Return type mismatch");
    }

    private ParameterBinding BindReturnValue(ParameterInfo parameter, Invocation invocation, Scope scope)
    {
        if (invocation.ReturnType == typeof(void))
            throw new ParameterBindingException(parameter.Name, "Method returns void");
        if (patchKind == PatchKind.Prefix && AlwaysRun)
            throw new ParameterBindingException(parameter.Name, "Binding return value not allowed for Prefix with AlwaysRun option");
        ValidateCast(parameter, invocation.ReturnType);
        return new() { parameter = parameter, bindingType = BindingType.Result, scope = scope };
    }

    private ParameterBinding BindInstance(ParameterInfo parameter, Invocation invocation, Scope scope)
    {
        if (IsStateMachine && scope == Scope.Outer)
        {
            if (target.IsStatic)
                throw new ParameterBindingException(parameter.Name, "Method is static");
            if (IsWriteableRef(parameter))
                throw new ParameterBindingException(parameter.Name,
                    "Accessing 'this' by reference is not supported for iterator state machine methods");

            var thisField = GetThisField(outer.InstanceType);
            Validate(parameter, thisField.FieldType, scope, "instance");
            return new() { parameter = parameter, bindingType = BindingType.Instance, scope = scope, fields = [thisField] };
        }

        if (invocation.IsStatic)
            throw new ParameterBindingException(parameter.Name, "Method is static");

        if (!invocation.InstanceType.IsValueType && invocation is not FieldInvocation)
            ValidateReference(parameter, invocation.InstanceType, scope, "instance");
        ValidateCast(parameter, invocation.InstanceType);
        return new() { parameter = parameter, bindingType = BindingType.Instance, scope = scope };
    }

    private ParameterBinding BindArgumentByName(ParameterInfo parameter, string name, Scope scope)
    {
        // Look in target parameters
        if (scope is Scope.Inner or Scope.Any)
            if (BindArgumentByName(parameter, name, inner, Scope.Inner) is { } parameterBinding)
                return parameterBinding;

        // Look in caller parameters
        if (scope is Scope.Outer or Scope.Any)
            if (BindArgumentByName(parameter, name, outer, Scope.Outer) is { } parameterBinding)
                return parameterBinding;

        // Look in closure fields
        if (scope is Scope.Inner or Scope.Any)
            if (BindClosureByName(parameter, name, inner.ParameterTypes, Scope.Inner) is { } parameterBinding)
                return parameterBinding;

        // Look in closure fields
        if (scope is Scope.Outer or Scope.Any)
            if (BindClosureByName(parameter, name, outer.ParameterTypes, Scope.Outer) is { } parameterBinding)
                return parameterBinding;

        throw new ParameterBindingException(parameter.Name, "Parameter not found");
    }

    private ParameterBinding? BindArgumentByName(ParameterInfo parameter, string name, Invocation invocation, Scope scope)
    {
        if (scope == Scope.Outer && IsStateMachine)
        {
            var iteratorType = invocation.InstanceType;
            var field = iteratorType.GetField(name, AccessTools.all);
            if (field != null)
            {
                Validate(parameter, field.FieldType, scope, "parameter");
                return new() { parameter = parameter, bindingType = BindingType.Instance, scope = scope, fields = [field] };
            }

            if (TryGetThisField(iteratorType, out var thisField) && thisField.FieldType.IsClosureType)
            {
                var type = thisField.FieldType.NoRefType;
                field = type.GetField(name, AccessTools.all);
                if (field != null)
                {
                    Validate(parameter, field.FieldType, scope, "parameter");
                    return new()
                    {
                        parameter = parameter, bindingType = BindingType.Instance, scope = scope, fields = [thisField, field],
                    };
                }
            }

            throw new ParameterBindingException(parameter.Name, "Parameter not found");
        }

        int index = Array.FindIndex(invocation.ParameterNames, p => p == name);
        if (index >= 0)
        {
            Validate(parameter, invocation.ParameterTypes[index], scope, "parameter");
            return new() { parameter = parameter, bindingType = BindingType.Argument, scope = scope, index = index };
        }

        return null;
    }

    private ParameterBinding? BindClosureByName(
        ParameterInfo parameter,
        string name,
        Type[] parameterTypes,
        Scope scope)
    {
        int closureIndex = Array.FindLastIndex(parameterTypes, p => p.IsClosureType);
        if (closureIndex >= 0)
        {
            var type = parameterTypes[closureIndex].NoRefType;

            var field = type.GetField(name, AccessTools.all);

            if (field != null)
            {
                ValidateCast(parameter, field.FieldType);
                return new()
                {
                    parameter = parameter,
                    bindingType = BindingType.Argument,
                    scope = scope,
                    index = closureIndex,
                    fields = [field],
                };
            }
        }

        return null;
    }

    private ParameterBinding BindFieldByName(ParameterInfo parameter, string name, Scope scope, Type? type)
    {
        // Look in inner instance fields
        if (scope is Scope.Inner or Scope.Any)
            if (BindFieldByName(parameter, name, inner, Scope.Inner, type) is { } parameterBinding)
                return parameterBinding;

        // Look in outer instance fields
        if (scope is Scope.Outer or Scope.Any)
            if (BindFieldByName(parameter, name, outer, Scope.Outer, type) is { } parameterBinding)
                return parameterBinding;

        throw new ParameterBindingException(parameter.Name, "Field not found");
    }

    private ParameterBinding? BindFieldByName(ParameterInfo parameter, string name, Invocation invocation, Scope scope, Type? type)
    {
        Type curType = invocation.InstanceType;
        List<FieldInfo> fields = [];
        if (scope == Scope.Outer && IsStateMachine && !invocation.IsStatic)
        {
            var thisField = GetThisField(curType);
            curType = thisField.FieldType;
            fields.Add(thisField);
        }

        var results = ReflectionTools.GetMembers(type ?? curType, name, MemberType.Getter, null, null, searchBaseTypes: true);
        if (results is not [FieldInfo])
            results = ReflectionTools.GetMembers(curType, $"<{name}>k__BackingField", MemberType.Getter, null, null, searchBaseTypes: true);
        if (results is not [FieldInfo])
            return null;

        var field = (FieldInfo)results[0];

        if (field.IsStatic)
        {
            ValidateCast(parameter, field.FieldType);
            return new() { parameter = parameter, bindingType = BindingType.StaticField, scope = scope, fields = [field] };
        }

        if (!invocation.IsStatic)
        {
            if (!field.DeclaringType!.IsAssignableFrom(curType))
                throw new ParameterBindingException(parameter.Name, "Instance type mismatch");

            fields.Add(field);
            ValidateCast(parameter, field.FieldType);
            return new() { parameter = parameter, bindingType = BindingType.Instance, scope = scope, fields = [.. fields] };
        }

        return null;
    }

    private ParameterBinding BindException(ParameterInfo parameter)
    {
        if (patchKind != PatchKind.Postfix || !AlwaysRun)
            throw new ParameterBindingException(parameter.Name, "Accessing exception is only supported for Postfix with AlwaysRun option");
        ValidateCast(parameter, typeof(Exception));
        return new() { parameter = parameter, bindingType = BindingType.Exception, scope = Scope.Any };
    }

    private ParameterBinding BindMemberInfo(ParameterInfo parameter, Invocation invocation, Scope scope)
    {
        if (parameter.ParameterType.IsByRef)
            throw new ParameterBindingException(parameter.Name, "[MemberInfo] cannot be bound to a 'ref' parameter");

        (MemberInfo memberInfo, Type validationType) = invocation switch
        {
            FieldInvocation fieldInvocation => ((MemberInfo)fieldInvocation.FieldInfo, typeof(FieldInfo)),
            MethodInvocation methodInvocation => ((MemberInfo)methodInvocation.MethodInfo, typeof(MethodInfo)),
            ConstructorInvocation constructorInvocation => ((MemberInfo)constructorInvocation.ConstructorInfo, typeof(ConstructorInfo)),
            _ => throw new ParameterBindingException(parameter.Name, "[MemberInfo] unsupported for this target type"),
        };

        ValidateCast(parameter, validationType);
        return new() { parameter = parameter, bindingType = BindingType.MemberInfo, scope = scope, memberInfo = memberInfo };
    }

    private static FieldInfo GetThisField(Type iteratorType)
    {
        return iteratorType.GetFields(AccessTools.all).Single(f => Regex.IsMatch(f.Name, ThisRegexPattern));
    }

    private static bool TryGetThisField(Type iteratorType, [NotNullWhen(true)] out FieldInfo? field)
    {
        field = iteratorType.GetFields(AccessTools.all).SingleOrDefault(f => Regex.IsMatch(f.Name, ThisRegexPattern));
        return field != null;
    }

    private static void ValidateCast(Type to, Type from, string? parameterName)
    {
        if (!to.NoRefType.IsAssignableFrom(from.NoRefType))
            throw new InvalidCastException($"{parameterName ?? "<unknown>"}: Can't convert {from.FullName} to {to.FullName}");
    }

    private void ValidateCast(ParameterInfo parameter, Type from)
    {
        Type to = parameter.ParameterType;
        string parameterName = parameter.Name ?? "<unknown>";

        if (AllowUnsafe && !to.NoRefType.IsValueType && !from.NoRefType.IsValueType)
            return;

        if (to.IsByRef && from.NoRefType.IsValueType)
        {
            if (to.NoRefType != from.NoRefType)
                throw new InvalidCastException($"{parameterName}: Can't convert {from.FullName} to {to.FullName}");
        }
        else if (parameter.IsIn)
        {
            if (!to.NoRefType.IsAssignableFrom(from.NoRefType))
                throw new InvalidCastException($"{parameterName}: Can't convert {from.FullName} to 'in' {to.FullName}");
        }
        else if (parameter.IsOut)
        {
            if (!from.NoRefType.IsAssignableFrom(to.NoRefType))
                throw new InvalidCastException($"{parameterName}: Can't convert {from.FullName} to 'out' {to.FullName}");
        }
        else if (to.IsByRef)
        {
            if (to.NoRefType != from.NoRefType)
                throw new InvalidCastException($"{parameterName}: Can't convert {from.FullName} to 'ref' {to.FullName}");
        }
        else
        {
            if (!to.NoRefType.IsAssignableFrom(from.NoRefType))
                throw new InvalidCastException($"{parameterName}: Can't convert {from.FullName} to {to.FullName}");
        }
    }

    private void ValidateReference(ParameterInfo parameter, Type type, Scope scope, string bindingType)
    {
        // Don't allow writing through a ref parameter to an argument of the outer method. This would
        // be wildly unreliable, as the compiler is free to copy those to locals any time it wants.
        if (IsWriteableRef(parameter) && !type.IsByRef && !AllowUnsafe)
        {
            if (scope == Scope.Outer && !(patchKind == PatchKind.Prefix && !IsInfix))
                throw new ParameterBindingException(parameter.Name,
                    $"{patchKind} can't access outer method {bindingType} by writeable reference");
            if (scope == Scope.Inner && !(patchKind == PatchKind.Prefix && IsInfix))
                throw new ParameterBindingException(parameter.Name,
                    $"{patchKind} can't access inner method {bindingType} by writeable reference");
        }
    }

    private void Validate(ParameterInfo parameter, Type type, Scope scope, string bindingType)
    {
        ValidateReference(parameter, type, scope, bindingType);
        ValidateCast(parameter, type);
    }

    private static bool IsWriteableRef(ParameterInfo parameter) => parameter.ParameterType.IsByRef && !parameter.IsIn;
}
