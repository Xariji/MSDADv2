using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Client;
using Server;

namespace PCS
{

    // this guy is only launching servers and clients locally, therefore doesn't need to remote anything
    public class PCSService : MarshalByRefObject
    {

        private static int port = 10000;


        public static void Main(String[] args)
        {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            System.Console.WriteLine("PCS running \n  Press Enter to leave");
            System.Console.ReadLine();
        }

        public void createServerProcess(String id, String URL, int maxFaults, int minDelay, int maxDelay)
        {
            SchedulingServer ss = new SchedulingServer(id, URL, maxFaults, minDelay, maxDelay);
            ss.start();
        }

        public void createClientProcess(String username, String cURL, String sURL, String script)
        {
            Cliente cc = new Cliente(username, cURL, sURL, script);
            cc.start();
        }
    }
}
