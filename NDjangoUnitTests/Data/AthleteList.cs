﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace NDjango.UnitTests.Data
{

    public class AthleteList : System.Collections.IEnumerable
    {

        public AthleteList()
        {
            list = new List<Athlete>(new Athlete[] {new Athlete("Michael","Jordan"), new Athlete("Magic","Johnson")});
        }
        List<Athlete> list;

        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        struct Athlete
        {
            public Athlete(string fstName, string lstName) { FirstName = fstName; LastName = lstName; }
            public string FirstName;
            public string LastName;
            public string name { get { return FirstName + ' ' + LastName; } }
        }
    }


}