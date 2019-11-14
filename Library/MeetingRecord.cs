using System;
using System.Collections.Generic;

namespace Library
{
    [Serializable]
    public class MeetingRecord
    {
        private User user;
        private List<Slot> slots; // i think it can be more than one but not entirely sure

        public MeetingRecord(User user, List<Slot> slots)
        {
            this.user = user;
            this.slots = slots;
        }
        public List<Slot> GetSlots()
        {
            return slots;
        }

        public User GetUser()
        {
            return user;
        }
    }
}
