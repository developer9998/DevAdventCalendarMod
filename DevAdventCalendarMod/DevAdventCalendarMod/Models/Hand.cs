using System;

namespace DevAdventCalendarMod.Models
{
    [Serializable]
    public class Hand
    {
        public GorillaTriggerColliderHandIndicator HandIndicator { get; set; }
        public bool HasObject { get; set; }
    }
}
