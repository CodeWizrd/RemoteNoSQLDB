///////////////////////////////////////////////////////////////
// TestExec.cs - Test Requirements for Project #4            //
// Ver 1.2                                                   //
// Application: Demonstration for CSE681-SMA, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10      //
// Author:      Sampath T Janardhan (508899838), SU          //
//              (315) 664-8206, storagar@syr.edu             //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package demonstrates that all functional requirements
 * have been met for Project#4.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   Server.exe, WpfWriteClient.exe
 *
 * Build Process:  devenv Project4.sln /Rebuild debug
 *
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 19 Nov 15
 * - first release
 *
 */

using System;
using System.Collections.Generic;
using static System.Console;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestExecutive
{
    class TestExec
    {
        List<int> tests = new List<int>();
        public bool pause { get; set; }

        public void startServer()
        {
            bool success = startProcess("../../../Server/bin/debug/Server.exe", true, "", "");
        }

        public void startWpfClient(string r, string w)
        {
            bool success = startProcess("../../../WpfWriteClient/bin/debug/WpfApplication1.exe", false, r, w);
        }

        public bool startProcess(string process, bool server, string r, string w)
        {
            process = System.IO.Path.GetFullPath(process);
            string arg = "";

            if (!server)
            {
                arg = r + " " + w;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = process,
                Arguments = arg,
                // set UseShellExecute to true to see child console, false hides console
                UseShellExecute = false
            };
            try
            {
                Process p = Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
        }

        static void Main(string[] args)
        {
            string r, w;
            try
            {
                r = args[0];
                w = args[1];
                TestExec exec = new TestExec();
                exec.startServer();
                exec.startWpfClient(r, w);
            }
            catch (Exception)
            {

            }
            

        }
    }
}
