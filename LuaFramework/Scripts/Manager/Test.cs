using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

    public class Test
    {
    public string b = "";
    public Test(string a) {
        b = a;
    }
        public void Say() {
            Debug.Log("I am test!! and b =="+b);

        }
    }

