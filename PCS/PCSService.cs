using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Client;
using Server;

namespace PCS
{

    public class PCSService : MarshalByRefObject
    {

        private static readonly int port = 10000;
        private Dictionary<String, Process> sProcesses;


        public PCSService()
        {
            this.sProcesses = new Dictionary<string, Process>();
        }


        public static void Main(String[] args)
        {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            PCSService pcs = new PCSService();
            RemotingServices.Marshal(pcs, "pcs", typeof(PCSService)); //TODO should i do this here or create another class??

            System.Console.WriteLine("PCS running \n  Press Enter to leave");
            System.Console.ReadLine();
        }

        public void createServerProcess(String serverID, String URL, int maxFaults, int minDelay, int maxDelay)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Directory.GetCurrentDirectory() + @"..\..\..\..\Server\bin\Debug\Server",
                    Arguments = serverID + "'" + URL + "'" + maxFaults + "'" + minDelay + "'" + maxDelay,

                }
            };

            process.Start();
            sProcesses.Add(serverID, process);
        }

        public void createClientProcess(String userName, String cURL, String sURL, String scriptPath)
        {
            Process process;
            if (scriptPath != "")
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Directory.GetCurrentDirectory() + @"..\..\..\..\Client\bin\Debug\Client",
                        Arguments = userName + "'" + cURL + "'" + sURL + " " + scriptPath, //TODO is a "'" missing between sURL and script?

                    }
                };
            }
            else
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Directory.GetCurrentDirectory() + @"..\..\..\..\Client\bin\Debug\Client",
                        Arguments = userName + "'" + cURL + "'" + sURL,

                    }
                };
            }

            process.Start();
        }

        public void crashServer(String serverID)
        {
            sProcesses.TryGetValue(serverID, out Process pr);
            pr.Kill();
            sProcesses.Remove(serverID);
        }
    }
}
