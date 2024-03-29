﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TFABot
{
    public class clsSpreadsheetReader
    {
        public string URL { get; private set; }

        public clsSpreadsheetReader(string url)
        {
            var reg = new Regex(@"^https.*[^\/]{40,}");
            var match = reg.Match(url);
            URL = match.Value;
        }

        //Generic sheet reader.
        public List<T> ReadSheet<T>(string SheetName, StringBuilder sb) where T : class
        {
            //Create a new list
            List<T> list = new List<T>();

            //Get Spreadsheet data
            var data = GetSpreadsheetData(SheetName);

            //Get the custom spreadsheet attributes in our target class.
            var type = typeof(T);
            var properties = type.GetProperties().Where(x => x.GetCustomAttribute(typeof(ASheetColumnHeader), false) != null).ToList();

            //Match the spreadsheet headers, with our target class type
            var columns = new Dictionary<int, PropertyInfo>();
            var indexColumn = -1;
            //Read the header line (Start from line 0, until we find the index)
            int r = 0;
            for (; r < data.Length; r++)
            {
                for (int c = 0; c < data[r].Length; c++)
                {
                    var property = properties.FirstOrDefault(x => x.GetCustomAttribute<ASheetColumnHeader>().IsMatch(data[r][c]));
                    if (property != null)
                    {
                        columns.Add(c, property);
                        if (property.GetCustomAttribute<ASheetColumnHeader>().IsIndex) indexColumn = c;
                    }
                }
                if (indexColumn != -1) break;  //We got our index, so move to read the userdata
            }

            int firstrow = ++r;
            //Read the data, and populate new target classes.
            for (; r < data.Length; r++)
            {
                if (data[r].Contains("<END>")) break;  //End of userdata.

                T dataClass = Activator.CreateInstance<T>();
                list.Add(dataClass);

                for (int c = 0; c < data[r].Length; c++)
                {
                    PropertyInfo pi;
                    if (columns.TryGetValue(c, out pi))
                    {
                        if (columns[c].PropertyType == typeof(TimeSpan))
                        {
                            TimeSpan timeSpan;
                            if (TimeSpan.TryParse(data[r][c].Trim(), out timeSpan))
                                columns[c].SetValue(dataClass, timeSpan);
                            else
                                sb.Append($"Error: Cannont read Time {Convert.ToChar(65 + c)}{firstrow + r}\n");
                        }
                        else if (columns[c].PropertyType == typeof(string[]))
                        {
                            var k = c;
                            List<string> stringlist = new List<string>();
                            for (; c < data[r].Length; c++)
                            {
                                if (c > properties.Count)
                                {
                                    sb.Append("Error: Too many columns. Please check data does not have commas.\n");
                                    break;
                                }
                                if (!string.IsNullOrEmpty(data[r][c])) stringlist.AddRange(data[r][c].Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries));
                            }
                            columns[k].SetValue(dataClass, stringlist.ToArray());
                        }
                        else
                        {
                            try
                            {
                                var ob = System.Convert.ChangeType(data[r][c].Trim(), columns[c].PropertyType);
                                columns[c].SetValue(dataClass, ob);
                            }
                            catch (Exception ex)
                            {
                                sb.Append($"Error: Cannont read {Convert.ToChar(65 + c)}{firstrow + r} {ex.Message}\n");
                            }
                        }
                    }
                }
                sb.Append(((ISpreadsheet<T>)dataClass).PostPopulate() ?? "");
            }

            return list;
        }


        String[][] GetSpreadsheetData(string SheetName)
        {
            var client = new RestClient(URL);
            var request = new RestRequest("gviz/tq");

            request.AddParameter("tqx", "out:csv");
            request.AddParameter("sheet", SheetName);
            request.AddParameter("headers", "true");
            //request.AddParameter("tq","select A,B,C,D,E,F,G,H");

            client.Timeout = 120000;

            Console.WriteLine($"Reading {SheetName}");
            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                string[][] stdata = new string[0][];

                var lines = response.Content.Split(new[] { '\n' });
                if (lines[0].Contains("invalid_query")) throw new Exception("Invalid query");

                for (int f = 0; f < lines.Length; f++)
                {
                    int e; //Strip away last empty fields.
                    for (e = lines[f].Length; e > 0; e -= 3) if (!lines[f].Substring(e - 3, 3).Contains(",\"\"")) break;

                    var columns = lines[f].Substring(0, e).Trim('\"').Split(new string[] { "\",\"", "," }, StringSplitOptions.None);
                    if (f == 0) stdata = new string[lines.Length][];
                    stdata[f] = columns;
                }

                return stdata;
            }
            else
            {
                throw new Exception($"Get spreadsheet data failed {response.ErrorMessage}");
            }

        }
    }
}
