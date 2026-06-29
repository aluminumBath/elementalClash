namespace Elementborn.Game
{
    public static class DialogueIntentClassifier
    {
        public static DialogueIntent Classify(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return DialogueIntent.Unknown;
            }

            string t = text.ToLowerInvariant();

            if (ContainsAny(t, "bye", "goodbye", "see you"))
                return DialogueIntent.Goodbye;

            if (ContainsAny(t, "thank", "thanks", "appreciate"))
                return DialogueIntent.Thanks;

            if (ContainsAny(t, "hello", "hi", "hey", "greetings"))
                return DialogueIntent.Greeting;

            if (ContainsAny(t, "threat", "kill you", "attack you", "fight you"))
                return DialogueIntent.Threaten;

            if (ContainsAny(t, "where", "location", "map", "lost", "directions", "how do i get"))
                return DialogueIntent.AskLocation;

            if (ContainsAny(t, "quest", "objective", "mission", "what should i do", "help kram", "task"))
                return DialogueIntent.AskQuest;

            if (ContainsAny(t, "rumor", "heard", "news", "gossip", "stories"))
                return DialogueIntent.AskRumor;

            if (ContainsAny(t, "buy", "sell", "trade", "shop", "merchant", "vendor"))
                return DialogueIntent.AskTrade;

            if (ContainsAny(t, "train", "teach", "learn", "skill", "ability"))
                return DialogueIntent.AskTraining;

            if (ContainsAny(t, "boat", "sail", "wind", "ship", "dock"))
                return DialogueIntent.AskBoat;

            if (ContainsAny(t, "creature", "mount", "ride", "companion", "animal"))
                return DialogueIntent.AskCreature;

            if (ContainsAny(t, "faction", "supremacist", "unification", "channeler", "water channeler", "fire channeler"))
                return DialogueIntent.AskFaction;

            if (ContainsAny(t, "help", "stuck", "what can you do"))
                return DialogueIntent.AskHelp;

            return DialogueIntent.Unknown;
        }

        private static bool ContainsAny(string text, params string[] terms)
        {
            foreach (var term in terms)
            {
                if (text.Contains(term))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
