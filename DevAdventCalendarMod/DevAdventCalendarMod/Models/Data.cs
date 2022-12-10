using System;
using System.Collections.Generic;
using System.Text;

namespace DevAdventCalendarMod.Models
{
    [Serializable]
    public class Data
    {
        public List<int> ChocolatesPickedUp;
        public List<int> DoorsOpened;
        public bool GiftOpened;
        public bool DevChocolatePickedUp;
    }
}
