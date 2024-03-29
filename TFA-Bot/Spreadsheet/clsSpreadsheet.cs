﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TFABot
{
    public class clsSpreadsheet
    {
        clsSpreadsheetReader sheet;

        public clsSpreadsheet(string sheeturl)
        {
            sheet = new clsSpreadsheetReader(sheeturl);
        }

        public string LoadSettings()
        {
            var sb = new StringBuilder();

            try
            {
                ReadSheetSettings("Settings", sb);
                ReadSheet<clsNotificationPolicy>("NotificationPolicy", Program.NotificationPolicyList, sb);
                ReadSheet<clsNetwork>("Networks", Program.NetworkList, sb);
                ReadSheet<DiscordUser>("Users", Program.UserList, sb);
                ReadSheet<clsNodeGroup>("NodeGroups", Program.NodeGroupList, sb);

                ReadSheet<clsNode>("Nodes", Program.NodesList, sb);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error loading Settings {ex.Message}");
            }
            return sb.ToString();
        }

        public void ReadSheetSettings(string SheetName, StringBuilder sb)
        {

            try
            {
                sb.Append($"Loading {SheetName}.....  ");
                var userSS = sheet.ReadSheet<clsSetting>(SheetName, sb);

                foreach (var setting in userSS)
                {
                    if (Program.SettingsList.ContainsKey(setting.Key))
                    {
                        Program.SettingsList[setting.Key] = setting.Value;
                    }
                    else
                    {
                        Program.SettingsList.Add(setting.Key, setting.Value);
                    }
                }
                sb.AppendLine($"{Program.SettingsList.Count} items, OK");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error: {ex.Message}");
                Console.WriteLine(ex.Message);
            }
        }

        public void ReadSheet<T>(string SheetName, Dictionary<string, T> MainList, StringBuilder builder) where T : class
        {
            try
            {
                builder.Append($"Loading {SheetName}.....  ");
                var userSS = sheet.ReadSheet<T>(SheetName, builder);
                Merge(MainList, userSS);
                builder.AppendLine($"{MainList.Count} items, OK");
            }
            catch (Exception ex)
            {
                builder.AppendLine($"Error: {ex.Message}");
                Console.WriteLine(ex.Message);
            }
        }


        //Merge Spreadsheet data 
        public void Merge<T>(Dictionary<string, T> mainList, List<T> SList) where T : class
        {

            //Use reflection to get index field
            var type = typeof(T);
            var indexProperty = type.GetProperties().Where(x => x.GetCustomAttribute<ASheetColumnHeader>() != null && x.GetCustomAttribute<ASheetColumnHeader>().IsIndex).ElementAt(0);

            //Copy the main listm and sort
            var mainCopyList = mainList.Values.OrderBy(x => indexProperty.GetValue(x, null)).ToList();
            //Sort Spreadsheet list on it's index property
            SList.Sort((x, y) => ((string)indexProperty.GetValue(x, null)).CompareTo(indexProperty.GetValue(y, null)));


            int u = 0;
            int s = 0;
            while (u < mainCopyList.Count || s < SList.Count)
            {
                //as both our lists are sorted, we can match them, to see if there are new/missing/matching entries.
                int match;
                if (u >= mainCopyList.Count)  //Nothing to match, so we have a new entry
                {
                    match = 1;
                }
                else if (s >= SList.Count)  //Exceeded existing list, so we delete
                {
                    match = -1;
                }
                else
                {
                    match = ((string)indexProperty.GetValue(mainCopyList[u], null)).CompareTo(((string)indexProperty.GetValue(SList[s], null)));
                }



                switch (match)
                {
                    case 0:    //match
                        ((ISpreadsheet<T>)mainCopyList[u]).Update(SList[s]);
                        u++;
                        s++;
                        break;

                    case 1:  //new
                        mainList.Add(((string)indexProperty.GetValue(SList[s], null)), SList[s]);  //new
                        s++;
                        break;

                    case -1:  //delete
                        mainList.Remove(((string)indexProperty.GetValue(mainCopyList[u], null)));
                        u++;
                        break;
                }
            }
        }
    }
}
