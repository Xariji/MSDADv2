using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class PuppetServer : MarshalByRefObject
    {
        ServerCli sc;

        public PuppetServer(ServerCli sc)
        {
            this.sc = sc;
        }

        public void freeze()
        {

        }

        public void unfreeze()
        {

        }

        public void addRoom(String location, int capacity, String roomName)
        {

        }


        public void addServer(String server)
        {

        }

        public Dictionary<string, string> fetchView()
        {
            return null;
        }
    }
}
