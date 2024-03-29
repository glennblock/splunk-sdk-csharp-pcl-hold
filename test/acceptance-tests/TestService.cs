﻿/*
 * Copyright 2014 Splunk, Inc.
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
    using Splunk.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Xunit;

    public class TestService : IUseFixture<AcceptanceTestingSetup>
    {
        [Trait("class", "Service")]
        [Fact]
        public void CanConstruct()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, Namespace.Default);
            Assert.Equal(service.ToString(), "https://localhost:8089/services");
        }

        #region Access Control

        [Trait("class", "Service: Access Control")]
        [Fact]
        public async Task CanLogin()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, Namespace.Default);
            await service.LoginAsync("admin", "changeme");

            Assert.NotNull(service.SessionKey);

            try
            {
                await service.LoginAsync("admin", "bad-password");
                Assert.False(true, string.Format("Expected: {0}, Actual: {1}", typeof(AuthenticationFailureException).FullName, "no exception"));
            }
            catch (AuthenticationFailureException e)
            {
                Assert.Equal(e.StatusCode, HttpStatusCode.Unauthorized);
                Assert.Equal(e.Details.Count, 1);
                Assert.Equal(e.Details[0], new Message(MessageType.Warning, "Login failed"));
            }
            catch (Exception e)
            {
                Assert.True(false, string.Format("Expected: {0}, Actual: {1}", typeof(AuthenticationFailureException).FullName, e.GetType().FullName));
            }
        }

        #endregion

        #region Applications

        [Trait("class", "Service: Applications")]
        [Fact]
        public async Task CanGetApps()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            var collection = await service.GetApplicationsAsync();
        }

        #endregion

        #region Configuration

        [Trait("class", "Service: Configuration")]
        [Fact]
        public async Task CanCrudConfiguration() // no delete operation is available
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            var fileName = string.Format("delete-me-{0:N}", Guid.NewGuid());

            //// Create

            var configuration = await service.CreateConfigurationAsync(fileName);

            //// Read

            configuration = await service.GetConfigurationAsync(fileName);

            //// Update the default stanza through a ConfigurationStanza object

            var defaultStanza = await configuration.UpdateStanzaAsync("default", new Argument("foo", "1"), new Argument("bar", "2"));
            await defaultStanza.UpdateAsync(new Argument("bar", "3"), new Argument("foobar", "4"));
            await defaultStanza.UpdateSettingAsync("foobar", "5");

            await defaultStanza.GetAsync(); // because the rest api does not return settings unless you ask for them
            Assert.Equal(3, defaultStanza.Count);
            List<ConfigurationSetting> settings;
            
            settings = defaultStanza.Select(setting => setting).Where(setting => setting.Name == "foo").ToList();
            Assert.Equal(1, settings.Count);
            Assert.Equal("1", settings[0].Value);

            settings = defaultStanza.Select(setting => setting).Where(setting => setting.Name == "bar").ToList();
            Assert.Equal(1, settings.Count);
            Assert.Equal("3", settings[0].Value);

            settings = defaultStanza.Select(setting => setting).Where(setting => setting.Name == "foobar").ToList();
            Assert.Equal(1, settings.Count);
            Assert.Equal("5", settings[0].Value);

            //// Create, read, update, and delete a stanza through the Service object

            await service.CreateConfigurationStanzaAsync(fileName, "stanza");

            await service.UpdateConfigurationSettingsAsync(fileName, "stanza", new Argument("foo", "1"), new Argument("bar", "2"));
            await service.UpdateConfigurationSettingAsync(fileName, "stanza", "bar", "3");

            var stanza = await service.GetConfigurationStanzaAsync(fileName, "stanza");

            settings = stanza.Select(setting => setting).Where(setting => setting.Name == "foo").ToList();
            Assert.Equal(1, settings.Count);
            Assert.Equal("1", settings[0].Value);

            settings = stanza.Select(setting => setting).Where(setting => setting.Name == "bar").ToList();
            Assert.Equal(1, settings.Count);
            Assert.Equal("3", settings[0].Value);

            await service.RemoveConfigurationStanzaAsync(fileName, "stanza");
        }

        [Trait("class", "Service: Configuration")]
        [Fact]
        public async Task CanGetConfigurations()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");
            
            var collection = await service.GetConfigurationsAsync();
        }

        [Trait("class", "Service: Configuration")]
        [Fact]
        public async Task CanReadConfigurations()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            //// Read the entire configuration system

            var configurations = await service.GetConfigurationsAsync();

            foreach (var configuration in configurations)
            {
                await configuration.GetAsync();

                foreach (ConfigurationStanza stanza in configuration)
                {
                    Assert.NotNull(stanza);
                    await stanza.GetAsync();
                }
            }
        }

        #endregion

        #region Indexes

        [Trait("class", "Service: Indexes")]
        [Fact]
        public async Task CanGetIndexes()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");
            var collection = service.GetIndexesAsync().Result;

            foreach (var entity in collection)
            {
                await entity.GetAsync();

                Assert.Equal(entity.ToString(), entity.Id.ToString());

                Assert.DoesNotThrow(() => { bool value = entity.AssureUTF8; });
                Assert.DoesNotThrow(() => { string value = entity.BlockSignatureDatabase; });
                Assert.DoesNotThrow(() => { int value = entity.BlockSignSize; });
                Assert.DoesNotThrow(() => { int value = entity.BloomFilterTotalSizeKB; });
                Assert.DoesNotThrow(() => { string value = entity.BucketRebuildMemoryHint; });
                Assert.DoesNotThrow(() => { string value = entity.ColdPath; });
                Assert.DoesNotThrow(() => { string value = entity.ColdPathExpanded; });
                Assert.DoesNotThrow(() => { string value = entity.ColdToFrozenDir; });
                Assert.DoesNotThrow(() => { string value = entity.ColdToFrozenScript; });
                Assert.DoesNotThrow(() => { int value = entity.CurrentDBSizeMB; });
                Assert.DoesNotThrow(() => { string value = entity.DefaultDatabase; });
                Assert.DoesNotThrow(() => { bool value = entity.Disabled; });
                Assert.DoesNotThrow(() => { Eai value = entity.Eai; });
                Assert.DoesNotThrow(() => { bool value = entity.EnableOnlineBucketRepair; });
                Assert.DoesNotThrow(() => { bool value = entity.EnableRealtimeSearch; });
                Assert.DoesNotThrow(() => { int value = entity.FrozenTimePeriodInSecs; });
                Assert.DoesNotThrow(() => { string value = entity.HomePath; });
                Assert.DoesNotThrow(() => { string value = entity.HomePathExpanded; });
                Assert.DoesNotThrow(() => { string value = entity.IndexThreads; });
                Assert.DoesNotThrow(() => { bool value = entity.IsInternal; });
                Assert.DoesNotThrow(() => { bool value = entity.IsReady; });
                Assert.DoesNotThrow(() => { bool value = entity.IsVirtual; });
                Assert.DoesNotThrow(() => { long value = entity.LastInitSequenceNumber; });
                Assert.DoesNotThrow(() => { long value = entity.LastInitTime; });
                Assert.DoesNotThrow(() => { string value = entity.MaxBloomBackfillBucketAge; });
                Assert.DoesNotThrow(() => { int value = entity.MaxBucketSizeCacheEntries; });
                Assert.DoesNotThrow(() => { int value = entity.MaxConcurrentOptimizes; });
                Assert.DoesNotThrow(() => { string value = entity.MaxDataSize; });
                Assert.DoesNotThrow(() => { int value = entity.MaxHotBuckets; });
                Assert.DoesNotThrow(() => { int value = entity.MaxHotIdleSecs; });
                Assert.DoesNotThrow(() => { int value = entity.MaxHotSpanSecs; });
                Assert.DoesNotThrow(() => { int value = entity.MaxMemMB; });
                Assert.DoesNotThrow(() => { int value = entity.MaxMetaEntries; });
                Assert.DoesNotThrow(() => { int value = entity.MaxRunningProcessGroups; });
                Assert.DoesNotThrow(() => { int value = entity.MaxRunningProcessGroupsLowPriority; });
                Assert.DoesNotThrow(() => { DateTime value = entity.MaxTime; });
                Assert.DoesNotThrow(() => { int value = entity.MaxTimeUnreplicatedNoAcks; });
                Assert.DoesNotThrow(() => { int value = entity.MaxTimeUnreplicatedWithAcks; });
                Assert.DoesNotThrow(() => { int value = entity.MaxTotalDataSizeMB; });
                Assert.DoesNotThrow(() => { int value = entity.MaxWarmDBCount; });
                Assert.DoesNotThrow(() => { string value = entity.MemPoolMB; });
                Assert.DoesNotThrow(() => { string value = entity.MinRawFileSyncSecs; });
                Assert.DoesNotThrow(() => { DateTime value = entity.MinTime; });
                Assert.DoesNotThrow(() => { int value = entity.PartialServiceMetaPeriod; });
                Assert.DoesNotThrow(() => { int value = entity.ProcessTrackerServiceInterval; });
                Assert.DoesNotThrow(() => { int value = entity.QuarantineFutureSecs; });
                Assert.DoesNotThrow(() => { int value = entity.QuarantinePastSecs; });
                Assert.DoesNotThrow(() => { int value = entity.RawChunkSizeBytes; });
                Assert.DoesNotThrow(() => { int value = entity.RepFactor; });
                Assert.DoesNotThrow(() => { int value = entity.RotatePeriodInSecs; });
                Assert.DoesNotThrow(() => { int value = entity.ServiceMetaPeriod; });
                Assert.DoesNotThrow(() => { bool value = entity.ServiceOnlyAsNeeded; });
                Assert.DoesNotThrow(() => { int value = entity.ServiceSubtaskTimingPeriod; });
                Assert.DoesNotThrow(() => { string value = entity.SummaryHomePathExpanded; });
                Assert.DoesNotThrow(() => { bool value = entity.Sync; });
                Assert.DoesNotThrow(() => { bool value = entity.SyncMeta; });
                Assert.DoesNotThrow(() => { string value = entity.ThawedPath; });
                Assert.DoesNotThrow(() => { string value = entity.ThawedPathExpanded; });
                Assert.DoesNotThrow(() => { int value = entity.ThrottleCheckPeriod; });
                Assert.DoesNotThrow(() => { long value = entity.TotalEventCount; });
                Assert.DoesNotThrow(() => { string value = entity.TStatsHomePath; });
                Assert.DoesNotThrow(() => { string value = entity.TStatsHomePathExpanded; });

                var sameEntity = await service.GetIndexAsync(entity.ResourceName.Title);

                Assert.Equal(entity.ResourceName, sameEntity.ResourceName);

                Assert.Equal(entity.AssureUTF8, sameEntity.AssureUTF8);
                Assert.Equal(entity.BlockSignatureDatabase, sameEntity.BlockSignatureDatabase);
                Assert.Equal(entity.BlockSignSize, sameEntity.BlockSignSize);
                Assert.Equal(entity.BloomFilterTotalSizeKB, sameEntity.BloomFilterTotalSizeKB);
                Assert.Equal(entity.BucketRebuildMemoryHint, sameEntity.BucketRebuildMemoryHint);
                Assert.Equal(entity.ColdPath, sameEntity.ColdPath);
                Assert.Equal(entity.ColdPathExpanded, sameEntity.ColdPathExpanded);
                Assert.Equal(entity.ColdToFrozenDir, sameEntity.ColdToFrozenDir);
                Assert.Equal(entity.ColdToFrozenScript, sameEntity.ColdToFrozenScript);
                Assert.Equal(entity.CurrentDBSizeMB, sameEntity.CurrentDBSizeMB);
                Assert.Equal(entity.DefaultDatabase, sameEntity.DefaultDatabase);
                Assert.Equal(entity.Disabled, sameEntity.Disabled);
                // Assert.Equal(entity.Eai, sameEntity.Eai); // TODO: verify this property setting (?)
                Assert.Equal(entity.EnableOnlineBucketRepair, sameEntity.EnableOnlineBucketRepair);
                Assert.Equal(entity.EnableRealtimeSearch, sameEntity.EnableRealtimeSearch);
                Assert.Equal(entity.FrozenTimePeriodInSecs, sameEntity.FrozenTimePeriodInSecs);
                Assert.Equal(entity.HomePath, sameEntity.HomePath);
                Assert.Equal(entity.HomePathExpanded, sameEntity.HomePathExpanded);
                Assert.Equal(entity.IndexThreads, sameEntity.IndexThreads);
                Assert.Equal(entity.IsInternal, sameEntity.IsInternal);
                Assert.Equal(entity.IsReady, sameEntity.IsReady);
                Assert.Equal(entity.IsVirtual, sameEntity.IsVirtual);
                Assert.Equal(entity.LastInitSequenceNumber, sameEntity.LastInitSequenceNumber);
                Assert.Equal(entity.LastInitTime, sameEntity.LastInitTime);
                Assert.Equal(entity.MaxBloomBackfillBucketAge, sameEntity.MaxBloomBackfillBucketAge);
                Assert.Equal(entity.MaxBucketSizeCacheEntries, sameEntity.MaxBucketSizeCacheEntries);
                Assert.Equal(entity.MaxConcurrentOptimizes, sameEntity.MaxConcurrentOptimizes);
                Assert.Equal(entity.MaxDataSize, sameEntity.MaxDataSize);
                Assert.Equal(entity.MaxHotBuckets, sameEntity.MaxHotBuckets);
                Assert.Equal(entity.MaxHotIdleSecs, sameEntity.MaxHotIdleSecs);
                Assert.Equal(entity.MaxHotSpanSecs, sameEntity.MaxHotSpanSecs);
                Assert.Equal(entity.MaxMemMB, sameEntity.MaxMemMB);
                Assert.Equal(entity.MaxMetaEntries, sameEntity.MaxMetaEntries);
                Assert.Equal(entity.MaxRunningProcessGroups, sameEntity.MaxRunningProcessGroups);
                Assert.Equal(entity.MaxRunningProcessGroupsLowPriority, sameEntity.MaxRunningProcessGroupsLowPriority);
                Assert.Equal(entity.MaxTime, sameEntity.MaxTime);
                Assert.Equal(entity.MaxTimeUnreplicatedNoAcks, sameEntity.MaxTimeUnreplicatedNoAcks);
                Assert.Equal(entity.MaxTimeUnreplicatedWithAcks, sameEntity.MaxTimeUnreplicatedWithAcks);
                Assert.Equal(entity.MaxTotalDataSizeMB, sameEntity.MaxTotalDataSizeMB);
                Assert.Equal(entity.MaxWarmDBCount, sameEntity.MaxWarmDBCount);
                Assert.Equal(entity.MemPoolMB, sameEntity.MemPoolMB);
                Assert.Equal(entity.MinRawFileSyncSecs, sameEntity.MinRawFileSyncSecs);
                Assert.Equal(entity.MinTime, sameEntity.MinTime);
                Assert.Equal(entity.PartialServiceMetaPeriod, sameEntity.PartialServiceMetaPeriod);
                Assert.Equal(entity.ProcessTrackerServiceInterval, sameEntity.ProcessTrackerServiceInterval);
                Assert.Equal(entity.QuarantineFutureSecs, sameEntity.QuarantineFutureSecs);
                Assert.Equal(entity.QuarantinePastSecs, sameEntity.QuarantinePastSecs);
                Assert.Equal(entity.RawChunkSizeBytes, sameEntity.RawChunkSizeBytes);
                Assert.Equal(entity.RepFactor, sameEntity.RepFactor);
                Assert.Equal(entity.RotatePeriodInSecs, sameEntity.RotatePeriodInSecs);
                Assert.Equal(entity.ServiceMetaPeriod, sameEntity.ServiceMetaPeriod);
                Assert.Equal(entity.ServiceOnlyAsNeeded, sameEntity.ServiceOnlyAsNeeded);
                Assert.Equal(entity.ServiceSubtaskTimingPeriod, sameEntity.ServiceSubtaskTimingPeriod);
                Assert.Equal(entity.SummaryHomePathExpanded, sameEntity.SummaryHomePathExpanded);
                Assert.Equal(entity.Sync, sameEntity.Sync);
                Assert.Equal(entity.SyncMeta, sameEntity.SyncMeta);
                Assert.Equal(entity.ThawedPath, sameEntity.ThawedPath);
                Assert.Equal(entity.ThawedPathExpanded, sameEntity.ThawedPathExpanded);
                Assert.Equal(entity.ThrottleCheckPeriod, sameEntity.ThrottleCheckPeriod);
                Assert.Equal(entity.TotalEventCount, sameEntity.TotalEventCount);
                Assert.Equal(entity.TStatsHomePath, sameEntity.TStatsHomePath);
                Assert.Equal(entity.TStatsHomePathExpanded, sameEntity.TStatsHomePathExpanded);
            }
        }

        [Trait("class", "Service: Indexes")]
        [Fact]
        public async Task CanCrudIndex()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            var indexName = string.Format("delete-me-{0:N}", Guid.NewGuid());
            Index index;

            // Create

            index = await service.CreateIndexAsync(indexName, new IndexArgs());
            Assert.Equal(true, index.EnableOnlineBucketRepair);

            // Read

            index = await service.GetIndexAsync(indexName);

            // Update

            var indexAttributes = new IndexAttributes()
            {
                EnableOnlineBucketRepair = false
            };

            await index.UpdateAsync(indexAttributes);
            Assert.Equal(false, index.EnableOnlineBucketRepair);

            // Delete

            await service.RemoveIndexAsync(indexName);
        }

        #endregion

        #region Saved Searches

        [Trait("class", "Service: Saved Searches")]
        [Fact]
        public async Task CanCrudSavedSearch()
        {
            var service = new Service(Scheme.Https, "localhost", 8089/*, new Namespace(user: "nobody", app: "search")*/);
            await service.LoginAsync("admin", "changeme");

            // Create
            
            var name = string.Format("delete-me-{0:N}", Guid.NewGuid());
            var attributes = new SavedSearchAttributes() { Search = "search index=_internal | head 1000" };

            var savedSearch = await service.CreateSavedSearchAsync(name, attributes);
            Assert.Equal(true, savedSearch.IsVisible);

            // Read
            
            savedSearch = await service.GetSavedSearchAsync(name);
            Assert.Equal(true, savedSearch.IsVisible);

            // Update

            attributes.IsVisible = false;
            attributes.ActionEmailBcc = "ljiang@splunk.com";
            attributes.ActionEmailCC = "dnoble@splunk.com";
            attributes.ActionEmailFrom = "fross@splunk.com";
            attributes.ActionEmailTo = "gblock@splunk.com, ineeman@splunk.com";

            savedSearch = await service.UpdateSavedSearchAsync(name, attributes);
            Assert.Equal(false, savedSearch.IsVisible);
            Assert.Equal("ljiang@splunk.com", savedSearch.Actions.Email.Bcc);
            Assert.Equal("dnoble@splunk.com", savedSearch.Actions.Email.CC);
            Assert.Equal("fross@splunk.com", savedSearch.Actions.Email.From);
            Assert.Equal("gblock@splunk.com, ineeman@splunk.com", savedSearch.Actions.Email.To);

            // Delete

            await savedSearch.RemoveAsync();
        }

        [Trait("class", "Service: Saved Searches")]
        [Fact]
        public async Task CanDispatchSavedSearch()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            Job job = await service.DispatchSavedSearchAsync("Splunk errors last 24 hours");
            SearchResults searchResults = await job.GetSearchResultsAsync();

            var records = new List<Splunk.Client.Result>(searchResults);
        }

        [Trait("class", "Service: Saved Searches")]
        [Fact]
        public async Task CanGetSavedSearchHistory()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            var attributes = new SavedSearchAttributes() { Search = "search index=_internal * earliest=-1m" };
            var name = string.Format("delete-me-{0:N}", Guid.NewGuid());
            var savedSearch = await service.CreateSavedSearchAsync(name, attributes);

            var jobHistory = await savedSearch.GetHistoryAsync();
            Assert.Equal(0, jobHistory.Count);

            Job job1 = await savedSearch.DispatchAsync();

            jobHistory = await savedSearch.GetHistoryAsync();
            Assert.Equal(1, jobHistory.Count);
            Assert.Equal(job1.Id, jobHistory[0].Id);
            Assert.Equal(job1.Name, jobHistory[0].Name);
            Assert.Equal(job1.Namespace, jobHistory[0].Namespace);
            Assert.Equal(job1.ResourceName, jobHistory[0].ResourceName);

            Job job2 = await savedSearch.DispatchAsync();

            jobHistory = await savedSearch.GetHistoryAsync();
            Assert.Equal(2, jobHistory.Count);
            Assert.Equal(1, jobHistory.Select(job => job).Where(job => job.Id.Equals(job1.Id)).Count());
            Assert.Equal(1, jobHistory.Select(job => job).Where(job => job.Id.Equals(job2.Id)).Count());

            await job1.CancelAsync();

            jobHistory = await savedSearch.GetHistoryAsync();
            Assert.Equal(1, jobHistory.Count);
            Assert.Equal(job2.Id, jobHistory[0].Id);

            await job2.CancelAsync();

            jobHistory = await savedSearch.GetHistoryAsync();
            Assert.Equal(0, jobHistory.Count);

            await savedSearch.RemoveAsync();
        }

        [Trait("class", "Service: Saved Searches")]
        [Fact]
        public async Task CanGetSavedSearches()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            var collection = await service.GetSavedSearchesAsync();
        }

        [Trait("class", "Service: Saved Searches")]
        [Fact]
        public async Task CanUpdateSavedSearch()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");

            await service.UpdateSavedSearchAsync("Errors in the last 24 hours", new SavedSearchAttributes() { IsVisible = false });
        }

        #endregion

        #region Search Jobs

        [Trait("class", "Service: Search Jobs")]
        [Fact]
        public async Task CanGetJob()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "nobody", app: "search"));
            await service.LoginAsync("admin", "changeme");
            Job job1 = null, job2 = null;

            job1 = await service.StartJobAsync("search index=_internal | head 100");
            await job1.GetSearchResultsAsync();
            await job1.GetSearchResultsEventsAsync();
            await job1.GetSearchResultsPreviewAsync();

            job2 = await service.GetJobAsync(job1.ResourceName.Title);
            Assert.Equal(job1.ResourceName.Title, job2.ResourceName.Title);
            Assert.Equal(job1.Name, job1.ResourceName.Title);
            Assert.Equal(job1.Name, job2.Name);
            Assert.Equal(job1.Sid, job1.Name);
            Assert.Equal(job1.Sid, job2.Sid);
            Assert.Equal(job1.Id, job2.Id);

            Assert.Equal(new SortedDictionary<string, Uri>().Concat(job1.Links), new SortedDictionary<string, Uri>().Concat(job2.Links));
        }

        [Trait("class", "Service: Search Jobs")]
        [Fact]
        public async Task CanGetJobs()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, new Namespace(user: "admin", app: "search"));
            await service.LoginAsync("admin", "changeme");
            
            var jobs = new Job[]
            {
                await service.StartJobAsync("search index=_internal | head 1000"),
                await service.StartJobAsync("search index=_internal | head 1000"),
                await service.StartJobAsync("search index=_internal | head 1000"),
                await service.StartJobAsync("search index=_internal | head 1000"),
                await service.StartJobAsync("search index=_internal | head 1000"),
            };

            JobCollection collection = null;
            Assert.DoesNotThrow(() => collection = service.GetJobsAsync().Result);
            Assert.NotNull(collection);
            Assert.Equal(collection.ToString(), collection.Id.ToString());

            foreach (var job in jobs)
            {
                Assert.Contains(job, collection, EqualityComparer<Job>.Default);
            }
        }

        [Trait("class", "Service: Search Jobs")]
        [Fact]
        public async Task CanStartSearch()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, Namespace.Default);
            await service.LoginAsync("admin", "changeme");

            var job = await service.StartJobAsync("search index=_internal | head 10");
            Assert.NotNull(job);
            var results = await job.GetSearchResultsAsync();
            Assert.NotNull(results);
            var records = new List<Result>(results);
            Assert.Equal(10, records.Count);
        }

        [Trait("class", "Service: Search Jobs")]
        [Fact]
        public async Task CanStartSearchExport()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, Namespace.Default);
            await service.LoginAsync("admin", "changeme");

            SearchResultsReader reader = await service.SearchExportAsync(new SearchExportArgs("search index=_internal | head 1000") { Count = 0 });
            var records = new List<Splunk.Client.Result>();

            foreach (var searchResults in reader)
            {
                records.AddRange(searchResults);
            }
        }

        [Trait("class", "Service: Search Jobs")]
        [Fact]
        public async Task CanStartSearchOneshot()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, Namespace.Default);
            await service.LoginAsync("admin", "changeme");

            SearchResults searchResults = await service.SearchOneshotAsync(new JobArgs("search index=_internal | head 100") { MaxCount = 100000 });
            var records = new List<Splunk.Client.Result>(searchResults);
        }

        #endregion

        #region System

        [Trait("class", "Service: System")]
        [Fact]
        public async Task CanGetServerInfo()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, Namespace.Default);
            var serverInfo = await service.Server.GetInfoAsync();

            Acl acl = serverInfo.Eai.Acl;
            Permissions permissions = acl.Permissions;
            int build = serverInfo.Build;
            string cpuArchitecture = serverInfo.CpuArchitecture;
            Guid guid = serverInfo.Guid;
            bool isFree = serverInfo.IsFree;
            bool isRealtimeSearchEnabled = serverInfo.IsRealtimeSearchEnabled;
            bool isTrial = serverInfo.IsTrial;
            IReadOnlyList<string> licenseKeys = serverInfo.LicenseKeys;
            IReadOnlyList<string> licenseLabels = serverInfo.LicenseLabels;
            string licenseSignature = serverInfo.LicenseSignature;
            LicenseState licenseState = serverInfo.LicenseState;
            Guid masterGuid = serverInfo.MasterGuid;
            ServerMode mode = serverInfo.Mode;
            string osBuild = serverInfo.OSBuild;
            string osName = serverInfo.OSName;
            string osVersion = serverInfo.OSVersion;
            string serverName = serverInfo.ServerName;
            Version version = serverInfo.Version;
        }

#if false
        [Trait("class", "Service: Server")]
        [Fact]
        public void CanRestartServer()
        {
            var service = new Service(Scheme.Https, "localhost", 8089, Namespace.Default);
            service.LoginAsync("admin", "changeme").Wait();
            service.Server.RestartAsync().Wait();
        }
#endif

        #endregion

        public void SetFixture(AcceptanceTestingSetup data)
        { }
    }
}
