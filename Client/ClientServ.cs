﻿using System;
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

        public ClientServ(Cliente arg)
        {
            this.client = arg;
            this.user = new User(arg.GetName());
        }

        public User getUser()
        {
            return user;
        }

        public void setUser(User u)
        {
            this.user = u;
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

        public void shareProposal(MeetingProposal mp){
            this.client.ShareProposal(mp);
        }

        public void receiveProposal(MeetingProposal mp){
            this.client.receiveProposal(mp);
        }
    }
}
