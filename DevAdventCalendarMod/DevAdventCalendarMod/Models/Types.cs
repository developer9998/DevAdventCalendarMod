namespace DevAdventCalendarMod.Models
{
    public enum GiveoutMode
    {
        None = 0,
        GivingOut = 1,
        LastDay = 2,
        GiftOpen = 3,
        GiftOpened = 4,
        GiftRead = 5,
        NoteReady = 6,
        NoteOpen = 7,
        NoteOpened = 8,
        ModeFinish = 9
    }

    public enum CandyMode
    {
        Intact = 0,
        Held = 1,
        Eaten = 2
    }

    public enum CandyEatMode
    {
        None = 0,
        Half = 1
    }
}
