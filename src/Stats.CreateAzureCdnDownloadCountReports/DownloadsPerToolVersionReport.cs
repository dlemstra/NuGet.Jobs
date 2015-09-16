// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stats.CreateAzureCdnDownloadCountReports
{
    public class DownloadsPerToolVersionReport
        : ReportBase
    {
        private const string _storedProcedureName = "[dbo].[SelectTotalDownloadCountsPerToolVersion]";
        private const int _defaultCommandTimeout = 1800; // 30 minutes max
        internal const string ReportName = "tools.v1.json";

        public DownloadsPerToolVersionReport(CloudStorageAccount cloudStorageAccount, string statisticsContainerName, SqlConnectionStringBuilder statisticsDatabase, SqlConnectionStringBuilder galleryDatabase)
            : base(cloudStorageAccount, statisticsContainerName, statisticsDatabase, galleryDatabase)
        {
        }

        public async Task Run()
        {
            var targetBlobContainer = await GetBlobContainer();

            Trace.TraceInformation("Generating Tools Download Count Report from {0}/{1} to {2}/{3}.", StatisticsDatabase.DataSource, StatisticsDatabase.InitialCatalog, CloudStorageAccount.Credentials.AccountName, StatisticsContainerName);

            // Gather download count data from statistics warehouse
            IReadOnlyCollection<ToolDownloadCountData> data;
            Trace.TraceInformation("Gathering Tools Download Counts from {0}/{1}...", StatisticsDatabase.DataSource, StatisticsDatabase.InitialCatalog);
            using (var connection = await StatisticsDatabase.ConnectTo())
            using (var transaction = connection.BeginTransaction(IsolationLevel.Snapshot))
            {
                data = (await connection.QueryWithRetryAsync<ToolDownloadCountData>(
                    _storedProcedureName, commandType: CommandType.StoredProcedure, transaction: transaction, commandTimeout: _defaultCommandTimeout)).ToList();
            }

            Trace.TraceInformation("Gathered {0} rows of data.", data.Count);

            if (data.Any())
            {
                // Group based on Package Id
                var grouped = data.GroupBy(p => p.ToolId);
                var registrations = new JArray();
                foreach (var group in grouped)
                {
                    var details = new JArray();
                    details.Add(group.Key);
                    foreach (var gv in group)
                    {
                        var version = new JArray(gv.ToolVersion, gv.TotalDownloadCount);
                        details.Add(version);
                    }
                    registrations.Add(details);
                }

                var blob = targetBlobContainer.GetBlockBlobReference(ReportName);
                Trace.TraceInformation("Writing report to {0}", blob.Uri.AbsoluteUri);
                blob.Properties.ContentType = "application/json";
                await blob.UploadTextAsync(registrations.ToString(Formatting.None));
                Trace.TraceInformation("Wrote report to {0}", blob.Uri.AbsoluteUri);
            }
        }
    }
}