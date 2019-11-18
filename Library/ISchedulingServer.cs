using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    public interface ISchedulingServer
    {
        void Register(string url);

        Tuple<Boolean, string> AddMeetingProposal(String topic, int minParticipants,
            string[] slots, string[] invitees, string username);

        void AddMeetingLocation(string ml);

        List<String> CloseMeetingProposal(String meetingTopic, string username);

        Tuple<Boolean, int> AddUserToProposal(String meetingTopic, string username, List<Slot> slots);

        int getCurrMPId();
      
        List<MeetingProposal> getJoinableMP(String username);

        void AddMeetingRoom(string location, string room, int capacity);
      
        List<MeetingRoom> GetAvailableMeetingRooms();
      
        List<MeetingLocation> GetAvailableMeetingLocations();

        String GetServerId();
    }
}
