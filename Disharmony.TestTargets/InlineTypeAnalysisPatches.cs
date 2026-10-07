namespace Disharmony.Tests;

public static class InlineTypeAnalysisPatches
{
    public static void Prefix_Is_Local_KnownSuccess(ref bool selectFirst) => selectFirst = true;
    public static void Prefix_Is_Local_KnownFailure(ref bool selectFirst) => selectFirst = false;
    public static void Prefix_Is_EvaluationStack_KnownSuccess(ref bool selectFirst) => selectFirst = true;
    public static void Prefix_Is_EvaluationStack_KnownFailure(ref bool selectFirst) => selectFirst = false;
    public static void Prefix_As_Local_KnownSuccess(ref bool selectFirst) => selectFirst = true;
    public static void Prefix_As_Local_KnownFailure(ref bool selectFirst) => selectFirst = false;
    public static void Prefix_As_EvaluationStack_KnownSuccess(ref bool selectFirst) => selectFirst = true;
    public static void Prefix_As_EvaluationStack_KnownFailure(ref bool selectFirst) => selectFirst = false;
}
