using FFXIVClientStructs.FFXIV.Client.Game.Object;
using PartyMarkerPartyList;

namespace PartyMarkerPartyList.Tests;

public class MarkerMatcherTests
{
    private const uint InvalidId = 0xE0000000;
    private const uint PlayerId = 0x12345678;

    private static readonly uint[] ExpectedIcons =
    [
        61201, 61202, 61203, 61204, 61205, 61211, 61212, 61213,
        61221, 61222, 61231, 61232, 61233, 61234, 61206, 61207, 61208,
    ];

    public static TheoryData<int> Slots()
    {
        var data = new TheoryData<int>();
        for (var slot = 0; slot < ExpectedIcons.Length; slot++)
            data.Add(slot);
        return data;
    }

    [Fact]
    public void CrossWorldRemoteMembersDoNotMatchEmptySlots()
    {
        // A cross-world party outside duties: remote members share the invalid ID with empty marker slots.
        var markers = FilledWithInvalid();
        AssertNoMarker(markers, InvalidId);
        AssertNoMarker(markers, PlayerId);
        AssertNoMarker(markers, 0);

        markers[0] = Entity(PlayerId);
        AssertMarker(markers, PlayerId, 61201);
        AssertNoMarker(markers, InvalidId);

        markers[0] = Entity(InvalidId);
        AssertNoMarker(markers, PlayerId);
    }

    [Fact]
    public void ZeroInitializedSlotsDoNotMatch()
    {
        var markers = new GameObjectId[17];
        AssertNoMarker(markers, 0);
        AssertNoMarker(markers, PlayerId);
    }

    [Theory]
    [MemberData(nameof(Slots))]
    public void EachNativeSlotMapsToItsIcon(int slot)
    {
        var markers = FilledWithInvalid();
        markers[slot] = Entity(PlayerId);
        AssertMarker(markers, PlayerId, ExpectedIcons[slot]);
        AssertNoMarker(markers, PlayerId + 1);
    }

    [Theory]
    [InlineData((byte)1)]
    [InlineData((byte)2)]
    [InlineData((byte)4)]
    public void NonEntityObjectTypesAreSkipped(byte type)
    {
        var markers = new GameObjectId[17];
        markers[0] = new GameObjectId { ObjectId = PlayerId, Type = type };
        AssertNoMarker(markers, PlayerId);

        markers[1] = Entity(PlayerId);
        AssertMarker(markers, PlayerId, 61202);
    }

    [Fact]
    public void EmptyMarkerSpanDoesNotMatch()
    {
        AssertNoMarker([], PlayerId);
    }

    [Fact]
    public void IconTableUsesSheetRowsOffsetByOne()
    {
        var table = MarkerMatcher.BuildIconTable(17, rowId => 70000 + rowId);
        Assert.Equal(70001u, table[0]);
        Assert.Equal(70017u, table[16]);
    }

    [Fact]
    public void IconTableFallsBackPerSlotWhenSheetRowIsMissing()
    {
        var table = MarkerMatcher.BuildIconTable(17, rowId => rowId == 1 ? 0u : 70000 + rowId);
        Assert.Equal(61201u, table[0]);
        Assert.Equal(70002u, table[1]);

        Assert.Equal(ExpectedIcons, MarkerMatcher.BuildIconTable(17, _ => 0));
    }

    [Fact]
    public void SlotsBeyondKnownIconsAreIgnoredWhenSheetHasNoIcon()
    {
        var table = MarkerMatcher.BuildIconTable(18, _ => 0);
        Assert.Equal(0u, table[17]);

        var markers = FilledWithInvalid(18);
        markers[17] = Entity(PlayerId);
        Assert.False(MarkerMatcher.TryGetMarkerIconId(markers, PlayerId, table, out _));
    }

    [Fact]
    public void CustomIconTableIsUsed()
    {
        var markers = FilledWithInvalid();
        markers[3] = Entity(PlayerId);
        var table = MarkerMatcher.BuildIconTable(17, rowId => 80000 + rowId);
        Assert.True(MarkerMatcher.TryGetMarkerIconId(markers, PlayerId, table, out var icon));
        Assert.Equal(80004u, icon);
    }

    private static GameObjectId Entity(uint id) => new() { ObjectId = id };

    private static GameObjectId[] FilledWithInvalid(int length = 17)
    {
        var markers = new GameObjectId[length];
        Array.Fill(markers, Entity(InvalidId));
        return markers;
    }

    private static void AssertMarker(GameObjectId[] markers, uint entityId, uint expectedIcon)
    {
        Assert.True(MarkerMatcher.TryGetMarkerIconId(markers, entityId, out var icon));
        Assert.Equal(expectedIcon, icon);
    }

    private static void AssertNoMarker(GameObjectId[] markers, uint entityId)
    {
        Assert.False(MarkerMatcher.TryGetMarkerIconId(markers, entityId, out var icon));
        Assert.Equal(0u, icon);
    }
}
