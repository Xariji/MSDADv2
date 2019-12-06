using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    public interface IClient
    {

        //void CreateProposal(String topic, int minParticipants, string[] slots, string[] invitees);

        //List<MeetingProposal> ListProposals();

        //void Participate(int meetingProposalId);

        //void CloseProposal(int meetingProposalId);

        User getUser();

        void setUser(User u);

        String[] getBackupServerURL();

        void setBackupServerURL(String[] urls);

        string getClientURL();

        void status();

        void updateLocalClients();
    }
}
