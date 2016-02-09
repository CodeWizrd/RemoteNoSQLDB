///////////////////////////////////////////////////////////////
// DBEngine.cs - define noSQL database                       //
// Ver 1.3                                                   //
// Application: Database structure for CSE681-SMA, Project#2 //
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
 * This package implements DBEngine<Key, Value> where Value
 * is the DBElement<key, Data> type.
 *
 * This class is a starter for the DBEngine package you need to create.
 * It doesn't implement many of the requirements for the db, e.g.,
 * It doesn't remove elements, doesn't persist to XML, doesn't retrieve
 * elements from an XML file, and it doesn't provide hook methods
 * for scheduled persistance.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: DBElement.cs, and
 *                 UtilityExtensions.cs only if you enable the test stub
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.3 : 29 Sep 15
 * - added methods for remove and clear
 * ver 1.2 : 24 Sep 15
 * - removed extensions methods and tests in test stub
 * - testing is now done in DBEngineTest.cs to avoid circular references
 * ver 1.1 : 15 Sep 15
 * - fixed a casting bug in one of the extension methods
 * ver 1.0 : 08 Sep 15
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
    public class DBEngine<Key, Value>
    {
        private Dictionary<Key, Value> dbStore;

        //----< Default Public Constructor >---------------------------------------------
        public DBEngine()
        {
            dbStore = new Dictionary<Key, Value>();
        }

        //----< Insert Key/Value pair into DB >------------------------------------------
        public bool insert(Key key, Value val)
        {
            if (dbStore.Keys.Contains(key))
            return false;
            dbStore[key] = val;
            return true;
        }

        //----< Remove Key/Value pair from DB >------------------------------------------
        public bool remove(Key key)
        {
            if (dbStore.Keys.Contains(key))
            {
                dbStore.Remove(key);
                return true;
            }

            return false;
        }

        //----< Clear DB contents >------------------------------------------------------
        public void clear()
        {
            dbStore.Clear();
        }

        //----< Extract value for a given Key >------------------------------------------
        public bool getValue(Key key, out Value val)
        {
            if(dbStore.Keys.Contains(key))
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

#if(TEST_DBENGINE)

  class TestDBEngine
  {
    static void Main(string[] args)
    {
      "Testing DBEngine Package".title('=');
      WriteLine();

      Write("\n  All testing of DBEngine class moved to DBEngineTest package.");
      Write("\n  This allow use of DBExtensions package without circular dependencies.");

      Write("\n\n");
    }
  }
#endif
}
