﻿/*
 * Copyright 2013 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

namespace Splunk.Client.UnitTesting
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Splunk.Client;

    /// <summary>
    /// Tests the Index class
    /// </summary>
    [TestClass]
    public class IndexTest : TestHelper
    {
        /// <summary>
        /// The assert root
        /// </summary>
        private static string assertRoot = "Index assert: ";

        /// <summary>
        /// Polls the index until wither time runs down, or the event count
        /// matches the desired value.
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="value">The desired event count value</param>
        /// <param name="seconds">The number seconds to poll</param>
        private static void WaitUntilEventCount(Index index, int value, int seconds)
        {
            while (seconds > 0)
            {
                Thread.Sleep(1000); // 1000ms (1 second sleep)
                seconds = seconds - 1;
                //index.Refresh();
                var count = index.TotalEventCount;
                if (count == value)
                {
                    return;
                }
            }

            Assert.Fail("Count did not reach the expected in alloted time.");
        }

        /// <summary>
        /// Tests the basic getters and setters of index
        /// </summary>
        [TestMethod]
        public void IndexAccessors()
        {
            string indexName = "sdk-tests2";
            Service service = Connect();
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now);
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss") +
                         string.Format("{0}{1} ", offset.Offset.Hours.ToString("D2"),
                             offset.Offset.Minutes.ToString("D2"));

            ServerInfo info = service.Server.GetInfoAsync().Result;

            //// set can_delete if not set, so we can delete events from the index.
            //User user = service.GetUsers().Get("admin");
            //string[] roles = user.Roles;
            //if (!this.Contains(roles, "can_delete"))
            //{
            //    string[] newRoles = new string[roles.Length + 1];
            //    roles.CopyTo(newRoles, 0);
            //    newRoles[roles.Length] = "can_delete";
            //    user.Roles = newRoles;
            //    user.Update();
            //}

            IndexCollection indexes = service.GetIndexesAsync().Result;
            foreach (Index idx in indexes)
            {
                int dummyInt;
                string dummyString;
                bool dummyBool;
                DateTime dummyTime;
                dummyBool = idx.AssureUTF8;
                dummyString = idx.BlockSignatureDatabase;
                dummyInt = idx.BlockSignSize;
                dummyInt = idx.BloomFilterTotalSizeKB;
                dummyString = idx.ColdPath;
                dummyString = idx.ColdPathExpanded;
                dummyString = idx.ColdToFrozenDir;
                dummyString = idx.ColdToFrozenScript;
                //dummyBool = idx.CompressRawdata;
                dummyInt = idx.CurrentDBSizeMB;
                dummyString = idx.DefaultDatabase;
                dummyBool = idx.EnableRealtimeSearch;
                dummyInt = idx.FrozenTimePeriodInSecs;
                dummyString = idx.HomePath;
                dummyString = idx.HomePathExpanded;
                dummyString = idx.IndexThreads;
                long time = idx.LastInitTime;
                dummyString = idx.MaxBloomBackfillBucketAge;
                dummyInt = idx.MaxConcurrentOptimizes;
                dummyString = idx.MaxDataSize;
                dummyInt = idx.MaxHotBuckets;
                dummyInt = idx.MaxHotIdleSecs;
                dummyInt = idx.MaxHotSpanSecs;
                dummyInt = idx.MaxMemMB;
                dummyInt = idx.MaxMetaEntries;
                dummyInt = idx.MaxRunningProcessGroups;
                dummyTime = idx.MaxTime;
                dummyInt = idx.MaxTotalDataSizeMB;
                dummyInt = idx.MaxWarmDBCount;
                dummyString = idx.MemPoolMB;
                dummyString = idx.MinRawFileSyncSecs;
                dummyTime = idx.MinTime;
                //dummyInt = idx.NumBloomfilters;
                //dummyInt = idx.NumHotBuckets;
                //dummyInt = idx.NumWarmBuckets;
                dummyInt = idx.PartialServiceMetaPeriod;
                dummyInt = idx.QuarantineFutureSecs;
                dummyInt = idx.QuarantinePastSecs;
                dummyInt = idx.RawChunkSizeBytes;
                dummyInt = idx.RotatePeriodInSecs;
                dummyInt = idx.ServiceMetaPeriod;
                //dummyString = idx.SuppressBannerList;
                bool sync = idx.Sync;
                dummyBool = idx.SyncMeta;
                dummyString = idx.ThawedPath;
                dummyString = idx.ThawedPathExpanded;
                dummyInt = idx.ThrottleCheckPeriod;
                long eventCount = idx.TotalEventCount;
                dummyBool = idx.Disabled;
                dummyBool = idx.IsInternal;
            }


            try
            {
                var x = indexes.GetIndexAsync(indexName).Result;

                if (indexes.GetIndexAsync(indexName).Result == null)
                {
                    indexes.CreateIndexAsync(indexName, new IndexArgs("", "", ""), new IndexAttributes()).Wait();
                    //indexes.Refresh();
                }
            }
            catch (Exception e)
            {
                indexes.CreateIndexAsync(indexName, new IndexArgs("", "", ""), new IndexAttributes()).Wait();
            }


            Assert.IsNotNull(indexes.GetIndexAsync(indexName).Result, assertRoot + "#1");

            Index index = indexes.GetIndexAsync(indexName).Result;

            IndexAttributes indexAttributes = GetIndexAttributes(index);

            // use setters to update most


            indexAttributes.BlockSignSize = index.BlockSignSize + 1;

            
            if (this.VersionCompare(service,"4.3") > 0)
            {
                indexAttributes.EnableOnlineBucketRepair = !index.EnableOnlineBucketRepair;
                indexAttributes.MaxBloomBackfillBucketAge = "20d";
            }

            indexAttributes.FrozenTimePeriodInSecs = index.FrozenTimePeriodInSecs + 1;
            indexAttributes.MaxConcurrentOptimizes = index.MaxConcurrentOptimizes + 1;
            indexAttributes.MaxDataSize = "auto";
            indexAttributes.MaxHotBuckets = index.MaxHotBuckets + 1;
            indexAttributes.MaxHotIdleSecs = index.MaxHotIdleSecs + 1;
            indexAttributes.MaxMemMB = index.MaxMemMB + 1;
            indexAttributes.MaxMetaEntries = index.MaxMetaEntries + 1;
            indexAttributes.MaxTotalDataSizeMB = index.MaxTotalDataSizeMB + 1;
            indexAttributes.MaxWarmDBCount = index.MaxWarmDBCount + 1;
            indexAttributes.MinRawFileSyncSecs = "disable";
            indexAttributes.PartialServiceMetaPeriod = index.PartialServiceMetaPeriod + 1;
            indexAttributes.QuarantineFutureSecs = index.QuarantineFutureSecs + 1;
            indexAttributes.QuarantinePastSecs = index.QuarantinePastSecs + 1;
            indexAttributes.RawChunkSizeBytes = index.RawChunkSizeBytes + 1;
            indexAttributes.RotatePeriodInSecs = index.RotatePeriodInSecs + 1;
            indexAttributes.ServiceMetaPeriod = index.ServiceMetaPeriod + 1;
            indexAttributes.SyncMeta = !index.SyncMeta;
            indexAttributes.ThrottleCheckPeriod = index.ThrottleCheckPeriod + 1;
            
            //indexAttributes.Update();
            index.UpdateAsync(indexAttributes).Wait();

           // check, then restore using map method
            //index.Refresh();

            ClearIndex(service, indexName, index);

            //index.Disable();            
            Assert.IsTrue(index.Disabled);

            this.SplunkRestart();

            service = this.Connect();
            index = service.GetIndexAsync(indexName).Result;
            //user = service.GetUsers().Get("admin");

            //index.Enable();
            Assert.IsFalse(index.Disabled);

            //// Restore original roles
            //user.Roles = roles;
            //user.Update();
        }

        /// <summary>
        /// Gets old values from given index, skip saving paths and things we cannot write
        /// </summary>
        /// <param name="index">The Index</param>
        /// <returns>The argument getIndexProperties</returns>
        private IndexAttributes GetIndexAttributes(Index index)
        {
            IndexAttributes indexAttributes = new IndexAttributes();

            indexAttributes.BlockSignSize = index.BlockSignSize;
            indexAttributes.FrozenTimePeriodInSecs = index.FrozenTimePeriodInSecs;
            indexAttributes.MaxConcurrentOptimizes = index.MaxConcurrentOptimizes;
            indexAttributes.MaxDataSize = index.MaxDataSize;
            indexAttributes.MaxHotBuckets = index.MaxHotBuckets;
            indexAttributes.MaxHotIdleSecs = index.MaxHotIdleSecs;
            indexAttributes.MaxHotSpanSecs = index.MaxHotSpanSecs;
            indexAttributes.MaxMemMB = index.MaxMemMB;
            indexAttributes.MaxMetaEntries = index.MaxMetaEntries;
            indexAttributes.MaxTotalDataSizeMB = index.MaxTotalDataSizeMB;
            indexAttributes.MaxWarmDBCount = index.MaxWarmDBCount;
            indexAttributes.MinRawFileSyncSecs = index.MinRawFileSyncSecs;
            indexAttributes.PartialServiceMetaPeriod = index.PartialServiceMetaPeriod;
            indexAttributes.QuarantineFutureSecs = index.QuarantineFutureSecs;
            indexAttributes.QuarantinePastSecs = index.QuarantinePastSecs;
            indexAttributes.RawChunkSizeBytes = index.RawChunkSizeBytes;
            indexAttributes.RotatePeriodInSecs = index.RotatePeriodInSecs;
            indexAttributes.ServiceMetaPeriod = index.ServiceMetaPeriod;
            indexAttributes.SyncMeta = index.SyncMeta;
            indexAttributes.ThrottleCheckPeriod = index.ThrottleCheckPeriod;

            return indexAttributes;
        }

        /// <summary>
        /// Tests submitting and streaming events to an index 
        /// and also removing all events from the index
        /// </summary>
        [TestMethod]
        public void IndexEvents()
        {
            string indexName = "sdk-tests2";
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now);
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss") +
                         string.Format("{0}{1} ", offset.Offset.Hours.ToString("D2"),
                             offset.Offset.Minutes.ToString("D2"));

            Service service = this.Connect();
            ServerInfo info = service.Server.GetInfoAsync().Result;
            Index index = service.GetIndexAsync(indexName).Result;

            if (index.TotalEventCount > 0)
            {
                //index.Clean(20);
            }

            Assert.AreEqual(0, index.TotalEventCount, assertRoot + "#1");

            ClearIndex(service, indexName, index);

            // submit events to index
            //index.Submit(now + " Hello World. \u0150");
            //index.Submit(now + " Goodbye world. \u0150");
            //WaitUntilEventCount(index, 2, 45);

            ClearIndex(service, indexName, index);

            //// stream events to index
            //Stream stream = index.Attach();
            //stream.Write(Encoding.UTF8.GetBytes(now + " Hello World again. \u0150\r\n"));
            //stream.Write(Encoding.UTF8.GetBytes(now + " Goodbye World again.\u0150\r\n"));
            //stream.Close();
            WaitUntilEventCount(index, 2, 45);

            ClearIndex(service, indexName, index);
            //index.Clean(180);
            Assert.AreEqual(0, index.TotalEventCount, assertRoot + "#6");

            string filename;
            if (info.OSName.Equals("Windows"))
            {
                filename = "C:\\Windows\\WindowsUpdate.log"; // normally here
            }
            else if (info.OSName.Equals("Linux"))
            {
                filename = "/var/log/syslog";
            }
            else if (info.OSName.Equals("Darwin"))
            {
                filename = "/var/log/system.log";
            }
            else
            {
                throw new Exception("OS: " + info.OSName + " not supported");
            }

            //try
            //{
            //    index.Upload(filename);
            //}
            //catch (Exception e)
            //{
            //    throw new Exception("File " + filename + "failed to upload: Exception -> " + e.Message);
            //}

            ClearIndex(service, indexName, index);
        }

        /// <summary>
        /// Tests submitting and streaming events to a default index 
        /// and also removing all events from the index
        /// </summary>
        [TestMethod]
        public void DefaultIndex()
        {
            string indexName = "main";
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now);
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss") +
                         string.Format("{0}{1} ", offset.Offset.Hours.ToString("D2"),
                             offset.Offset.Minutes.ToString("D2"));

            Service service = this.Connect();
            //Receiver receiver = service.GetReceiver();
            //Index index = service.GetIndexes().Get(indexName);
            //index.Enable();
            //Assert.IsFalse(index.Disabled);

            //// submit events to default index
            //receiver.Log(now + " Hello World. \u0150");
            //receiver.Log(now + " Goodbye World. \u0150");

            //// stream event to default index
            //Stream streamDefaultIndex = receiver.Attach();
            //streamDefaultIndex.Write(Encoding.UTF8.GetBytes(now + " Hello World again. \u0150\r\n"));
            //streamDefaultIndex.Write(Encoding.UTF8.GetBytes(now + " Goodbye World again.\u0150\r\n"));
            //streamDefaultIndex.Close();
        }

        /// <summary>
        /// Clear the index
        /// </summary>
        /// <param name="service">A service</param>
        /// <param name="indexName">The index name</param>
        /// <param name="index">The index object</param>
        private void ClearIndex(Service service, string indexName, Index index)
        {
            var result = service.SearchOneshotAsync(string.Format("search index={0} * | delete", indexName)).Result;

            //StreamReader reader = new StreamReader(stream);
            //string message = reader.ReadToEnd();

            //if (message.Contains("msg type=\"FATAL\""))
            //{
            //    throw new ApplicationException(string.Format("web reqest return error: {0}", message));
            //}

            WaitUntilEventCount(index, 0, 45);
        }

        /// <summary>
        /// Tests submitting and streaming events to an index given the indexAttributes argument
        /// and also removing all events from the index
        /// </summary>
        [TestMethod]
        public void IndexArgs()
        {
            string indexName = "sdk-tests2";
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now);
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss") +
                         string.Format("{0}{1} ", offset.Offset.Hours.ToString("D2"),
                             offset.Offset.Minutes.ToString("D2"));

            Service service = this.Connect();
            Index index = service.GetIndexAsync(indexName).Result;
                                    
            //index.Enable();
            Assert.IsFalse(index.Disabled);

            IndexAttributes indexAttributes = GetIndexAttributes(index);

            ClearIndex(service, indexName, index);

            // submit event to index using variable arguments
            //index.Submit(indexAttributes, now + " Hello World. \u0150");
            //index.Submit(indexAttributes, now + " Goodbye World. \u0150");
            //WaitUntilEventCount(index, 2, 45);

            ClearIndex(service, indexName, index);

            // stream event to index with variable arguments
            //Stream streamArgs = index.Attach(indexAttributes);
            //streamArgs.Write(Encoding.UTF8.GetBytes(now + " Hello World again. \u0150\r\n"));
            //streamArgs.Write(Encoding.UTF8.GetBytes(now + " Goodbye World again.\u0150\r\n"));
            //streamArgs.Close();
            //WaitUntilEventCount(index, 2, 45);

            // submit event using ReceiverSubmitArgs
            const string Source = "splunk-sdk-tests";
            const string SourceType = "splunk-sdk-test-event";
            const string Host = "test-host";
            var args = new ReceiverArgs
            {
                index = indexName,
                host = Host,
                source = Source,
                //sourceType = SourceType,
            };

            //var receiver = service.GetReceiver();
            //receiver.Submit(args, "Hello World.");
            //receiver.Submit(args, "Goodbye world.");
            //WaitUntilEventCount(index, 4, 45);
            //// verify the fields of events in the index matching the args.
            //using (var stream =
            //    service.Oneshot(
            //        string.Format(
            //            "search index={0} host={1} source={2} sourcetype={3}",
            //            indexName,
            //            Host,
            //            Source,
            //            SourceType)))
            //using (var reader = new ResultsReaderXml(stream))
            //{
            //    Assert.AreEqual(2, reader.Count());
            //}

            //ClearIndex(service, indexName, index);
            //index.Clean(180);
            //Assert.AreEqual(0, index.TotalEventCount, "Expected the total event count to be 0");
        }

        /// <summary>
        /// Test submitting and streaming to a default index given the indexAttributes argument
        /// and also removing all events from the index
        /// </summary>
        [TestMethod]
        public void DefaultIndexArgs()
        {
            string indexName = "main";
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now);
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss") +
                         string.Format("{0}{1} ", offset.Offset.Hours.ToString("D2"),
                             offset.Offset.Minutes.ToString("D2"));

            Service service = this.Connect();
            //Receiver receiver = service.GetReceiver();
            Index index = service.GetIndexAsync(indexName).Result;

            //index.Enable();
            Assert.IsFalse(index.Disabled);

            IndexAttributes indexAttributes = GetIndexAttributes(index);

            // submit event to default index using variable arguments
            //receiver.Log(indexAttributes, "Hello World. \u0150");
            //receiver.Log(indexAttributes, "Goodbye World. \u0150");

            // stream event to default index with variable arguments
            //Stream streamArgs = receiver.Attach(indexAttributes);
            //streamArgs.Write(Encoding.UTF8.GetBytes(" Hello World again. \u0150\r\n"));
            //streamArgs.Write(Encoding.UTF8.GetBytes(" Goodbye World again.\u0150\r\n"));
            //streamArgs.Close();
        }
    }
}
