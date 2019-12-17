using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library;

namespace Client
{
    class ClientServ : MarshalByRefObject, IClient
    {
        private Cliente client;
        private User user;
        private string connectedServer;

        public ClientServ(Cliente cl)
        {
            this.client = cl;
            this.user = new User(cl.GetName());
        }

        public User getUser()
        {
            return user;
        }

        public void setUser(User u)
        {
            this.user = u;
        }

        public List<MeetingProposal> getMyProp()
        {
            return this.user.getMyMP();
        }

        public List<MeetingProposal> activeMP()
        {
            return this.user.getActiveMP();
        }

        public String[] getBackupServerURL()
        {
            return client.getBackupServerURL();
        }
        public void setBackupServerURL(String[] urls)
        {
            this.client.setBackupServerURL(urls);
        }

        public string getClientURL(){
            return client.getURL();
        }

        public void shareProposal(MeetingProposal mp, List<string> list, string serv){
            this.client.ShareProposal(mp, list, serv);
        }

        public void receiveProposal(MeetingProposal mp, List<string> list, string serv){
            this.client.receiveProposal(mp, list, serv);
        }

        public void status()
        {
            Console.WriteLine();
            Console.WriteLine("Client: " + getClientURL());
            Console.WriteLine("Currently connected to: " + client.getSURL());
            Console.Write("Insert command: ");
        }

        public void updateLocalClients()
        {
            this.client.updateLocalClients();
        }

        public void connectToBackup(int index, List<string> list)
        {
            this.client.connectToBackup(index, list);
        }

        public void setLocalClients(List<String> localClients)
        {
            this.client.setLocalClients(localClients);
        }

        public List<MeetingProposal> getMeetingProposals()
        {
            return this.client.getMeetingProposals();
        } 

        public void updateClientState()
        {
            client.updateClientState();
        }
    }
}
