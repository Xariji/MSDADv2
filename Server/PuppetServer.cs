using System;
using Library;
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
            sc.freeze();
        }

        public void unfreeze()
        {
            sc.unfreeze();

        }

        public void status()
        {
            sc.status();
        }

        public void initializeView(String serverid, String serverurl)
        {
            sc.initializeView(serverid, serverurl);
        }

        public void addServerToView(String serverid, String serverurl)
        {
            sc.addServerToView(serverid, serverurl);
        }

        public void updateLocations(List<MeetingLocation> mls)
        {
            sc.updateLocations(mls);
        }
    }
}
