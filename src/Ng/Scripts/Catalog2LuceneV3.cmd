@echo OFF
	
cd Ng

:Top
	echo "Starting job - #{Jobs.catalog2lucenev3.Title}"
	
	set NUGETJOBS_STORAGE_PRIMARY=#{Jobs.common.v3.Storage.Primary}

	title #{Jobs.catalog2lucenev3.Title}

    start /w Ng.exe catalog2lucene -source #{Jobs.common.v3.Source} -luceneDirectoryType azure -luceneStorageAccountName #{Jobs.common.v3.Storage.Primary.Name} -luceneStorageKeyValue #{Jobs.common.v3.Storage.Primary.Key} -luceneStorageContainer #{Jobs.catalog2lucenev3.LuceneContainer} -registration #{Jobs.catalog2lucenev3.Registration} -elasticsearchendpoint #{Jobs.common.v3.Logging.ElasticsearchEndpoint} -elasticsearchusername #{Jobs.common.v3.Logging.ElasticsearchUsername} -elasticsearchpassword #{Jobs.common.v3.Logging.ElasticsearchPassword} -verbose true -interval #{Jobs.common.v3.Interval}

	echo "Finished #{Jobs.catalog2lucenev3.Title}"

	goto Top