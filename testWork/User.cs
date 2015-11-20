﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testWork
{

    public class User
    {
        public ObjectId Id { get; set; }
        public String name { get; set; }
        public String password { get; set; }

        public User(String userName, String password)
        {
            this.name = userName;
            this.password = password;

        }
    }
}
