using System;
using Sitecore.Data;
using System.Text;
using Sitecore.Collections;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.SharedSource.ItemVersionTracker.Helpers
{
    public static class CustomHelper
    {
        //************************************************************************************************************************

        public static bool IsValidSource(string path,Database objDatabase)
        {
            var isValid = true;
            try
            {
                if (!Sitecore.Data.ID.IsID(path) && !IsValidPath(path, objDatabase))
                {
                    isValid = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error in IsValidSource **********************", ex.Message);
            }
            return isValid;
        }

        //************************************************************************************************************************

        public static bool IsValidPath(string path, Database objDatabase)
        {
            var isValid = false;
            try
            {
                var validItem = objDatabase.GetItem(path);
                if (validItem != null)
                {
                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error in IsValidPath **********************", ex.Message);
            }
            return isValid;
        }

        //************************************************************************************************************************

        public static string GetErrorMessage(string message)
        {
            StringBuilder htmlTableErrorMessageString = new StringBuilder();
            htmlTableErrorMessageString.AppendLine("<div class='not-found-error'>");
            htmlTableErrorMessageString.AppendLine("<span>" + message + "</span>");
            htmlTableErrorMessageString.AppendLine("</div>");
            return htmlTableErrorMessageString.ToString();
        }

        //************************************************************************************************************************

        public static int GetMaxVersionCount()
        {
            var maxVersionCount = 10; //set the default count to 10 if no value set...
            try
            {
                if (!String.IsNullOrEmpty(Sitecore.Configuration.Settings.GetSetting("Sitecore.SharedSource.ItemVersionTracker.MaxVersionCount")))
                {
                    maxVersionCount = System.Convert.ToInt32(Sitecore.Configuration.Settings.GetSetting("Sitecore.SharedSource.ItemVersionTracker.MaxVersionCount"));
                }
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error while fetching max versions  **********************", ex.Message);
            }
            return maxVersionCount;
        }

        //************************************************************************************************************************

        public static LanguageCollection GetLanguages()
        {
            var masterDb = Database.GetDatabase(Sitecore.Configuration.Settings.GetSetting("Sitecore.SharedSource.ItemVersionTracker.SourceDatabase"));
            return LanguageManager.GetLanguages(masterDb);
        }

        //************************************************************************************************************************
        //************************************************************************************************************************
    }
}