using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LiveClient
{


    public class User
    {
        public readonly long id;
        public readonly string name;

        public User(long id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}