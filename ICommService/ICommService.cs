/////////////////////////////////////////////////////////////////////////
// ICommService.cs - Contract for WCF message-passing service          //
// Ver 1.2                                                             //
// Application: Demonstration for CSE681-SMA, Project #4               //
// Language:    C#, ver 6.0, Visual Studio 2015                        //
// Platform:    Dell Inspiron 7520, Core-i7, Windows 10                //
// Author:      Sampath T Janardhan (508899838), SU                    //
//              (315) 664-8206, storagar@syr.edu                       //
// Source:      Jim Fawcett, CST 4-187, Syracuse University            //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to System.ServiceModel
 * - Added using System.ServiceModel
 * - Added reference to System.Runtime.Serialization
 * - Added using System.Runtime.Serialization
 */
/*
 * Maintenance History:
 * --------------------
 * ver 1.2 : 17 Nov 2015
 * - added MessageID
 * ver 1.1 : 29 Oct 2015
 * - added comment in data contract
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RemoteNoSqlDB
{
    [ServiceContract (Namespace ="Project4Starter")]
    public interface ICommService
    {
        [OperationContract(IsOneWay = true)]
        void sendMessage(Message msg);
    }

    [DataContract]
    public class Message
    {
        public static int MessageID = 1;
        
        [DataMember]
        public int messageID { get; set; }
        [DataMember]
        public string fromUrl { get; set; }
        [DataMember]
        public string toUrl { get; set; }
        [DataMember]
        public string content { get; set; }  // will hold XML defining message information
        [DataMember]
        public int type { get; set; }

    }
}
