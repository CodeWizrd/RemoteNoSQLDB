///////////////////////////////////////////////////////////////
// DBFactoryTest.cs - Test DBFactory and DBExtensions        //
// Ver 1.0                                                   //
// Application: Test DBFactory for CSE681-SMA, Project#2     //
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
 * This package replaces DBFactory test stub to remove
 * circular package references.
 *
 * Now this testing depends on the class definitions in DBElement,
 * DBFactory, and the extension methods defined in DBExtensions.
 * We no longer need to define extension methods in DBFactory.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   DBFactory.cs, DBElement.cs, DBExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 08 Oct 15
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
    class DBFactoryTest
    {
        static void Main(string[] args)
        {
            /*
             * Create and edit a DBEngine first.
             * Then create a DBFactory using the DBEngine.
             */

            "Testing DBEngine Package".title('=');
            WriteLine();

            Write("\n --- Test DBElement<int,string> ---");
            DBElement<int, string> elem1 = new DBElement<int, string>();
            elem1.payload = "a payload";
            Write(elem1.showElement<int, string>());
            WriteLine();

            DBElement<int, string> elem2 = new DBElement<int, string>("Darth Vader", "Evil Overlord");
            elem2.payload = "The Empire strikes back!";
            Write(elem2.showElement<int, string>());
            WriteLine();

            int key = 0;
            Func<int> keyGen = () => { ++key; return key; };  // anonymous function to generate keys

            DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
            bool p1 = db.insert(keyGen(), elem1);
            bool p2 = db.insert(keyGen(), elem2);

            Write("\n --- Create DBFactory<int, DBElement<int, string>> from DBEngine<int,DBElement<int, string>> ---");

            DBFactory<int, DBElement<int, string>> dbFactory = new DBFactory<int, DBElement<int, string>>(db);
            foreach (int dbKey in dbFactory.Keys())
            {
                DBElement<int, string> value;
                dbFactory.getValue(dbKey, out value);
                value.showElement();
            }
            dbFactory.showDB();
            Write("\n\n");
        }
    }

}
