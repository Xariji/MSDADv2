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
            sc.freeze();
        }

        public void unfreeze()
        {
            Console.WriteLine("hey this is ps unfroze");
            sc.unfreeze();

        }

        public void addRoom(String location, int capacity, String roomName)
        {
            sc.AddMeetingRoom(location, roomName,capacity);
        }


        public void addServer(String server)
        {

        }
    }
}
