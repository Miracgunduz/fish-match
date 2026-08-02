using System.Collections.Generic;
using FishMatch.Match3;
using NUnit.Framework;
using UnityEngine;

public class DecorationOwnershipRegistryTests
{
    [SetUp]
    public void ResetState()
    {
        DecorationOwnershipRegistry.Reset();
    }

    [Test]
    public void MarkOwnedByIdMakesIsOwnedTrueForAMatchingDefinition()
    {
        var decoration = ScriptableObject.CreateInstance<DecorationDefinition>();
        decoration.decorationId = "TestDecoration";

        DecorationOwnershipRegistry.MarkOwnedById("TestDecoration");

        Assert.IsTrue(DecorationOwnershipRegistry.IsOwned(decoration));

        Object.DestroyImmediate(decoration);
    }

    [Test]
    public void IsOwnedIsFalseForNullDecoration()
    {
        Assert.IsFalse(DecorationOwnershipRegistry.IsOwned(null));
    }

    [Test]
    public void FilterOwnedSkipsNullEntriesAndUnownedDecorations()
    {
        var owned = ScriptableObject.CreateInstance<DecorationDefinition>();
        owned.decorationId = "Owned";
        var notOwned = ScriptableObject.CreateInstance<DecorationDefinition>();
        notOwned.decorationId = "NotOwned";

        DecorationOwnershipRegistry.MarkOwnedById("Owned");

        var catalog = new[] { owned, null, notOwned };
        var result = new List<DecorationDefinition>(DecorationOwnershipRegistry.FilterOwned(catalog));

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(owned, result[0]);

        Object.DestroyImmediate(owned);
        Object.DestroyImmediate(notOwned);
    }
}
