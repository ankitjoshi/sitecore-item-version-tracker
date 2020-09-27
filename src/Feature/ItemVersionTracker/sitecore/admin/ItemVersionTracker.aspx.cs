using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.ItemVersionTracker.Helpers;
using Sitecore.SharedSource.ItemVersionTracker.Models;

namespace Sitecore.SharedSource.ItemVersionTracker.sitecore.admin
{
    public partial class ItemVersionTracker : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack) return;
            BindData();
            ClearMessages();
        }

        //************************************************************************************************************************

        public void BindData()
        {
            lblToolVersion.Text = "Sitecore Item Version Tracker Version- " + Constants.Constant.ToolVersion;
            BindLanguagesDropdown();
        }

        //************************************************************************************************************************

        private void BindLanguagesDropdown()
        {
            var languageData = CustomHelper.GetLanguages();
            if (languageData == null || !languageData.Any()) return;
            ddlLanguage.Items.Add(new ListItem
            {
                Text = "--Select--",
                Value = "All"
            });
            foreach (var language in languageData.OrderBy(x => x.Name))
            {
                ddlLanguage.Items.Add(new ListItem
                {
                    Text = language.Name,
                    Value = language.Name
                });
            }
        }

        //************************************************************************************************************************

        private void ClearMessages()
        {
            lblContentError.Text = "";
            ltlReport.Text = "";
        }

        //************************************************************************************************************************

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            ClearMessages();

            #region control values

            var contentRoot = txtContentRootPath.Text.Trim();
            var includeChildItems = chkBoxIncludeChildItems.Checked;
            var contentType = ddlContentType.SelectedValue;
            var selectedFilter = ddlFilterBy.SelectedValue;
            var selectedLanguage = ddlLanguage.SelectedValue;

            #endregion

            if (!String.IsNullOrEmpty(contentRoot))
            {
                var masterDb = Database.GetDatabase(Sitecore.Configuration.Settings.GetSetting("Sitecore.SharedSource.ItemVersionTracker.SourceDatabase"));
                var isValid = CustomHelper.IsValidSource(contentRoot, masterDb);
                if (isValid)
                {
                    var objVersionData = GetVersionsData(contentRoot, includeChildItems, contentType, selectedFilter, selectedLanguage);
                    if (objVersionData != null && objVersionData.Any())
                    {
                        PopulateVersionReport(objVersionData);
                    }
                    else
                    {
                        ltlReport.Text = CustomHelper.GetErrorMessage(Constants.Messages.NoRecordsFound);
                    }
                }
                else
                {
                    lblContentError.Text = Constants.Messages.InvalidSource;
                }
            }
            else
            {
                lblContentError.Text = Constants.Messages.RootSourceRequired;
            }
        }

        //************************************************************************************************************************

        private List<VersionData> GetVersionsData(string contentRoot, bool includeChildItems, string contentType, string filterBy, string selectedLanguage)
        {
            try
            {
                Log.Info("*********************Sitecore Version Tracker: START-Fetching Item versions data **********************", this);

                var masterDb = Database.GetDatabase(Sitecore.Configuration.Settings.GetSetting("Sitecore.SharedSource.ItemVersionTracker.SourceDatabase"));
                var objVersionData = ExtractItemVersions(contentRoot, masterDb, includeChildItems, contentType, filterBy, selectedLanguage);
                if (objVersionData != null && objVersionData.Any())
                {
                    return objVersionData;
                }

                Log.Info("*********************Sitecore Version Tracker: END-Fetching Item versions data **********************", this);
                return objVersionData;
            }
            catch (Exception ex)
            {
                Log.Error("*********************Sitecore Version Tracker | Error while fetching versions data **********************", ex.Message);
                ltlReport.Text = CustomHelper.GetErrorMessage(Constants.Messages.ErrorOccured);
                return new List<VersionData>();
            }
        }

        //************************************************************************************************************************

        private List<VersionData> ExtractItemVersions(string contentRoot, Database objDatabase, bool includeChildItems, string contentType, string filterBy, string selectedLanguage)
        {
            try
            {
                Log.Info("*********************Sitecore Version Tracker: START-Extracting Item Versions **********************", this);

                var objVersionData = new List<VersionData>();
                var contentRootItem = SitecoreItemHelper.GetItemByPath(contentRoot, objDatabase);
                if (contentRootItem != null)
                {
                    if (includeChildItems)
                    {
                        // GetDescendants()- This can be optimized later (in next phase)...
                        // As next update we would be adding pagination also...
                        var allItems = contentRootItem.Axes.GetDescendants().ToList();
                        allItems.Add(contentRootItem);
                        foreach (var itemToProcess in allItems)
                        {
                            objVersionData.Add(new VersionData
                            {
                                ItemGuid = itemToProcess.ID.ToGuid(),
                                ItemPath = itemToProcess.Paths.FullPath,
                                ItemName = itemToProcess.Name,
                                TotalLanguages = SitecoreItemHelper.GetItemLanguageCount(itemToProcess),
                                IsPageItem = SitecoreItemHelper.IsPageItem(itemToProcess),
                                EditorUrl = itemToProcess.GetContentEditorUrl(),
                                LanguageDataList = FetchItemVersionData(itemToProcess, filterBy, selectedLanguage)
                            });
                        }
                    }
                    else
                    {
                        objVersionData.Add(new VersionData
                        {
                            ItemGuid = contentRootItem.ID.ToGuid(),
                            ItemPath = contentRootItem.Paths.FullPath,
                            ItemName = contentRootItem.Name,
                            TotalLanguages = SitecoreItemHelper.GetItemLanguageCount(contentRootItem),
                            IsPageItem = SitecoreItemHelper.IsPageItem(contentRootItem),
                            EditorUrl = contentRootItem.GetContentEditorUrl(),
                            LanguageDataList = FetchItemVersionData(contentRootItem, filterBy, selectedLanguage)
                        });
                    }

                    switch (contentType)
                    {
                        //filter by Page/data Item...
                        case "0":
                            return objVersionData.Where(x => x.LanguageDataList.Any() && x.IsPageItem).ToList();
                        case "1":
                            return objVersionData.Where(x => x.LanguageDataList.Any() && (!x.IsPageItem)).ToList();
                        default:
                            return objVersionData.Where(x => x.LanguageDataList.Any()).ToList();
                    }
                }

                Log.Info("*********************Sitecore Version Tracker: END-Extracting Item Versions **********************", this);
                ltlReport.Text = CustomHelper.GetErrorMessage(Constants.Messages.ErrorOccured);
                return new List<VersionData>();
            }
            catch (Exception ex)
            {
                Log.Error("*********************Sitecore Version Tracker | Error while extracting versions data **********************", ex.Message);
            }
            return new List<VersionData>();
        }

        //************************************************************************************************************************

        private List<LanguageData> FetchItemVersionData(Item itemToProcess, string filterBy, string selectedLanguage)
        {
            return SitecoreItemHelper.GetItemLanguages(itemToProcess, filterBy, selectedLanguage);
        }

        //************************************************************************************************************************

        private void PopulateVersionReport(List<VersionData> objVersionData)
        {
            try
            {
                Log.Info("*********************Sitecore Version Tracker: START-Binding Report **********************", this);

                StringBuilder versionReportString = new StringBuilder();

                versionReportString.AppendLine("<div class='table-format'>");
                versionReportString.AppendLine("<div class='table-title'>");
                versionReportString.AppendLine("Total Item(s):  " + objVersionData.Count + "");
                versionReportString.AppendLine("</div>");
                versionReportString.AppendLine("<div class='table-section'>");
                versionReportString.AppendLine("<table cellspacing='1'>");
                versionReportString.AppendLine("<tbody>");
                versionReportString.AppendLine("<tr>");
                versionReportString.AppendLine("<th>Item ID</th>");
                versionReportString.AppendLine("<th>Item Name</th>");
                versionReportString.AppendLine("<th title='Total language(s) available for this item'>Total Languages</th>");
                versionReportString.AppendLine("<th title='Name of the language'>Language Name</th>");
                versionReportString.AppendLine("<th title='Total available version(s) for this language'>Total Version(s)</th>");
                versionReportString.AppendLine("<th title='If latest version is published'>Latest Version Live</th>");
                versionReportString.AppendLine("<th title='Version no. which is published'>Live Version</th>");
                versionReportString.AppendLine("</tr>");

                foreach (VersionData itemRecord in objVersionData)
                {
                    versionReportString.AppendLine("<tr>");
                    versionReportString.AppendLine("<td style='width: 252px'>" + "<a href="+itemRecord.EditorUrl+ " target='_blank'>" + itemRecord.ItemGuid + "</a>"+ "</td>");
                    versionReportString.AppendLine("<td style='width: 160px' title='" + itemRecord.ItemPath + "'>" + itemRecord.ItemName + "</td>");
                    versionReportString.AppendLine("<td style='width: 94px'>" + itemRecord.TotalLanguages + "</td>");

                    versionReportString.AppendLine("<td colspan='4'>");
                    versionReportString.AppendLine("<table cellspacing='1' style='width:430px'>");

                    foreach (var languageRecord in itemRecord.LanguageDataList)
                    {
                        versionReportString.AppendLine("<tr>");
                        versionReportString.AppendLine("<td style='width:93px'>" + languageRecord.LanguageName + "</td>");
                        versionReportString.AppendLine("<td style='width:95px'>" + languageRecord.VersionCount + "</td>");
                        versionReportString.AppendLine("<td class='status-" + languageRecord.IsLatestVersionLive + "'>" + languageRecord.IsLatestVersionLive + "</td>");
                        versionReportString.AppendLine("<td>" + languageRecord.VersionNoLive + "</td>");
                        versionReportString.AppendLine("</tr>");
                    }

                    versionReportString.AppendLine("</table>");
                    versionReportString.AppendLine("</td>");
                    versionReportString.AppendLine("</tr>");
                }

                versionReportString.AppendLine("</tbody>");
                versionReportString.AppendLine("</table>");
                versionReportString.AppendLine("</div>");
                versionReportString.AppendLine("</div>");
                ltlReport.Text = versionReportString.ToString();

                Log.Info("*********************Sitecore Version Tracker: END-Binding Report**********************", this);
            }

            catch (Exception ex)
            {
                Log.Error("*********************Sitecore Version Tracker | Error while binding report **********************", ex.Message);
                ltlReport.Text = Constants.Messages.ErrorOccured;
            }
        }

        //************************************************************************************************************************

        protected void btnDownloadReport_Click(object sender, EventArgs e)
        {
            try
            {
                ClearMessages();

                var contentRoot = txtContentRootPath.Text.Trim();
                var includeChildItems = chkBoxIncludeChildItems.Checked;
                var contentType = ddlContentType.SelectedValue;
                var selectedFilter = ddlFilterBy.SelectedValue;
                var selectedLanguage = ddlLanguage.SelectedValue;

                if (!String.IsNullOrEmpty(contentRoot))
                {
                    var objVersionData = GetVersionsData(contentRoot, includeChildItems, contentType, selectedFilter, selectedLanguage);
                    if (objVersionData.Any())
                    {
                        DownloadReportDataToCsv(objVersionData);
                    }
                    else
                    {
                        ltlReport.Text = CustomHelper.GetErrorMessage(Constants.Messages.NoRecordsFound);
                    }
                }
                else
                {
                    lblContentError.Text = Constants.Messages.RootSourceRequired;
                }
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error while downloading report **********************", ex.Message);
                ltlReport.Text = CustomHelper.GetErrorMessage(Constants.Messages.ErrorOccured);
            }
        }

        //************************************************************************************************************************

        private void DownloadReportDataToCsv(List<VersionData> objVersionData)
        {
            try
            {
                string filename = string.Format("Item_Version_Tracker_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                StringBuilder content = new StringBuilder();
                string CSVHeaderRow = "Item ID,Item Path,Item Name,Total Languages,Language Name,Total Version(s),Latest version live,Live Version";
                content.AppendLine(CSVHeaderRow);
                foreach (VersionData itemRecord in objVersionData)
                {
                    var versionRecord = "";
                    if (itemRecord.LanguageDataList.Count > 1)
                    {
                        foreach (var languageRecord in itemRecord.LanguageDataList)
                        {
                            if (itemRecord.LanguageDataList.First() == languageRecord)
                            {
                                versionRecord = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                                    itemRecord.ItemGuid.ToString(),
                                    itemRecord.ItemPath,
                                    itemRecord.ItemName,
                                    itemRecord.TotalLanguages,
                                    languageRecord.LanguageName,
                                    languageRecord.VersionCount,
                                    languageRecord.IsLatestVersionLive,
                                    languageRecord.VersionNoLive);
                            }
                            else
                            {
                                versionRecord = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", "", "", "", "", languageRecord.LanguageName, languageRecord.VersionCount, languageRecord.IsLatestVersionLive, languageRecord.VersionNoLive);
                            }
                            content.AppendLine(versionRecord);
                        }
                    }
                    else
                    {
                        foreach (var languageRecord in itemRecord.LanguageDataList)
                        {
                            versionRecord = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                                itemRecord.ItemGuid.ToString(),
                                itemRecord.ItemPath,
                                itemRecord.ItemName,
                                itemRecord.TotalLanguages,
                                languageRecord.LanguageName,
                                languageRecord.VersionCount,
                                languageRecord.IsLatestVersionLive,
                                languageRecord.VersionNoLive);
                            content.AppendLine(versionRecord);
                        }
                    }
                }

                Response.Clear();
                Response.ContentType = "application/CSV";
                Response.ContentEncoding = Encoding.UTF8;
                Response.BinaryWrite(Encoding.UTF8.GetPreamble());
                Response.AddHeader("content-disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(filename, Encoding.UTF8) + "");
                Response.Write(content.ToString());
                Response.Flush();
                Response.End();
            }
            catch (Exception ex)
            {
                Log.Error("*********************Item Version Tracker | Error while downloading report **********************", ex.Message);
                ltlReport.Text = CustomHelper.GetErrorMessage(Constants.Messages.ErrorOccured);
            }
        }

        //************************************************************************************************************************
        //************************************************************************************************************************
    }
}