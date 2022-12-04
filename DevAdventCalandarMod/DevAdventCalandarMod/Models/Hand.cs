using System;

namespace DevAdventCalandarMod.Models
{
    [Serializable]
    public class Hand
    {
        public GorillaTriggerColliderHandIndicator handIndicator { get; set; }
        public bool HasObject { get; set; }
    }
}
