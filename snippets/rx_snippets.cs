//Below are methods that will be translated into snippets with placeholders. 
//You can run these by grabbing the existing SDK and running a local Splunk instance.
//
//Each method (aside from Main and Login) will have a snippet associated.
//please give us feedback on the RX code / and suggest any changes.
//
//You can fork the code OR just give feedback in the comments.
//
//Thanks!


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Splunk.Client;
using System.Threading.Tasks;
using System.Reactive;

namespace WorkOnTemplates
{
    public class Main
    {
        static void Main(string[] args)
        {
            using (var service = new Service(Scheme.Https, "localhost", 8089))
            {
                Run(service).Wait();
            }
        }


        static async Task Run(Service service)
        {
            //// Login
            await service.LoginAsync("admin", "changeme");
        }

        static async Task ListSavedSearches(Service service)
        {
            foreach (SavedSearch savedSearch in await service.GetSavedSearchesAsync())
            {
                // Insert code here
            }
        }

        static async Task ListJobs(Service service)
        {
            foreach (Job job in await service.GetJobsAsync())
            {
                // Insert code here
            }
        }

        static async Task CreateSavedSearch(Service service)
        {
            SavedSearchAttributes savedSearchAttributes = new SavedSearchAttributes();
            // For a full list of attributes, see
            //
            //     http://docs.splunk.com/Documentation/Splunk/latest/RESTAPI/RESTsearch#POST_saved.2Fsearches
            // savedSearchAttributes.Description = "Human readable description of this saved search";
            // savedSearchAttributes.IsScheduled = true;
            // savedSearchAttributes.CronSchedule = "*/5 * * * *";

            // The Search attribute is required when creating a saved search.   
            savedSearchAttributes.Search = "search index=_internal | head 5";
            SavedSearch savedSearch = await service.CreateSavedSearchAsync("name", savedSearchAttributes);
        }

        static async Task DispatchSavedSearch(Service service)
        {
            SavedSearch savedSearch = await service.GetSavedSearchAsync("name");
            Task<Job> dispatchedJobTask = savedSearch.DispatchAsync();

            // Anything else you may need
            Job dispatchedJob = await dispatchedJobTask;
            while (!dispatchedJob.IsDone)
            {
                // Refresh
                // Sleep
            }
        }

        static async Task IterateOverResults(Service service)
        {
            JobArgs args = new JobArgs("search index=_internal | head 10");
            Job job = await service.StartJobAsync(args);
            using (SearchResults searchResults = await job.GetSearchResultsAsync())
            {
                try
                {
                    foreach (var record in searchResults)
                    {
                        Console.WriteLine(string.Format("{0}", record));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("SearchResults error: {0}", e.Message));
                }
            }
        }

        static async Task RxIterateOverResults(Service service)
        {
            // This snippet requires the Rx NuGet package.
            JobArgs args = new JobArgs("search index=_internal | head 10");
            Job job = await service.StartJobAsync(args);

            SearchResults searchResults = await job.GetSearchResultsAsync();
            searchResults.Subscribe(
                onCompleted: () =>
                {
                    Console.WriteLine("Done!");
                    // Clean up from iteration.
                    searchResults.Dispose(); // Clean up the SearchResults object to avoid memory leaks
                },
                onError: (Exception e) =>
                {
                    Console.WriteLine("Error: " + e.Message);
                },
                onNext: (Result r) =>
                {
                    Console.WriteLine(r);
                }
            );
        }

        static async Task RunOneshot(Service service)
        {
            JobArgs args = new JobArgs("search index=_internal | head 10");
            using (SearchResults searchResults = await service.SearchOneshotAsync(args))
            {
                try
                {
                    foreach (var record in searchResults)
                    {
                        Console.WriteLine(string.Format("{0}", record));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("SearchResults error: {0}", e.Message));
                }
            }
        }

        static async Task RxRunOneshot(Service service)
        {
            // This snippet requires the Rx NuGet package.
            JobArgs args = new JobArgs("search index=_internal | head 10");
            SearchResults searchResults = await service.SearchOneshotAsync(args);
            searchResults.Subscribe(
                onCompleted: () =>
                {
                    Console.WriteLine("Done!");
                    // Clean up from iteration.
                    searchResults.Dispose(); // Clean up the SearchResults object to avoid memory leaks
                },
                onError: (Exception e) =>
                {
                    Console.WriteLine("Error: " + e.Message);
                },
                onNext: (Result r) =>
                {
                    Console.WriteLine(r);
                }
            );
        }

        static async Task RunRealtime(Service service)
        {
            JobArgs args = new JobArgs("search index=_internal | head 10");
            args.ExecutionMode = ExecutionMode.Normal;
            args.SearchMode = SearchMode.Realtime;
            args.EarliestTime = "rt-1m";
            args.LatestTime = "rt";
            args.StatusBuckets = 300; // Enable the timeline on this job

            Job job = await service.StartJobAsync(args);

            // TODO: Wait for job to be ready once we implement that in SDK

            try
            {
                while (true)
                {
                    using (SearchResults searchResults = await job.GetSearchResultsPreviewAsync())
                    {
                        foreach (var record in searchResults)
                        {
                            Console.WriteLine(string.Format("{0}", record));
                        }
                    }

                    await Task.Delay(2000); // Sleep for 2 seconds
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("SearchResults error: {0}", e.Message));
            }
        }

        static async Task RxRunRealtime(Service service)
        {
            // This snippet requires the Rx NuGet package.
            JobArgs args = new JobArgs("search index=_internal | head 10");
            args.ExecutionMode = ExecutionMode.Normal;
            args.SearchMode = SearchMode.Realtime;
            args.EarliestTime = "rt-1m";
            args.LatestTime = "rt";
            args.StatusBuckets = 300; // Enable the timeline on this job

            Job job = await service.StartJobAsync(args);

            // TODO: Wait for job to be ready once we implement that in SDK

            SearchResults searchResults = await job.GetSearchResultsAsync();
            searchResults.Subscribe(
                onCompleted: () =>
                {
                    Console.WriteLine("Done!");
                    // Clean up from iteration.
                    searchResults.Dispose(); // Clean up the SearchResults object to avoid memory leaks
                },
                onError: (Exception e) =>
                {
                    Console.WriteLine("Error: " + e.Message);
                    searchResults.Dispose();
                },
                onNext: (Result r) =>
                {
                    Console.WriteLine(r);
                }
            );
        }

        static async Task RunExport(Service service)
        {
            SearchExportArgs args = new SearchExportArgs("search index=_internal | head 10");
            try
            {
                using (SearchResultsReader searchResultsReader = await service.SearchExportAsync(args))
                {
                    foreach (SearchResults resultsSet in searchResultsReader)
                    {

                        foreach (var record in resultsSet)
                        {
                            Console.WriteLine(string.Format("{0}", record));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("SearchResults error: {0}", e.Message));
            }
        }

        static async Task RxRunExport(Service service)
        {
            SearchExportArgs args = new SearchExportArgs("search index=_internal | head 10");
            SearchResultsReader exportResultsReader = await service.SearchExportAsync(args);
            exportResultsReader.Subscribe(
                onCompleted: () =>
                {
                    Console.WriteLine("Finished iterating over results sets.");
                    exportResultsReader.Dispose();
                },
                onError: (Exception e) =>
                {
                    Console.WriteLine("Error iterating over results sets: {0}", e.Message);
                    exportResultsReader.Dispose();
                },
                onNext: (SearchResults resultSet) =>
                {
                    Console.WriteLine("Iterating over a results set.");
                    resultSet.Subscribe(
                           onCompleted: () =>
                           {
                               Console.WriteLine("Done!");
                               // Clean up from iteration.
                               resultSet.Dispose(); // Clean up the SearchResults object to avoid memory leaks
                           },
                           onError: (Exception e) =>
                           {
                               Console.WriteLine("Error: " + e.Message);
                               resultSet.Dispose();
                           },
                           onNext: (Result r) =>
                           {
                               Console.WriteLine(r);
                           }
                    );
                }
            );
        }

        static async Task TimeWindows(Service service)
        {
            JobArgs args = new JobArgs("search index=_internal | head 10");
            args.EarliestTime = "-15m@m";
            args.LatestTime = "now";

            JobArgs args2 = new JobArgs("search index=_internal | head 10");
            args2.EarliestTime = "-60m@m";
            args2.LatestTime = "now";

            JobArgs args3 = new JobArgs("search index=_internal | head 10");
            args3.EarliestTime = "-4h@h";
            args3.LatestTime = "now";

            JobArgs args4 = new JobArgs("search index=_internal | head 10");
            args4.EarliestTime = "-24h@h";
            args4.LatestTime = "now";

        }
    }
}
