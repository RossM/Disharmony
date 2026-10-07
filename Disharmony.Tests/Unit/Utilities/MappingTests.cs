using Disharmony.Utilities;

namespace Disharmony.Tests.Unit.Utilities;

[TestFixture]
public sealed class MappingTests
{
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record Value(int Id);

    [Test]
    public void Indexer_UnmappedKey_MapsToItself()
    {
        Mapping<int> mapping = [];

        Assert.That(mapping[1], Is.EqualTo(1));
    }

    [Test]
    public void Indexer_Assignment_AddsAndReplacesMapping()
    {
        Mapping<int> mapping = [];
        mapping[1] = 2;

        mapping[1] = 3;

        Assert.Multiple(() =>
        {
            Assert.That(mapping[1], Is.EqualTo(3));
            Assert.That(mapping, Is.EquivalentTo([new MappingElement<int>(1, 3)]));
        });
    }

    [Test]
    public void Indexer_AssigningIdentity_RemovesExplicitMapping()
    {
        Mapping<int> mapping = [];
        mapping[1] = 2;

        mapping[1] = 1;

        Assert.Multiple(() =>
        {
            Assert.That(mapping[1], Is.EqualTo(1));
            Assert.That(mapping, Is.Empty);
        });
    }

    [Test]
    public void Equality_CustomValueEquality_IsUsedForKeysAndIdentityMappings()
    {
        Mapping<Value> mapping = [];
        Value key = new(1);
        Value equalKey = new(1);
        Value value = new(2);
        mapping[key] = value;

        Value mapped = mapping[equalKey];
        mapping[key] = new Value(1);

        Assert.Multiple(() =>
        {
            Assert.That(mapped, Is.EqualTo(value));
            Assert.That(mapping, Is.Empty);
        });
    }

    [Test]
    public void Enumeration_GenericAndNonGeneric_ReturnExplicitMappings()
    {
        Mapping<int> mapping = [];
        mapping[1] = 2;
        mapping[3] = 4;

        Assert.Multiple(() =>
        {
            Assert.That(mapping, Is.EquivalentTo([
                new MappingElement<int>(1, 2),
                new MappingElement<int>(3, 4),
            ]));
            Assert.That(mapping.Cast<MappingElement<int>>(), Is.EquivalentTo([
                new MappingElement<int>(1, 2),
                new MappingElement<int>(3, 4),
            ]));
        });
    }

    [Test]
    public void Merge_SingleElementMappings_ComposesEveryOverlap()
    {
        (string Name, int Key1, int Value1, int Key2, int Value2, MappingElement<int>[] Expected)[] cases =
        [
            ("Disjoint", 1, 2, 3, 4,
                [new(1, 2), new(3, 4)]),
            ("Key1EqualsKey2", 1, 2, 1, 3,
                [new(1, 2)]),
            ("Key1EqualsValue2", 1, 2, 3, 1,
                [new(1, 2), new(3, 1)]),
            ("Value1EqualsKey2", 1, 2, 2, 3,
                [new(1, 3), new(2, 3)]),
            ("Value1EqualsValue2", 1, 3, 2, 3,
                [new(1, 3), new(2, 3)]),
            ("PairsEqual", 1, 2, 1, 2,
                [new(1, 2)]),
            ("PairsReversed", 1, 2, 2, 1,
                [new(2, 1)]),
        ];

        foreach (var testCase in cases)
        {
            Mapping<int> first = [];
            first[testCase.Key1] = testCase.Value1;
            Mapping<int> second = [];
            second[testCase.Key2] = testCase.Value2;

            Mapping<int> merged = Mapping<int>.Merge(first, second);

            Assert.Multiple(() =>
            {
                Assert.That(merged, Is.EquivalentTo(testCase.Expected), testCase.Name);
                foreach (int input in new[]
                         {
                             testCase.Key1,
                             testCase.Value1,
                             testCase.Key2,
                             testCase.Value2,
                         }.Distinct())
                {
                    Assert.That(merged[input], Is.EqualTo(second[first[input]]),
                        $"{testCase.Name}: input {input}");
                }
                Assert.That(first, Is.EquivalentTo([
                        new MappingElement<int>(testCase.Key1, testCase.Value1),
                    ]), $"{testCase.Name}: first mapping was mutated");
                Assert.That(second, Is.EquivalentTo([
                        new MappingElement<int>(testCase.Key2, testCase.Value2),
                    ]), $"{testCase.Name}: second mapping was mutated");
            });
        }
    }
}
