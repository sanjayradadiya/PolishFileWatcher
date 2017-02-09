﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolishFileWatcher
{
    class Program
    {
        static SqlDataHelper dh = new SqlDataHelper();
        static void Main(string[] args)
        {
            //string vCurdate = DateTime.Now.ToString("yyyy-MM-dd");
            string vCurdate = "2017-01-20";
            string vWatchDirName = string.Join(" ", args);
            Utility.PrintMessage("Start Watching " + vWatchDirName, ConsoleColor.Green);

            DirectoryInfo di = new DirectoryInfo(vWatchDirName);
            Utility.PrintMessage("Last Write Time : " + di.LastWriteTime, ConsoleColor.Green);
            Utility.PrintMessage("Last Access Time : " + di.LastAccessTime, ConsoleColor.Green);
            Utility.PrintMessage("Total Folders : " + di.GetDirectories().Length, ConsoleColor.Green);
            Utility.PrintMessage("Total Files : " + di.GetFiles().Length, ConsoleColor.Green);

            Console.WriteLine();

            //get the information regarding current date folder
            var folderlist = from i in Directory.GetDirectories(vWatchDirName)
                             where i.Split('\\').LastOrDefault().Split('_').Length > 2 &&
                             i.Split('\\').LastOrDefault().Split('_').FirstOrDefault().Equals(vCurdate)
                             select new
                             {
                                 FullDirPath = i,
                                 DirName = i.Split('\\').LastOrDefault()
                             };

            // check for current data has any information or not.
            if (!folderlist.Any()) { Utility.PrintMessage("No any information found for date " + vCurdate, ConsoleColor.Red); return; }

            // scan all the folder which is belongs to current date
            foreach (var item in folderlist)
            {
                Console.WriteLine();
                Utility.PrintMessage("Start Scanning " + item.DirName, ConsoleColor.Yellow);

                Utility.Loading();

                string vDirName = item.DirName;
                string[] vSplitDirName = vDirName.Split('_');
                if (vSplitDirName.Length > 2)
                {
                    string vDate = vSplitDirName.FirstOrDefault();
                    string vNumber = vSplitDirName.Skip(1).FirstOrDefault();
                    string vRecordType = string.Join("_", vSplitDirName.Skip(2));

                    string vFilename = (vRecordType.ToLower().Equals("wanted_to_arrest") ? "wanted-to-arrest1" : "wanted-to-arrest2") + "_" + vDate + "_" + vNumber + ".csv"; // generate file name from folder name and fetch from the folders file list and scan it.

                    string vFilePath = item.FullDirPath + @"\" + vFilename;
                    if (!File.Exists(vFilePath)) { Utility.PrintMessage("No such file found for import..!!!", ConsoleColor.Red); continue; }

                    if (vRecordType.ToLower().Equals("wanted_to_arrest"))
                    {

                        DataTable dt = Utility.GetCSVData(vFilePath).AddDateColumn(vDate, "FileDate");

                        var filterdata = from ndata in (from data in dt.AsEnumerable()
                                                        select new
                                                        {
                                                            Date = Convert.ToDateTime(data["FileDate"]),
                                                            Name = data.Field<string>("Name").Split(' '),
                                                            TextField = data.Field<string>("Reason"),
                                                            Area = data.Field<string>("Wanted by police in"),
                                                            LikelyArea = data.Field<string>("Likely whereabouts"),
                                                            Age = data.Field<string>("Age").Split(' '),
                                                            Description = data.Field<string>("Description"),
                                                            Link_To_Photo = data.Field<string>("Image URL"),
                                                        })
                                         select new
                                         {
                                             Date = ndata.Date,
                                             FirstName = ndata.Name[0],
                                             MiddleName = ndata.Name.Length == 3 ? ndata.Name[1] : "",
                                             LastName = ndata.Name.Length == 3 ? ndata.Name[2] : ndata.Name[1],
                                             TextField = ndata.TextField,
                                             Area = ndata.Area,
                                             LikelyArea = ndata.LikelyArea,
                                             Age = Convert.ToInt32(ndata.Age[0]),
                                             Description = ndata.Description,
                                             Link_To_Photo = ndata.Link_To_Photo
                                         };

                        DataTable dtnew = new DataTable();

                        dtnew.Columns.Add(new DataColumn() { ColumnName = "Date", DataType = typeof(DateTime) , AllowDBNull=true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "FirstName", DataType = typeof(string), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "MiddleName", DataType = typeof(string), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "LastName", DataType = typeof(string), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "TextField", DataType = typeof(string), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "Area", DataType = typeof(string), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "LikelyArea", DataType = typeof(string), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "Age", DataType = typeof(int), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "Description", DataType = typeof(string), AllowDBNull = true });
                        dtnew.Columns.Add(new DataColumn() { ColumnName = "Link_To_Photo", DataType = typeof(string), AllowDBNull = true });

                        foreach (var itemdata in filterdata)
                        {
                            dtnew.Rows.Add(itemdata.Date,itemdata.FirstName,itemdata.MiddleName,itemdata.LastName,itemdata.TextField,itemdata.Area,itemdata.LikelyArea,itemdata.Age,itemdata.Description,itemdata.Link_To_Photo);
                        }

                        Utility.PrintMessage("Total Record Found " + dt.Rows.Count);

                        // create mappings for bulkinsert in which csv column fields and target table name fileds should be map.
                        Dictionary<string, string> vMappings = new Dictionary<string, string>()
                        {
                            { "Date", "Date" },
                            { "FirstName", "FirstName"},
                            { "MiddleName", "MiddleName"},
                            { "LastName", "LastName"},
                            { "TextField", "TextField"},
                            { "Area", "Area"},
                            { "LikelyArea", "LikelyArea"},
                            { "Age", "Age"},
                            { "Description", "Description"},
                            { "Link_To_Photo", "Link_To_Photo"},
                        };

                        //execute bulk insert so insert statement is doesn't requred,Console.ForegroundColor its superfast technique to insert data.
                        dh.InsertBulkData("PolishRecord", vMappings, dtnew);
                    }
                    else if (vRecordType.ToLower().Equals("policeten7"))
                    {
                        Dictionary<string, string> vMappings = new Dictionary<string, string>();
                        vMappings.Add("FileDate", "FileDate");
                        vMappings.Add("Title", "Title");
                        vMappings.Add("Episode", "Episode");
                        vMappings.Add("Date", "Date");
                        vMappings.Add("Description", "Description");
                        vMappings.Add("Image 1", "Image");

                        DataTable dt = Utility.GetCSVData(vFilePath).AddDateColumn(vDate, "FileDate");

                        // filter only those data which have record is marked as wanted 
                        var filterdata = from data in dt.AsEnumerable()
                                         where data.Field<string>("Title").Contains("Wanted")
                                         select data;

                        DataTable newdt = filterdata.CopyToDataTable();

                        Utility.PrintMessage("Total (Wanted) Record Found " + newdt.Rows.Count);

                        dh.InsertBulkData("PolishEvent", vMappings, newdt);
                    }
                }

            }

            Console.WriteLine();
        }
    }
}
