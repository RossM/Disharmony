namespace Disharmony;

/// <summary>
///     Provides reflection helpers used when inspecting and identifying patch targets.
/// </summary>
public static class ReflectionExtensions
{
    /// <param name="member">The member to describe.</param>
    extension(MemberInfo member)
    {
        /// <summary>
        ///     Gets the member name qualified by its declaring type, in the form
        ///     <c>Namespace.DeclaringType::Member</c>.
        /// </summary>
        /// <remarks>
        ///     If the member has no declaring type, the result starts with <c>::</c>. Parameter types are not included.
        /// </remarks>
        public string FullName => $"{member.DeclaringType?.FullName}::{member.Name}";
    }

    /// <param name="method">The method or constructor to inspect.</param>
    extension(MethodBase method)
    {
        /// <summary>
        ///     Gets a value indicating whether the method's calling convention includes an instance argument.
        /// </summary>
        internal bool HasThis => (method.CallingConvention & CallingConventions.HasThis) != 0;

        /// <summary>
        ///     Gets the generated <c>MoveNext</c> method that implements an iterator or asynchronous method.
        /// </summary>
        /// <returns>
        ///     The state machine's <c>MoveNext</c> method, or <see langword="null" /> if <paramref name="method" /> is
        ///     not marked with <see cref="IteratorStateMachineAttribute" /> or <see cref="AsyncStateMachineAttribute" />,
        ///     or the generated type has no <c>MoveNext</c> method.
        /// </returns>
        public MethodInfo? GetStateMachineImplementation()
        {
            // Check if the method is an iterator state machine wrapper. If so, look at the iterator's MoveNext method.
            Type? stateMachineType = method.GetCustomAttribute<IteratorStateMachineAttribute>()?.StateMachineType ??
                                     method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
            return stateMachineType?.GetMethod("MoveNext", AccessTools.all);
        }
    }

    extension(Type type)
    {
        /// <summary>
        ///     Gets a value indicating whether the type is a managed reference or an unmanaged pointer.
        /// </summary>
        internal bool IsPointerLike => type.IsByRef || type.IsPointer;

        /// <summary>
        ///     Gets a value indicating whether Disharmony treats the type as a pointer-compatible numeric type.
        /// </summary>
        internal bool IsPointerCompatibleNumeric =>
            type == typeof(int) || type == typeof(uint) || type == typeof(IntPtr) || type == typeof(UIntPtr);

        /// <summary>
        ///     Gets the element type of a managed reference, or the original type if it is not a managed reference.
        /// </summary>
        internal Type NoRefType => type.IsByRef ? type.GetElementType()! : type;

        /// <summary>
        ///     Gets the type used for an invocation receiver: value types by reference and reference types unchanged.
        /// </summary>
        internal Type CallableType => type.IsValueType ? type.MakeByRefType() : type;

        /// <summary>
        ///     Gets a value indicating whether the type appears to be a compiler-generated closure type.
        /// </summary>
        internal bool IsClosureType
        {
            get
            {
                type = type.NoRefType;
                return type.Name.StartsWith("<>c") && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
            }
        }
    }

    extension(ILGenerator generator)
    {
        internal void Emit(OpCode opCode, MethodBaseInvocation invocation)
        {
            switch (invocation)
            {
                case MethodInvocation method: generator.Emit(opCode, method.MethodInfo); break;
                case ConstructorInvocation constructor: generator.Emit(opCode, constructor.ConstructorInfo); break;
                default: throw new ArgumentOutOfRangeException(nameof(invocation));
            }
        }
    }
}
