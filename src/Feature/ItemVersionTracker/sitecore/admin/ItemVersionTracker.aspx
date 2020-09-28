<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ItemVersionTracker.aspx.cs" Inherits="Sitecore.SharedSource.ItemVersionTracker.sitecore.admin.ItemVersionTracker" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sitecore Item Version Tracker</title>
    <style>
        body {
            font-family: "Trebuchet MS", Arial, Helvetica, sans-serif;
            font-size: 12px;
            margin: 0;
            padding: 0;
        }

        .page-title {
            font-size: 24px;
            text-align: center;
            padding: 20px 0;
            clear: both;
            background: gray;
            color: #fff;
            margin-bottom: 15px;
        }

        body form {
            max-width: 1000px;
            width: 100%;
            margin: 0 auto;
        }

        .table-format {
            border: 1px solid #000;
            padding: 20px 10px 10px 10px;
            position: relative;
        }

            .table-format .table-title {
                position: absolute;
                background: #fff;
                padding: 6px 10px;
                top: -16px;
                font-weight: bold;
                font-size: 16px;
                color: #000;
            }

            .table-format .table-section {
                max-height: 300px;
                min-width: 800px;
                overflow: auto;
            }

            .table-format table {
                width: 100%;
            }

            .table-format td, .table-format th {
                border-bottom: 1px solid #ddd;
                padding: 8px;
                font-size: 12px;
                font-family: "Trebuchet MS", Arial, Helvetica, sans-serif;
            }

            .table-format tr:nth-child(even) {
                background-color: #f2f2f2;
            }

            .table-format tr:hover {
                background-color: #ddd;
            }

            .table-format th {
                padding-top: 12px;
                padding-bottom: 12px;
                text-align: left;
                /*background-color: #4CAF50;*/
                background-color: #43464b;
                color: white;
            }

        .footer, .header {
            color: rgba(255,255,255,.6);
            background-color: #161c27;
            width: 100%;
            float: left;
            box-sizing: border-box;
        }

        .header {
            padding: 15px;
        }

        .footer-left {
            display: inline-block;
            width: 49%;
            padding: 10px 20px;
            float: left;
            box-sizing: border-box;
        }

        .footer-right {
            display: inline-block;
            width: 49%;
            float: right;
            text-align: right;
        }

            .footer-right ul {
                margin: 0;
                padding: 10px 20px;
            }

                .footer-right ul li {
                    display: inline-block;
                    list-style: none;
                    padding: 0 5px;
                }

                    .footer-right ul li a {
                        color: rgba(255,255,255,.6);
                        text-decoration: none;
                    }

        .form-section div {
            width: 100%;
            display: inline-block;
            padding: 10px 0;
        }

            .form-section div .error-message {
                padding: 0;
                color: red;
            }

                .form-section div .error-message span {
                    display: block !important;
                    width: 100% !important;
                    padding-left: 0 !important;
                }

        .not-found-error {
            color: red;
            padding: 0 20px;
            font-size: 14px;
            line-height: 24px;
        }

        .form-section div:nth-child(even) {
            background-color: #f2f2f2;
        }

        input[type=text], select, textarea {
            width: 25%;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
            box-sizing: border-box;
            resize: vertical;
        }

        input[type="submit"] {
            background-color: #4CAF50;
            color: white;
            padding: 12px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            float: right;
            margin-bottom: 20px;
        }

        #btnClear {
            float: left;
            background: gray;
        }

        .form-section div span {
            width: 24%;
            display: inline-block;
            vertical-align: middle;
            padding-left: 10px;
            box-sizing: border-box;
        }

        .loader {
            background: rgba(0,0,0,.4);
            position: fixed;
            width: 100%;
            height: 100%;
            z-index: 10;
            left: 0;
            top: 0;
            display: none;
        }

            .loader img {
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
            }

        .fa-info-circle {
            font-size: 40px;
            color: red;
            vertical-align: middle;
            cursor: pointer;
        }

        .status-YES {
            color: green;
            font-weight: bold;
        }

        .status-NO {
            color: red;
            font-weight: bold;
        }

        th {
            position: -webkit-sticky;
            position: sticky;
            top: 0;
            z-index: 2;
        }

        input[type="submit"].download-report {
            background: #D2691E;
            float: left;
        }

        #lblToolVersion {
            padding: 10px;
            display: block;
        }

        .notes {
            margin-bottom: 20px;
            padding-left: 10px;
            background: #e4f9e5 !important;
            font-size: 14px;
        }

            .notes p {
                font-weight: bold;
                padding-left: 10px;
            }

        .form-section div.report {
            padding: 10px 20px;
            box-sizing: border-box;
        }

            .form-section div.report input[type="submit"] {
                margin: 0;
            }

        .form-section div.filter-section span {
            width: auto;
            padding-top: 10px;
        }

        .form-section div.filter-section input[type=text], .form-section div.filter-section select, .form-section div.filter-section textarea {
            width: 20%;
            margin-right: 25px;
        }

        .form-section div input[type=text] {
            margin-right: 25px;
        }

        .form-section div span {
            width: auto;
            text-align: left;
            padding-top: 8px;
        }
		.form-section span div {
			display: block;
		}

    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="scrptManagerVersionTracker" runat="server"></asp:ScriptManager>
        <div class="page-title">
            Sitecore Item Version Tracker
        </div>
        <div class="form-section">
            <div>
                <span id="lblContentRootPath">Enter root path(page/data):<div class="error-message">
                    <asp:Label ID="lblContentError" runat="server"></asp:Label>
                </div>
                </span>
                <asp:TextBox ID="txtContentRootPath" runat="server" ToolTip="Enter content root path or ID to search"></asp:TextBox>

                <span id="lblIncludeChildItems">Include Child Items:<div class="error-message">
                    <asp:Label ID="lblIncludeChildItemsError" runat="server"></asp:Label>
                </div>
                </span>
                <asp:CheckBox ID="chkBoxIncludeChildItems" runat="server"></asp:CheckBox>
            </div>
            <div class="filter-section">
                <span id="lblContentTyperBy">Content Type:<div class="error-message">
                    <asp:Label ID="lblContentTyperError" runat="server"></asp:Label>
                </div>
                </span>
                <asp:DropDownList runat="server" ID="ddlContentType" ToolTip="Select the content type (Page/Non Page Item)">
                    <asp:ListItem>--Select--</asp:ListItem>
                    <asp:ListItem Value="0" Text="Only Page item(s)"></asp:ListItem>
                    <asp:ListItem Value="1" Text="Non Page item(s)"></asp:ListItem>
                </asp:DropDownList>

                <span id="lblLanguage">Language:<div class="error-message">
                    <asp:Label ID="Label2" runat="server"></asp:Label>
                </div>
                </span>
                <asp:DropDownList runat="server" ID="ddlLanguage" ToolTip="Select the language">
                </asp:DropDownList>

                <span id="lblFilterBy">Filter By:<div class="error-message">
                    <asp:Label ID="Label1" runat="server"></asp:Label>
                </div>
                </span>
                <asp:DropDownList runat="server" ID="ddlFilterBy" ToolTip="Select the filter">
                    <asp:ListItem>--Select--</asp:ListItem>
                    <asp:ListItem Value="0" Text="Latest version is not published"></asp:ListItem>
                    <asp:ListItem Value="1" Text="Max Versions"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="report">
                <asp:Button ID="btnDownloadReport" CssClass="download-report" runat="server" Text="Download Report (CSV)" OnClick="btnDownloadReport_Click" ToolTip="Download the report in CSV format" />
                <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report" OnClick="btnGenerateReport_Click" ToolTip="Generate the Report" />
            </div>

            <div class="notes">
                <p>Note(s):</p>
                <ul>
                    <li>Mouse hover on the item name to get item path.</li>
                    <li>Language version is not published to target database if "Live Version" is set to zero (0).</li>
                    <li>"Max Versions" would filter all language versions where more than x(Recommendation is to have max 10 numbered versions) no of versions has been created, and this (x) can be controlled via setting in config file.</li>
                    <li>Click on Item ID and it would redirect you to selected item in Content editor.</li>
                </ul>
            </div>

        </div>
        <asp:Literal ID="ltlReport" runat="server" />
        <br />
        <br />

        <div class="footer">
            <div class="footer-left">
                <br />
                <div id="currentDate"></div>
            </div>
            <div class="footer-right">
                <div>
                    <asp:Label ID="lblToolVersion" Font-Bold="True" ForeColor="white" runat="server"></asp:Label>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
