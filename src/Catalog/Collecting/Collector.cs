﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Collecting
{
    public abstract class Collector
    {
        static Collector()
        {
            ServicePointManager.DefaultConnectionLimit = 4;
            ServicePointManager.MaxServicePointIdleTime = 10000;
        }

        public async Task Run(Uri index, DateTime last)
        {
            using (CollectorHttpClient client = new CollectorHttpClient())
            {
                await Fetch(client, index, last);
            }
        }

        protected abstract Task Fetch(CollectorHttpClient client, Uri index, DateTime last);
    }
}
