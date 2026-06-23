using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A producer of other players' live positions for the map. The networked build registers a source backed by
    /// Nakama presence (publishing the local position when the player opts in, and pumping friends' positions into
    /// the registry); offline there is no source, so friend markers stay correct-but-empty. <see cref="MapState"/>
    /// owns the <see cref="PresenceRegistry"/> and the freshness window and drives this each poll.
    /// </summary>
    public interface IFriendPresence
    {
        /// <summary>Broadcast the local player's position upstream — only meaningful when <paramref name="sharing"/>
        /// is true; when false the implementation should stop broadcasting. Offline/dev sources may ignore it.</summary>
        void PublishSelf(string localId, Vector3 position, bool sharing, float now);

        /// <summary>Report the latest known friend positions into the registry (refreshing their timestamps).</summary>
        void Poll(PresenceRegistry into, float now);
    }
}
