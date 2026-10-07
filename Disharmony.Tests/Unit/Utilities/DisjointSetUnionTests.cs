using Disharmony.Utilities;

namespace Disharmony.Tests.Unit.Utilities;

[TestFixture]
public sealed class DisjointSetUnionTests
{
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record ValueRecord(int Value);

    [Test]
    public void Add_NewAndExistingValues_ReturnsWhetherTheValueWasAdded()
    {
        DisjointSetUnion<object> sets = new();
        object value = new();

        Assert.Multiple(() =>
        {
            Assert.That(sets.Add(value), Is.True);
            Assert.That(sets.Add(value), Is.False);
            Assert.That(sets[value], Is.SameAs(value));
            Assert.That(sets[value], Is.SameAs(value));
        });
    }

    [Test]
    public void GetRoot_ValueWasNotAdded_ThrowsKeyNotFoundException()
    {
        DisjointSetUnion<object> sets = new();

        Assert.That(() => sets[new object()], Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void Merge_TwoValues_UsesTheRightSetRoot()
    {
        DisjointSetUnion<object> sets = new();
        object left = new();
        object right = new();
        sets.Add(left);
        sets.Add(right);

        sets.Merge(left, right);

        Assert.Multiple(() =>
        {
            Assert.That(sets[left], Is.SameAs(right));
            Assert.That(sets[right], Is.SameAs(right));
        });
    }

    [Test]
    public void Merge_ChainedSets_UpdatesEveryValueToTheFinalRoot()
    {
        DisjointSetUnion<object> sets = new();
        object first = new();
        object second = new();
        object third = new();
        object fourth = new();
        sets.Add(first);
        sets.Add(second);
        sets.Add(third);
        sets.Add(fourth);

        sets.Merge(first, second);
        sets.Merge(third, fourth);
        sets.Merge(second, third);

        Assert.Multiple(() =>
        {
            Assert.That(sets[first], Is.SameAs(fourth));
            Assert.That(sets[second], Is.SameAs(fourth));
            Assert.That(sets[third], Is.SameAs(fourth));
            Assert.That(sets[fourth], Is.SameAs(fourth));
        });
    }

    [Test]
    public void Merge_ValuesAlreadyInTheSameSet_DoesNotChangeTheRoot()
    {
        DisjointSetUnion<object> sets = new();
        object first = new();
        object second = new();
        sets.Add(first);
        sets.Add(second);
        sets.Merge(first, second);

        sets.Merge(second, first);
        sets.Merge(first, first);

        Assert.Multiple(() =>
        {
            Assert.That(sets[first], Is.SameAs(second));
            Assert.That(sets[second], Is.SameAs(second));
        });
    }

    [Test]
    public void Enumeration_ReturnsOneGroupingPerDisjointSet()
    {
        DisjointSetUnion<object> sets = new();
        object first = new();
        object second = new();
        object third = new();
        object fourth = new();
        sets.Add(first);
        sets.Add(second);
        sets.Add(third);
        sets.Add(fourth);
        sets.Merge(first, second);
        sets.Merge(third, fourth);

        var groups = sets.ToList();

        Assert.Multiple(() =>
        {
            Assert.That(groups, Has.Count.EqualTo(2));
            Assert.That(groups.Single(group => ReferenceEquals(group.Key, second)),
                Is.EquivalentTo([first, second]));
            Assert.That(groups.Single(group => ReferenceEquals(group.Key, fourth)),
                Is.EquivalentTo([third, fourth]));
            Assert.That(sets.Cast<object>().Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public void CustomEquality_EqualValuesBelongToTheSameSet()
    {
        DisjointSetUnion<ValueRecord> sets = new();
        ValueRecord canonical = new(1);
        ValueRecord equalValue = new(1);

        bool addedCanonical = sets.Add(canonical);
        bool addedEqualValue = sets.Add(equalValue);

        Assert.Multiple(() =>
        {
            Assert.That(addedCanonical, Is.True);
            Assert.That(addedEqualValue, Is.False);
            Assert.That(sets[canonical], Is.EqualTo(sets[equalValue]));
            Assert.That(sets[canonical], Is.EqualTo(sets[equalValue]));
            Assert.That(sets.Count(), Is.EqualTo(1));
            Assert.That(sets.Single().Count(), Is.EqualTo(1));
            Assert.That(sets.Single().Single(), Is.EqualTo(canonical));
        });
    }

    [Test]
    public void CustomEquality_MergeUsingEqualInstancesCombinesTheirSets()
    {
        DisjointSetUnion<ValueRecord> sets = new();
        ValueRecord left = new(1);
        ValueRecord equalLeft = new(1);
        ValueRecord right = new(2);
        ValueRecord equalRight = new(2);
        sets.Add(left);
        sets.Add(right);

        sets.Merge(equalLeft, equalRight);

        Assert.Multiple(() =>
        {
            Assert.That(sets[left], Is.EqualTo(sets[equalLeft]));
            Assert.That(sets[left], Is.EqualTo(sets[right]));
            Assert.That(sets[right], Is.EqualTo(sets[equalRight]));
            Assert.That(sets.Count(), Is.EqualTo(1));
            Assert.That(sets.Single(), Is.EquivalentTo([left, right]));
        });
    }

    [Test]
    public void CustomEquality_MergeEqualValuesLeavesTheSetUnchangedAndUsable()
    {
        DisjointSetUnion<ValueRecord> sets = new();
        ValueRecord value = new(1);
        ValueRecord equalValue = new(1);
        sets.Add(value);

        sets.Merge(value, equalValue);

        Assert.Multiple(() =>
        {
            Assert.That(sets[value], Is.EqualTo(sets[equalValue]));
            Assert.That(sets.Count(), Is.EqualTo(1));
            Assert.That(sets.Single().Count(), Is.EqualTo(1));
            Assert.That(sets.Single().Single(), Is.EqualTo(value));
        });
    }
}
