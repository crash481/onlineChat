using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testWork
{
    public class Message
    {
        public ObjectId Id { get; set; }
        public String userName { get; set; }
        public String message { get; set; }
        public String time { get; set; }

        public Message(String userName, String message, String time)
        {
            this.userName = userName;
            this.message = message;
            this.time = time;
        }
    }
}
