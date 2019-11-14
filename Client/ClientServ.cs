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
        User user;

        public ClientServ(User arg)
        {
            this.user = arg;
        }

        public User getUser()
        {
            return user;
        }

        public void setUser(User u)
        {
            this.user = u;
        }
    }
}
