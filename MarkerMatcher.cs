using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace PartyMarkerPartyList;

internal static class MarkerMatcher
{
    // Remote party members and empty marker slots can share this invalid ID.
    private const uint InvalidEntityId = 0xE0000000;

    /// <summary>
    /// Fallback icons in Marker sheet order (slot N is row N + 1): Attack 1-5, Bind 1-3, Ignore 1-2, shapes, Attack 6-8.
    /// </summary>
    internal static readonly uint[] DefaultTargetMarkerIconIds =
    [
        61201, 61202, 61203, 61204, 61205,
        61211, 61212, 61213,
        61221, 61222,
        61231, 61232, 61233, 61234,
        61206, 61207, 61208,
    ];

    /// <summary>
    /// Builds the slot-to-icon table from the Marker sheet, falling back per slot to the built-in icons.
    /// </summary>
    /// <param name="slotCount">Number of native marker slots.</param>
    /// <param name="getSheetIcon">Returns the Icon column of a Marker sheet row, or 0 when the row is missing.</param>
    internal static uint[] BuildIconTable(int slotCount, Func<uint, uint> getSheetIcon)
    {
        var table = new uint[slotCount];
        for (var slot = 0; slot < slotCount; slot++)
        {
            var icon = getSheetIcon((uint)slot + 1);
            if (icon == 0 && slot < DefaultTargetMarkerIconIds.Length)
                icon = DefaultTargetMarkerIconIds[slot];

            table[slot] = icon;
        }

        return table;
    }

    internal static bool TryGetMarkerIconId(
        ReadOnlySpan<GameObjectId> markers,
        uint entityId,
        out uint markerIconId)
    {
        return TryGetMarkerIconId(markers, entityId, DefaultTargetMarkerIconIds, out markerIconId);
    }

    internal static bool TryGetMarkerIconId(
        ReadOnlySpan<GameObjectId> markers,
        uint entityId,
        ReadOnlySpan<uint> iconIds,
        out uint markerIconId)
    {
        markerIconId = 0;
        if (entityId is 0 or InvalidEntityId)
            return false;

        var count = Math.Min(markers.Length, iconIds.Length);
        for (var index = 0; index < count; index++)
        {
            // Only type 0 encodes an EntityId; other types use a base ID or object index.
            if (markers[index].Type == 0 && markers[index].ObjectId == entityId && iconIds[index] != 0)
            {
                markerIconId = iconIds[index];
                return true;
            }
        }

        return false;
    }
}
