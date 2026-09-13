using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace PartyMarkerPartyList;

internal static class MarkerMatcher
{
    // Remote party members and empty marker slots can share this invalid ID.
    private const uint InvalidEntityId = 0xE0000000;

    // Marker.csv order: Attack 1-5, Bind 1-3, Ignore 1-2, shapes, Attack 6-8.
    private static readonly uint[] TargetMarkerIconIds =
    [
        61201, 61202, 61203, 61204, 61205,
        61211, 61212, 61213,
        61221, 61222,
        61231, 61232, 61233, 61234,
        61206, 61207, 61208,
    ];

    internal static bool TryGetMarkerIconId(
        ReadOnlySpan<GameObjectId> markers,
        uint entityId,
        out uint markerIconId)
    {
        markerIconId = 0;
        if (entityId is 0 or InvalidEntityId)
            return false;

        var count = Math.Min(markers.Length, TargetMarkerIconIds.Length);
        for (var index = 0; index < count; index++)
        {
            // Only type 0 encodes an EntityId; other types use a base ID or object index.
            if (markers[index].Type == 0 && markers[index].ObjectId == entityId)
            {
                markerIconId = TargetMarkerIconIds[index];
                return true;
            }
        }

        return false;
    }
}
