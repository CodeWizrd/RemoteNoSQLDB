///////////////////////////////////////////////////////////////
// DBFactory.cs - Immutable version of DBEngine              //
//                Reuirement#8 of Project#2 - CSE681-SMA     //
// Ver 1.0                                                   //
// Application: Immutable DB for CSE681-SMA, Project#2       //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10      //
// Author:      Sampath T Janardhan (508899838), SU          //
//              (315) 664-8206, storagar@syr.edu             //
// Source:      Jim Fawcett, CST 4-187, Syracuse University  //
//              (315) 443-3948, jfawcett@twcny.rr.com        //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements extensions methods to support 
 * displaying DBElements and DBEngine instances.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   DBFactory.cs, DBElement.cs, DBEngine.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 07 Oct 15
 * - first release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace NoSqlDB
{
    public class DBFactory<Key, Value>
    {
        private Dictionary<Key, Value> dbStore;

        //----< Default Constructor >----------------------------------------------------
        DBFactory()
        {
            dbStore = new Dictionary<Key, Value>();
        }

        //----< Public Constructor >-----------------------------------------------------
        public DBFactory(DBEngine<Key, Value> db)
        {
            dbStore = new Dictionary<Key, Value>();
            foreach (Key key in db.Keys())
            {
                Value value;
                db.getValue(key, out value);
                dbStore[key] = value;
            }
        }

        //----< Extract value for a given Key >------------------------------------------
        public bool getValue(Key key, out Value val)
        {
            if (dbStore.Keys.Contains(key))
            {
                val = dbStore[key];
                return true;
            }
            val = default(Value);
            return false;
        }

        //----< Extract collection of all Keys in DB >-----------------------------------
        public IEnumerable<Key> Keys()
        {
            return dbStore.Keys;
        }
    }

#if (DBFACTORYTEST)
    class DBFactoryTest
    {
        static void Main(string[] args)
        {
            "Testing DBFactory Package".title('=');
            WriteLine();

            Write("\n  All testing of DBFactory class moved to DBFactoryTest package.");
            Write("\n  This allow use of DBExtensions package without circular dependencies.");

            Write("\n\n");
        }
    }
#endif
}
