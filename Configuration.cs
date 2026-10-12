using System.Numerics;

using Dalamud.Configuration;

namespace PartyMarkerPartyList;

/// <summary>
/// Which side of the job icon the marker is drawn on.
/// </summary>
public enum MarkerSide
{
    /// <summary>Left of the job icon.</summary>
    Left,

    /// <summary>Right of the job icon, over the start of the name.</summary>
    Right,
}

/// <summary>
/// Persisted plugin settings.
/// </summary>
[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    /// <inheritdoc/>
    public int Version { get; set; } = 1;

    /// <summary>Gets or sets a value indicating whether the overlay is drawn.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether duty-support/trust NPC rows show markers.</summary>
    public bool ShowTrustMembers { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the marker fades together with the party list.</summary>
    public bool FollowPartyListAlpha { get; set; } = true;

    /// <summary>Gets or sets the marker side relative to the job icon.</summary>
    public MarkerSide Side { get; set; } = MarkerSide.Left;

    /// <summary>Gets or sets the marker size relative to the job icon.</summary>
    public float SizeScale { get; set; } = 1.0f;

    /// <summary>Gets or sets an extra offset, in unscaled party-list pixels.</summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;
}
