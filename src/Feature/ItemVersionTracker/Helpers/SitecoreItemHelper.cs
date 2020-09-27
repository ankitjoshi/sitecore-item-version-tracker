using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.ItemVersionTracker.Models;

namespace Sitecore.SharedSource.ItemVersionTracker.Helpers
{
    public static class SitecoreItemHelper
    {
        //************************************************************************************************************************

        public static List<LanguageData> GetItemLanguages(Item itemToProcess, string filterBy, string selectedLanguage)
        {
            var objItemVersionList = new List<LanguageData>();
            try
            {
                var masterDb = Database.GetDatabase(Sitecore.Configuration.Settings.GetSetting("Sitecore.SharedSource.ItemVersionTracker.SourceDatabase"));
                var webDb = Database.GetDatabase(Sitecore.Configuration.Settings.GetSetting("Sitecore.SharedSource.ItemVersionTracker.TargetDatabase"));

                if (masterDb == null)
                {
                    Log.Info("*********************Sitecore Version Tracker: SourceDb is null **********************", "Item Version Tracker");
                    return new List<LanguageData>();
                }

                if (webDb == null)
                {
                    Log.Info("*********************Sitecore Version Tracker: TargetDb is null **********************", "Item Version Tracker");
                    return new List<LanguageData>();
                }

                Sitecore.Data.Items.Item contentRootItemFromSourceDb = masterDb.GetItem(itemToProcess.ID);

                var itemLanguages = contentRootItemFromSourceDb.Languages;
                if (selectedLanguage != "All")
                {
                    itemLanguages = itemLanguages.Where(x => x.Name == selectedLanguage).ToArray();
                }
                foreach (var language in itemLanguages)
                {
                    var liveVersion = 0;
                    // Get the version which has been published and available in live environment...
                    Sitecore.Data.Items.Item contentRootItemFromTargetDb = webDb.GetItem(itemToProcess.ID, language); //gets latest version from web(live)...
                    if (contentRootItemFromTargetDb != null)
                    {
                        var webVersionCount = contentRootItemFromTargetDb.Versions.Count;
                        liveVersion = 0;
                        if (webVersionCount != 0)
                        {
                            liveVersion = contentRootItemFromTargetDb.Version.Number;
                        }
                    }

                    // Get the latest version from Source Db...
                    Sitecore.Data.Items.Item objMasterItem = masterDb.GetItem(contentRootItemFromSourceDb.ID, language);

                    if (objMasterItem != null && objMasterItem.Versions.Count > 0)
                    {
                        var masterVersionCount = objMasterItem.Versions.Count;
                        var isLatestVersionLive = masterVersionCount == liveVersion ? "YES" : "NO";

                        objItemVersionList.Add(new LanguageData
                        {
                            LanguageName = language.Name,
                            VersionCount = masterVersionCount,
                            IsLatestVersionLive = isLatestVersionLive,
                            VersionNoLive = liveVersion.ToString()
                        });
                    }
                }

                switch (filterBy)
                {
                    case "0":
                        // Filter by items where latest version is not the live version...
                        return objItemVersionList.Where(x => x.IsLatestVersionLive == "NO").ToList();
                    case "1":
                        // Return language versions where total no of versions are greater than say 10 (configured via Setting)...
                        var maxVersionCount = CustomHelper.GetMaxVersionCount();
                        return objItemVersionList.Where(x => x.VersionCount > maxVersionCount).ToList();
                    default:
                        return objItemVersionList.ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error while fetching item language versions **********************", ex.Message);
            }
            return objItemVersionList;
        }

        //************************************************************************************************************************

        public static Item GetItemByPath(string itemPath, Database objDatabase)
        {
            return objDatabase.GetItem(itemPath);
        }

        //************************************************************************************************************************

        public static int GetItemLanguageCount(Item itemToProcess)
        {
            var languageCount = 0;
            try
            {
                languageCount = ItemManager.GetContentLanguages(itemToProcess).Select(lang => new {
                    lang.Name,
                    Versions = ItemManager.GetVersions(itemToProcess, lang).Count
                }).Where(t => t.Versions > 0).Select(t => t.Name).ToList().Count;
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error while fetching total languages for selected item **********************", ex.Message);
            }
            return languageCount;
        }

        //************************************************************************************************************************

        public static bool IsPageItem(Item itemToProcess)
        {
            try
            {
                return itemToProcess.Fields[Sitecore.FieldIDs.LayoutField] != null
                       && !String.IsNullOrEmpty(itemToProcess.Fields[Sitecore.FieldIDs.LayoutField].Value);
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error while checking if it's page or non-page item **********************", ex.Message);
            }
            return false;
        }

        //************************************************************************************************************************

        public static string GetContentEditorUrl(this Item item)
        {
            return string.Format("{0}/sitecore/shell/Applications/Content%20Editor.aspx?fo={1}",
                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), item.ID);
        }


        //************************************************************************************************************************
        //************************************************************************************************************************
    }
}