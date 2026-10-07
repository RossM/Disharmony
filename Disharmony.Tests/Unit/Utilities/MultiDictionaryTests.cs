using Disharmony.Utilities;

namespace Disharmony.Tests.Unit.Utilities;

[TestFixture]
public sealed class MultiDictionaryTests
{
    [Test]
    public void AddAssociatesMultipleValuesWithEachKeyInInsertionOrder()
    {
        MultiDictionary<string, int> dictionary = new()
        {
            { "first", 1 },
            { "second", 2 },
            { "first", 3 },
        };

        Assert.Multiple(() =>
        {
            Assert.That(dictionary.TryGetValues("first", out var first), Is.True);
            Assert.That(first, Is.EqualTo(new[] { 1, 3 }));
            Assert.That(dictionary.TryGetValues("second", out var second), Is.True);
            Assert.That(second, Is.EqualTo(new[] { 2 }));
        });
    }

    [Test]
    public void IndexerAndContains_ReportExistingAndMissingKeys()
    {
        MultiDictionary<string, int> dictionary = new() { { "present", 1 }, { "present", 2 } };

        Assert.Multiple(() =>
        {
            Assert.That(dictionary.Contains("present"), Is.True);
            Assert.That(dictionary["present"], Is.EqualTo(new[] { 1, 2 }));
            Assert.That(dictionary.Contains("missing"), Is.False);
            Assert.That(dictionary["missing"], Is.Empty);
        });
    }

    [Test]
    public void Get_ExistingAndMissingKeys_ReturnReadOnlyLists()
    {
        MultiDictionary<string, int> dictionary = new()
        {
            { "present", 1 },
            { "present", 2 },
        };

        IReadOnlyList<int> present = dictionary["present"];
        IReadOnlyList<int> missing = dictionary["missing"];

        Assert.Multiple(() =>
        {
            Assert.That(present, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(missing, Is.Empty);
        });
    }

    [Test]
    public void Get_AfterRemovingLastValue_ReturnsEmptyList()
    {
        MultiDictionary<string, int> dictionary = new() { { "key", 1 } };

        dictionary.Remove("key", 1);

        Assert.That(dictionary["key"], Is.Empty);
    }

    [Test]
    public void Count_CountsKeysRatherThanValues()
    {
        MultiDictionary<string, int> dictionary = new()
        {
            { "first", 1 },
            { "first", 2 },
            { "second", 3 },
        };

        Assert.That(dictionary.Count, Is.EqualTo(2));
    }

    [Test]
    public void Remove_ExistingValue_PreservesNonemptyGrouping()
    {
        MultiDictionary<string, int> dictionary = new()
        {
            { "key", 1 },
            { "key", 2 },
        };

        bool removed = dictionary.Remove("key", 1);

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.True);
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(dictionary.Contains("key"), Is.True);
            Assert.That(dictionary["key"], Is.EqualTo(new[] { 2 }));
        });
    }

    [Test]
    public void Remove_LastValue_RemovesGroupingFromCountAndEnumeration()
    {
        MultiDictionary<string, int> dictionary = new()
        {
            { "removed", 1 },
            { "remaining", 2 },
        };

        bool removed = dictionary.Remove("removed", 1);
        IGrouping<string, int>[] genericGroups = dictionary.ToArray();
        IGrouping<string, int>[] nonGenericGroups = ((System.Collections.IEnumerable)dictionary)
            .Cast<IGrouping<string, int>>()
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.True);
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(dictionary.Contains("removed"), Is.False);
            Assert.That(genericGroups.Select(group => group.Key), Is.EqualTo(new[] { "remaining" }));
            Assert.That(genericGroups.Single(), Is.EqualTo(new[] { 2 }));
            Assert.That(nonGenericGroups.Select(group => group.Key), Is.EqualTo(new[] { "remaining" }));
            Assert.That(nonGenericGroups.Single(), Is.EqualTo(new[] { 2 }));
        });
    }

    [Test]
    public void Remove_MissingAssociation_ReturnsFalseWithoutChangingContents()
    {
        MultiDictionary<string, int> dictionary = new() { { "key", 1 } };

        bool missingKeyRemoved = dictionary.Remove("missing", 1);
        bool missingValueRemoved = dictionary.Remove("key", 2);

        Assert.Multiple(() =>
        {
            Assert.That(missingKeyRemoved, Is.False);
            Assert.That(missingValueRemoved, Is.False);
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(dictionary["key"], Is.EqualTo(new[] { 1 }));
        });
    }

    [Test]
    public void RemoveAll_ForKey_RemovesMatchingValuesAndReturnsCount()
    {
        MultiDictionary<string, int> dictionary = new()
        {
            { "first", 1 },
            { "first", 2 },
            { "first", 3 },
            { "second", 4 },
        };

        int removed = dictionary.RemoveAll("first", value => value != 2);
        int missing = dictionary.RemoveAll("missing", _ => true);

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.EqualTo(2));
            Assert.That(missing, Is.Zero);
            Assert.That(dictionary.Count, Is.EqualTo(2));
            Assert.That(dictionary["first"], Is.EqualTo(new[] { 2 }));
            Assert.That(dictionary["second"], Is.EqualTo(new[] { 4 }));
        });
    }

    [Test]
    public void RemoveAll_AcrossKeys_RemovesMatchingValuesAndEmptyGroupings()
    {
        MultiDictionary<string, int> dictionary = new()
        {
            { "first", 1 },
            { "first", 2 },
            { "second", 2 },
            { "third", 3 },
        };

        int removed = dictionary.RemoveAll((key, value) => key == "second" || value % 2 != 0);
        IGrouping<string, int>[] groups = dictionary.ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.EqualTo(3));
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(groups.Select(group => group.Key), Is.EqualTo(new[] { "first" }));
            Assert.That(groups.Single(), Is.EqualTo(new[] { 2 }));
        });
    }

    [Test]
    public void Add_AfterRemovingGrouping_DoesNotRestoreRemovedValues()
    {
        MultiDictionary<string, int> dictionary = new() { { "removed", 1 } };
        dictionary.Remove("removed", 1);

        dictionary.Add("new", 2);

        Assert.Multiple(() =>
        {
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(dictionary.Contains("removed"), Is.False);
            Assert.That(dictionary["new"], Is.EqualTo(new[] { 2 }));
        });
    }

    [Test]
    public void ClearRemovesEveryAssociation()
    {
        MultiDictionary<string, int> dictionary = new() { { "first", 1 }, { "second", 2 } };

        dictionary.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(dictionary.Count, Is.Zero);
            Assert.That(dictionary, Is.Empty);
            Assert.That(dictionary.Contains("first"), Is.False);
            Assert.That(dictionary["first"], Is.Empty);
            Assert.That(dictionary.TryGetValues("first", out var values), Is.False);
            Assert.That(values, Is.Null);
        });
    }
}
