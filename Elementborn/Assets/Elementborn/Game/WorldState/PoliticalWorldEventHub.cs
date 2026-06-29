using System;

namespace Elementborn.Game
{
    public static class PoliticalWorldEventHub
    {
        public static event Action<PoliticalWorldEventDefinition> EventBecameEligible;
        public static event Action<PoliticalWorldEventDefinition> EventActivated;
        public static event Action<PoliticalWorldEventDefinition> EventResolved;

        public static void RaiseEligible(PoliticalWorldEventDefinition definition)
        {
            EventBecameEligible?.Invoke(definition);
        }

        public static void RaiseActivated(PoliticalWorldEventDefinition definition)
        {
            EventActivated?.Invoke(definition);
        }

        public static void RaiseResolved(PoliticalWorldEventDefinition definition)
        {
            EventResolved?.Invoke(definition);
        }
    }
}
