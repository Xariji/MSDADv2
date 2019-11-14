using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    [Serializable]
    public class User
    {
        private String name;
        private List<MeetingProposal> myMP; //contains MP's initiated by the user
        private List<MeetingProposal> activeMP;  //contains joined MP's

        public User(string name)
        {
            this.name = name;
            myMP = new List<MeetingProposal>();
            activeMP = new List<MeetingProposal>();
        }

        public String getName()
        {
            return name;
        }

        public List<MeetingProposal> getMyMP()
        {
            return myMP;
        }
        public List<MeetingProposal> getActiveMP()
        {
            return activeMP;
        }
        public void addMyMP(MeetingProposal mp)
        {
            myMP.Add(mp);
        }
        public void removeMyMP(MeetingProposal mp)
        {
            if (myMP.Contains(mp))
            {
                myMP.Remove(mp);
            } 
        }
        public void addActiveMP(MeetingProposal mp)
        {
            activeMP.Add(mp);
        }
        public void removeActiveMP(MeetingProposal mp)
        {
            if (activeMP.Contains(mp))
            {
                activeMP.Remove(mp);
            }
        }
    }
}