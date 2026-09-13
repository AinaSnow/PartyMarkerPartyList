using FFXIVClientStructs.FFXIV.Client.Game.Object;
using PartyMarkerPartyList;

const uint invalidId = 0xE0000000;
const uint playerId = 0x12345678;
var markers = new GameObjectId[17];
var checks = 0;

void Expect(uint entityId, uint expectedIcon, string scenario)
{
    var found = MarkerMatcher.TryGetMarkerIconId(markers, entityId, out var icon);
    if (found != (expectedIcon != 0) || icon != expectedIcon)
        throw new Exception($"{scenario}: found={found}, icon={icon}, expected={expectedIcon}");
    checks++;
}

// A cross-world party outside duties: three remote members share the invalid ID.
Array.Fill(markers, new GameObjectId { ObjectId = invalidId });
for (var member = 0; member < 3; member++)
    Expect(invalidId, 0, "Remote member must not match empty Attack 1");
Expect(playerId, 0, "Unmarked local member");
Expect(0, 0, "Zero party ID with sentinel marker slots");

markers[0] = new GameObjectId { ObjectId = playerId };
Expect(playerId, 61201, "Marked local member in cross-world party");
Expect(invalidId, 0, "Remote member with a real local marker present");
markers[0] = new GameObjectId { ObjectId = invalidId };
Expect(playerId, 0, "Marker cleared between frames");

Array.Clear(markers);
Expect(0, 0, "Zero party ID must not match zero marker slots");
Expect(playerId, 0, "All slots zero-initialized");

uint[] expectedIcons = [61201, 61202, 61203, 61204, 61205, 61211, 61212, 61213,
    61221, 61222, 61231, 61232, 61233, 61234, 61206, 61207, 61208];
for (var slot = 0; slot < expectedIcons.Length; slot++)
{
    Array.Fill(markers, new GameObjectId { ObjectId = invalidId });
    markers[slot] = new GameObjectId { ObjectId = playerId };
    Expect(playerId, expectedIcons[slot], $"Native marker slot {slot}");
    Expect(playerId + 1, 0, $"Another player's marker slot {slot}");
}

foreach (byte type in new byte[] { 1, 2, 4 })
{
    Array.Clear(markers);
    markers[0] = new GameObjectId { ObjectId = playerId, Type = type };
    Expect(playerId, 0, $"Non-entity object type {type} with same numeric ID");
    markers[1] = new GameObjectId { ObjectId = playerId };
    Expect(playerId, 61202, "Skip non-entity marker and find real entity marker");
}

markers = [];
Expect(playerId, 0, "Empty marker span");
Console.WriteLine($"PASS: {checks} marker matching regression checks.");
