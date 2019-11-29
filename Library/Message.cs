using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    [Serializable]
    public class Message
    {

        private bool sucess;
        private object obj;
        private string message;

        public Message(bool sucess, object obj, string message)
        {
            this.sucess = sucess;
            this.obj = obj;
            this.message = message;
        }


        public bool getSucess()
        {
            return sucess;
        }

        public object getObj()
        {
            return obj;
        }

        public string getMessage()
        {
            return message;
        }

    }
}
