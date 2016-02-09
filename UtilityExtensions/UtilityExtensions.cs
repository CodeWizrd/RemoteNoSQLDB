/////////////////////////////////////////////////////////////////////
// UtilityExtensions.cs - define methods to simplify project code  //
// Ver 1.1                                                         //
// Application: Demonstration for CSE687-OOD, Project#2            //
// Language:    C#, ver 6.0, Visual Studio 2015                    //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10            //
// Author:      Sampath T Janardhan (508899838), SU                //
//              (315) 664-8206, storagar@syr.edu                   //
// Source:      Jim Fawcett, CST 4-187, Syracuse University        //
//              (315) 443-3948, jfawcett@twcny.rr.com              //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package implements utility extensions that are not specific
 * to a single package.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: UtilityExtensions.cs
 *
 * Build Process:  devenv Project2Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.1 : 29 Sep 15
 * - added title2, body, success & error methods
 * ver 1.0 : 13 Sep 15
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
  public static class UtilityExtensions
  {
        public static void title(this string aString, char underline = '-')
        {
            Console.Write("\n {0}", new string(underline, aString.Length + 2));
            Console.Write("\n  {0}", aString);
            Console.Write("\n {0}\n", new string(underline, aString.Length + 2));
        }

        public static void title2(this string aString, char underline = '-')
        {
            Console.Write("\n  {0}", aString);
            Console.Write("\n {0}", new string(underline, aString.Length + 2));

        }

        public static void body(this string aString)
        {
            Console.Write("\n  {0}", aString);
        }

        public static void error(this string aString)
        {
            Console.Write("\n\n ---Error!---  {0}", aString);
        }

        public static void success(this string aString)
        {
            Console.Write("\n\n ---Success---  {0}", aString);
        }
    }
  public class TestUtilityExtensions
  {
    static void Main(string[] args)
    {
      "Testing UtilityExtensions.title".title();
      Write("\n\n");
    }
  }
}
