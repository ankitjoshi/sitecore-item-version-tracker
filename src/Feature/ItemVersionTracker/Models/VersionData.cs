using System;
using System.Collections.Generic;

namespace Sitecore.SharedSource.ItemVersionTracker.Models
{
    public class VersionData
    {
        public Guid ItemGuid { get; set; }
        public string ItemPath { get; set; }
        public string ItemName { get; set; }
        public int TotalLanguages { get; set; }
        public bool IsPageItem { get; set; }
        public string EditorUrl { get; set; }
        public List<LanguageData> LanguageDataList { get; set; }
    }

    //******************************************************************************************************

    public class LanguageData
    {
        public string LanguageName { get; set; }
        public int VersionCount { get; set; }
        public string IsLatestVersionLive { get; set; }
        public string VersionNoLive { get; set; }
    }

    //******************************************************************************************************
    //******************************************************************************************************

}