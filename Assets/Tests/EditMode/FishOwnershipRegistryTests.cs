using System.Collections.Generic;
using FishMatch.Match3;
using NUnit.Framework;
using UnityEngine;

public class FishOwnershipRegistryTests
{
    [SetUp]
    public void ResetState()
    {
        FishOwnershipRegistry.Reset();
    }

    [Test]
    public void MarkOwnedByIdMakesIsOwnedTrueForAMatchingDefinition()
    {
        var fish = ScriptableObject.CreateInstance<FishSpeciesDefinition>();
        fish.speciesId = "TestFish";

        FishOwnershipRegistry.MarkOwnedById("TestFish");

        Assert.IsTrue(FishOwnershipRegistry.IsOwned(fish));

        Object.DestroyImmediate(fish);
    }

    [Test]
    public void IsOwnedIsFalseForNullSpecies()
    {
        Assert.IsFalse(FishOwnershipRegistry.IsOwned(null));
    }

    [Test]
    public void FilterOwnedSkipsNullEntriesAndUnownedSpecies()
    {
        var owned = ScriptableObject.CreateInstance<FishSpeciesDefinition>();
        owned.speciesId = "Owned";
        var notOwned = ScriptableObject.CreateInstance<FishSpeciesDefinition>();
        notOwned.speciesId = "NotOwned";

        FishOwnershipRegistry.MarkOwned(owned);

        var roster = new[] { owned, null, notOwned };
        var result = new List<FishSpeciesDefinition>(FishOwnershipRegistry.FilterOwned(roster));

        Assert.AreEqual(1, result.Count);
        Assert.AreSame(owned, result[0]);

        Object.DestroyImmediate(owned);
        Object.DestroyImmediate(notOwned);
    }
}
