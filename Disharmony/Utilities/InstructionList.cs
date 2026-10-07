using System.Collections;

namespace Disharmony.Utilities;

/// <summary>
///     A simple box class to hold a value of type T. This is used to create a location for a value on the managed heap, allowing it to be passed by reference in IL code.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class Box<T>
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    public T value = default!;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}

internal class InstructionList(ILGenerator generator) : IEnumerable<CodeInstruction>
{
    public readonly List<CodeInstruction> instructions = [];
    public List<LocalTrackerBuilder> locals = [];
    public readonly ILGenerator generator = generator;

    public IEnumerator<CodeInstruction> GetEnumerator() => instructions.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(CodeInstruction instruction) => instructions.Add(instruction);

    // ReSharper disable once ParameterHidesMember
    public void AddRange(IEnumerable<CodeInstruction> instructions) => this.instructions.AddRange(instructions);

    public void EmitLocalInitializer(LocalTrackerBuilder local)
    {
        Type type = local.Type;

        if (type.IsByRef)
        {
            // We need a managed reference to a location that stores of the value of the correct type,
            // and for safety it needs to be a stored on the managed heap so that it won't go out of
            // scope when the function returns. We allocate a Box<T> object and take a reference to
            // its value field.

            // Emitted code:
            //      var box = new Box<T>();
            //      local = &box.value;
            var boxType = typeof(Box<>).MakeGenericType(type.NoRefType);
            var constructor = boxType.GetConstructor([]);
            var field = boxType.GetField(nameof(Box<>.value));
            Add(new(OpCodes.Newobj, constructor));
            Add(new(OpCodes.Ldflda, field));
            Add(local.Store());
        }
        else if (type.IsClass || type.IsInterface)
        {
            Add(new(OpCodes.Ldnull));
            Add(local.Store());
        }
        else if (type.IsPrimitive || type.IsEnum)
        {
            var underlyingType = type.IsEnum ? type.GetEnumUnderlyingType() : type;

            if (underlyingType == typeof(float))
                Add(new(OpCodes.Ldc_R4, (float)0));
            else if (underlyingType == typeof(double))
                Add(new(OpCodes.Ldc_R8, (double)0));
            else if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
                Add(new(OpCodes.Ldc_I8, (long)0));
            else
                Add(new(OpCodes.Ldc_I4_0));

            Add(local.Store());
        }
        else if (type.IsValueType)
        {
            Add(local.Load(true));
            Add(new(OpCodes.Initobj, type));
        }
        else
        {
            throw new NotImplementedException($"targetType {type}");
        }
    }

    public LocalTrackerBuilder AddLocal(Type type)
    {
        var local = new LocalTrackerBuilder(generator.DeclareLocal(type));
        locals.Add(local);
        return local;
    }
}
