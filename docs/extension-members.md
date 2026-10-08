# Reading extension members in the API reference

Disharmony's fluent API and reflection helpers include extension methods and properties. Some entries in the generated reference use static method signatures or accessor names. The examples below show how to translate those entries into C# code.

## Extension methods

A `this` parameter identifies the object on which you can call an extension method. For example, the reference shows:

```csharp
public static PatchConfig Options(this PatchConfig patchConfig, PatchOptions options)
```

You can call `Options` on a `PatchConfig` as part of a fluent chain:

```csharp
PatchConfig configured = Patch.Postfix
    .Of(targetMethod)
    .With(patchMethod)
    .Options(PatchOptions.AlwaysRun);
```

Here, `targetMethod` and `patchMethod` are the `MethodInfo` objects for the method to patch and your postfix method.

## Extension properties

An entry named `get_...` represents a property getter. The property syntax below uses C# 14 extension properties. Use the property name after the dot in your C# code:

| Displayed reference entry | C# usage |
| --- | --- |
| [Patch.get_Prefix(PatchConfig)](xref:Disharmony.Patch.get_Prefix(Disharmony.PatchConfig)) | `configuration.Prefix` |
| [Patch.get_Postfix(PatchConfig)](xref:Disharmony.Patch.get_Postfix(Disharmony.PatchConfig)) | `configuration.Postfix` |
| [ReflectionExtensions.get_FullName(MemberInfo)](xref:Disharmony.ReflectionExtensions.get_FullName(System.Reflection.MemberInfo)) | `member.FullName` |

In these examples, `configuration` is a `PatchConfig` and `member` is a `MemberInfo`. Import the `Disharmony` namespace to use these extension members.

The static entry points `Patch.Prefix` and `Patch.Postfix` already appear as properties and use the syntax shown in the reference.
