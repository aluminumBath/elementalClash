using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class RecipeBookSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<string> KnownRecipeIds = new List<string>();
    }
}
