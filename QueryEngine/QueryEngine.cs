///////////////////////////////////////////////////////////////
// QueryEngine.cs - Support queries for our NoSqlDB          //
//                  Requirement 7 of Project #2              //
// Ver 1.0                                                   //
// Application: Query processing for CSE681-SMA, Project#2   //
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
 * This package provides the language for queries, using 
 * lambda predicates.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   DBElement.cs, DBEngine, DBFactory.cs, Display
 *   PersistEngine.cs, QueryEngine.cs, UtilityExtensions.cs 
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
    public class QueryEngine
    {
        DateTime dtime;

        //----< Default Public Constructor >----------------------------------------------------
        public QueryEngine()
        {
            dtime = default(DateTime);
            DateTime temp = dtime;
        }

        //----< Query for value of a given key >------------------------------------------------
        public static Value queryValue<Key, Value, Data>(Key key, DBEngine<Key, Value> db)
        {
            Value value = default(Value);
            if (db.Keys().Contains(key))
            {
                db.getValue(key, out value);
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                return value;
            }
            else
            {
                "Key doesn't exist!".error();
                return value;
            }   
        }

        //----< build queryPredicate for querying children >------------------------------------
        /*
         * Query returns true if query(key) condition is satisfied.
         * In this example the query is checking to see if the query
         * parameter, the captured string, test, is a substring of
         * the database element referenced by key.
         */
        public Func<Key, bool> queryChildren<Key, Value, Data>(DBEngine<Key, Value> db)
        {
            Func<Key, bool> queryPredicate = (Key key) =>
            {
                if (db.Keys().Contains(key))
                {
                    return true;
                }
                return false;
            };
            return queryPredicate;
        }

        //----< process children query using queryPredicate >-----------------------------------
        public bool processChildrenQuery<Key, Value, Data>(Func<Key, bool> queryPredicate, 
            out List<Key> keyCollection, Key searchKey, DBEngine<Key, Value> db)
        {
            keyCollection = new List<Key>();
            Value value;
            if (db.Keys().Contains(searchKey))
            {
                db.getValue(searchKey, out value);
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                foreach (Key child in elem.children)
                {
                    if (queryPredicate(child))
                    {
                        keyCollection.Add(child);
                    }
                }
            }

            if (keyCollection.Count() > 0)
                return true;
            return false;
        }

        //----< build queryPredicate for metadata query >---------------------------------------
        /*
         * Query returns true if query(key) condition is satisfied.
         * In this example the query is checking to see if the query
         * parameter, the captured string, test, is a substring of
         * the database element referenced by key.
         */
        public Func<Key, bool> queryMetadata<Key, Value, Data>(string queryParam, DBEngine<Key, Value> db)
        {
            // Func<int, bool> is delegate that binds to 
            // functions of the form bool query(int key).

            Func<Key, bool> queryPredicate = (Key key) =>
            {
                Value value;
                db.getValue(key, out value);
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                if (elem.name.Contains(queryParam))
                {
                    return true;
                }
                if (elem.descr.Contains(queryParam))
                {
                    return true;
                }
                if (elem.timeStamp.ToString().Contains(queryParam))
                {
                    return true;
                }
                foreach (Key child in elem.children)
                {
                    if (child.ToString().Contains(queryParam))
                    {
                        return true;
                    }
                }
                return false;
            };
            return queryPredicate;
        }

        //----< build queryPredicate for timestamp query >---------------------------------------
        public Func<Key, bool> queryTimestamp<Key, Value, Data>(DBEngine<Key, Value> db, 
            DateTime initial, DateTime final=default(DateTime))
        {
            Func<Key, bool> queryPredicate = (Key key) =>
            {
                Value value;
                db.getValue(key, out value);
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;

                int init = elem.timeStamp.CompareTo(initial);
                if (final == default(DateTime))
                {
                    final = DateTime.Now;
                }
                int fin = elem.timeStamp.CompareTo(final);

                if ( init >= 0 && fin <= 0 )
                {
                    return true;
                }
                
                return false;
            };
            return queryPredicate;
        }

        //----< build queryPredicate for regex query >---------------------------------------
        public Func<Key, bool> queryRegEx<Key, Value, Data>(DBEngine<Key, Value> db, string regEx = ".*")
        {
            Func<Key, bool> queryPredicate = (Key key) =>
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(key.ToString(), regEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase) )
                {
                    return true;
                }

                return false;
            };
            return queryPredicate;
        }

        //----< process query using queryPredicate >---------------------------------------------
        public bool processQuery<Key, Value>(Func<Key, bool> queryPredicate,
            out List<Key> keyCollection, DBEngine<Key, Value> db)
        {
            /*
            * step through all the keys in the db to see if
            * the queryPredicate is true for one or more keys.
            */
            keyCollection = new List<Key>();
            foreach (Key key in db.Keys())
            {
                if (queryPredicate(key))
                {
                    keyCollection.Add(key);
                }
            }

            if (keyCollection.Count() > 0)
                return true;
            return false;
        }

        //----< show query results >-------------------------------------------------------------
        public DBFactory<Key, Value> queryResults<Key, Value, Data>(bool result,
            List<Key> keyCollection, DBEngine<Key, Value> db)
        {
            DBEngine<Key, Value> newDb = new DBEngine<Key, Value>();
            if (result) // query succeeded for at least one key
            {
                Value value;
                foreach (Key key in keyCollection)
                {
                    db.getValue(key, out value);
                    newDb.insert(key, value);
                }
                DBFactory<Key, Value> dbFactory = new DBFactory<Key, Value>(newDb);
                return dbFactory;
            }
            else
            {
                DBFactory<Key, Value> dbFactory = new DBFactory<Key, Value>(newDb);
                return dbFactory;
            }
        }

    }

#if (QUERYENGINETEST)
    class QueyEngineTest
    {
        DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
        static void Main(string[] args)
        {
            QueyEngineTest test = new QueyEngineTest();
            QueryEngine qEngine = new QueryEngine();

            "Creating & Populating DB".title2();
            test.TestCreate();
            test.TestAdd(1);
            test.TestAdd(2);
            test.TestAdd(4);
            test.TestAdd(3);
            test.TestAdd(5);
            test.TestAdd(15);
            test.TestRemove(6);
            test.TestRemove(4);
            WriteLine();

            "Query by Key for value".title2();
            DBElement<int, string> value = QueryEngine.queryValue<int, DBElement<int, string>, string>(2, test.db);
            "Querying for value of Key = 2".body();
            string status = "Result of query: ";
            status.success();
            value.showElement();
            WriteLine();

            "Query by Key for children".title2();
            "Querying for children of Key = 1".body();
            QueryEngine queryEngine = new QueryEngine();
            DBFactory<int, DBElement<int, string>> newDB;
            // build query
            Func<int, bool> query = queryEngine.queryChildren<int, DBElement<int, string>, string>(test.db);
            // process query
            List<int> keyCollection;
            bool result = queryEngine.processChildrenQuery<int, DBElement<int, string>, string>(query, out keyCollection, 1, test.db);
            newDB = queryEngine.queryResults<int, DBElement<int, string>, string>(result, keyCollection, test.db);
            newDB.showDB();
            WriteLine();

            "Query by KeyPattern for matching Keys".title2();
            "Querying for pattern \"1.*\" ".body();
            DBFactory<int, DBElement<int, string>> newDB2;
            // build query
            string regEx = "1.*";
            query = queryEngine.queryRegEx<int, DBElement<int, string>, string>(test.db, regEx);
            // process query
            result = queryEngine.processQuery<int, DBElement<int, string>>(query, out keyCollection, test.db);
            newDB2 = queryEngine.queryResults<int, DBElement<int, string>, string>(result, keyCollection, test.db);
            newDB2.showDB();
            WriteLine();

            // build query
            "Querying without suplying a pattern ( will be treated as \".*\") ".body();
            query = queryEngine.queryRegEx<int, DBElement<int, string>, string>(test.db);
            // process query
            result = queryEngine.processQuery<int, DBElement<int, string>>(query, out keyCollection, test.db);
            newDB = queryEngine.queryResults<int, DBElement<int, string>, string>(result, keyCollection, test.db);
            newDB.showDB();
            WriteLine();

            "Query by metadata for Keys".title2();
            "Querying for metadata substring \"of 2\" ".body();
            DBFactory<int, DBElement<int, string>> newDB3;
            // build query
            string search = "of 2";
            query = queryEngine.queryMetadata<int, DBElement<int, string>, string>(search, test.db);
            // process query
            result = queryEngine.processQuery<int, DBElement<int, string>>(query, out keyCollection, test.db);
            newDB3 = queryEngine.queryResults<int, DBElement<int, string>, string>(result, keyCollection, test.db);
            newDB3.showDB();
            WriteLine();
            "Querying for metadata substring \"of \" ".body();
            DBFactory<int, DBElement<int, string>> newDB4;
            // build query
            search = "of ";
            query = queryEngine.queryMetadata<int, DBElement<int, string>, string>(search, test.db);
            // process query
            result = queryEngine.processQuery<int, DBElement<int, string>>(query, out keyCollection, test.db);
            newDB4 = queryEngine.queryResults<int, DBElement<int, string>, string>(result, keyCollection, test.db);
            newDB4.showDB();


            "Query by timestamp for Keys".title2();
            DBFactory<int, DBElement<int, string>> newDB5;
            TimeSpan tspan = new TimeSpan(0, 0, 0, 0, 300);
            DateTime initial = DateTime.Now.Subtract(tspan);
            string message = "Querying for Keys added between " + initial.ToLongTimeString() + " and now";
            message += "\n\t(A timespan of " + tspan.TotalMilliseconds + " milliseconds)";
            WriteLine();
            message.body();
            WriteLine();
            // build query
            message = "\n\tsecond value of range not specified, current time assumed";
            WriteLine();
            message.body();
            query = queryEngine.queryTimestamp<int, DBElement<int, string>, string>(test.db, initial);
            // process query
            result = queryEngine.processQuery<int, DBElement<int, string>>(query, out keyCollection, test.db);
            newDB5 = queryEngine.queryResults<int, DBElement<int, string>, string>(result, keyCollection, test.db);
            newDB5.showDB();
            WriteLine();
        }

        //----< Create & Populate DB >--------------------------------------
        void TestCreate()
        {
            "Demonstrating Requirement #2 (DB <int, string>)".title();
            DBElement<int, string> elem = new DBElement<int, string>();
            elem.name = "element";
            elem.descr = "test element";
            elem.timeStamp = DateTime.Now;
            elem.children.AddRange(new List<int> { 2, 3 });
            elem.payload = "elem's payload";
            "Adding below element with Key = 1".title2();
            db.insert(1, elem);
            WriteLine();
        }

        //----< Add contents to DB >-----------------------------------------
        void TestAdd(int key)
        {
            string str = "Adding element with key " + key.ToString();
            str.title2();
            DBElement<int, string> elem = new DBElement<int, string>();
            elem.name = "Element" + key.ToString();
            elem.descr = "Description of " + key.ToString();
            elem.timeStamp = DateTime.Now;
            elem.children.AddRange(new List<int> { key - 1, key + 1, key + 2 });
            elem.payload = "Element " + key.ToString() + "'s payload";
            bool success = db.insert(key, elem);
            if (success)
            {
                "Element added successfully".success();
            }
            else "Key already exists".error();
            WriteLine();
        }

        //----< Remove contents from DB >------------------------------------
        void TestRemove(int key)
        {
            string str = "Removing element with key " + key.ToString();
            str.title2();
            bool success = db.remove(key);
            if (success)
            {
                "Element removed successfully".success();
            }
            else "Key does not exist".error();
            WriteLine();
        }
    }
#endif
}
