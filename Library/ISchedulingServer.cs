using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    public interface ISchedulingServer
    {
        Message Response(string request, List<string> args);

    }
}
