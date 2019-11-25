using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PuppetServer : MarshalByRefObject
    {
        ServerCli sc;

        public PuppetServer(ServerCli sc)
        {
            this.sc = sc;
        }

        public void freeze()
        {
            Console.WriteLine("hey this is ps");
        }

        public void unfreeze()
        {
            Console.WriteLine("hey this is ps unfroze");

        }

        public void addRoom(String location, int capacity, String roomName)
        {

        }


        public void addServer(String server)
        {

        }

        public void addServerToView(String serverid, String serverurl)
        {
            sc.addServerToView(serverid, serverurl);
        }
    }
}
