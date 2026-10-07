namespace Disharmony;

/// <summary>
///     Provides helpers for constructing Harmony <see cref="CodeInstruction" /> instances.
/// </summary>
internal static class CodeInstructionExtensions
{
    extension(CodeInstruction inst)
    {
        /// <summary>
        ///     Creates a non-executable annotation that Disharmony includes in IL debug output.
        /// </summary>
        /// <param name="str">The annotation text.</param>
        /// <returns>A <see cref="OpCodes.Nop" /> instruction carrying the annotation text as its operand.</returns>
        internal static CodeInstruction Annotation(string str) => new(OpCodes.Nop, str);

        internal int PushCount()
        {
            return inst.opcode.StackBehaviourPush switch
            {
                StackBehaviour.Push0 => 0,
                StackBehaviour.Push1_push1 => 2,
                StackBehaviour.Varpush when inst.operand is MethodInfo method && method.ReturnType == typeof(void) => 0,
                StackBehaviour.Varpush when inst.opcode == OpCodes.Call && inst.operand is ConstructorInfo => 0,
                StackBehaviour.Varpush when inst.operand.GetType() == HarmonyInterface.InlineSignature &&
                                            (Type)HarmonyInterface.InlineSignature_ReturnType(inst.operand) == typeof(void) => 0,
                _ => 1,
            };
        }

        internal int PopCount(Type methodReturnType)
        {
            return inst.opcode.StackBehaviourPop switch
            {
                StackBehaviour.Pop0 => 0,
                StackBehaviour.Pop1 => 1,
                StackBehaviour.Pop1_pop1 => 2,
                StackBehaviour.Popi => 1,
                StackBehaviour.Popi_pop1 => 2,
                StackBehaviour.Popi_popi => 2,
                StackBehaviour.Popi_popi8 => 2,
                StackBehaviour.Popi_popi_popi => 3,
                StackBehaviour.Popi_popr4 => 2,
                StackBehaviour.Popi_popr8 => 2,
                StackBehaviour.Popref => 1,
                StackBehaviour.Popref_pop1 => 2,
                StackBehaviour.Popref_popi => 2,
                StackBehaviour.Popref_popi_popi => 3,
                StackBehaviour.Popref_popi_popi8 => 3,
                StackBehaviour.Popref_popi_popr4 => 3,
                StackBehaviour.Popref_popi_popr8 => 3,
                StackBehaviour.Popref_popi_popref => 3,
                StackBehaviour.Popref_popi_pop1 => 3,
                StackBehaviour.Varpop when inst.operand is MethodInfo methodInfo => methodInfo.GetParameters().Length +
                                                                                    (methodInfo.HasThis ? 1 : 0),
                StackBehaviour.Varpop when inst.operand is ConstructorInfo constructorInfo => constructorInfo.GetParameters().Length + 
                                                                                              (inst.opcode == OpCodes.Newobj ? 0 : 1),
                StackBehaviour.Varpop when inst.operand?.GetType() == HarmonyInterface.InlineSignature =>
                    HarmonyInterface.InlineSignature_Parameters(inst.operand).Count +
                    (HarmonyInterface.InlineSignature_HasThis(inst.operand) ? 2 : 1),
                StackBehaviour.Varpop => OpCodeData.GetCanonicalOpcode(inst) switch
                {
                    OpCodeValues.Ret => methodReturnType == typeof(void) ? 0 : 1,
                    _ => throw new ArgumentOutOfRangeException(),
                },
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
