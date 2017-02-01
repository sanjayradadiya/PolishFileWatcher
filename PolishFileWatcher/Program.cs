using System;
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
            string vCurdate = DateTime.Now.ToString("yyyy-MM-dd");

            string vWatchDirName = string.Join(" ", args);
            Utility.PrintMessage("Start Watching " + vWatchDirName, ConsoleColor.Green);

            DirectoryInfo di = new DirectoryInfo(vWatchDirName);
            Utility.PrintMessage("Last Write Time : " + di.LastWriteTime , ConsoleColor.Green);
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
                        // create mappings for bulkinsert in which csv column fields and target table name fileds should be map.
                        Dictionary<string, string> vMappings = new Dictionary<string, string>();
                        vMappings.Add("FileDate", "Date");
                        vMappings.Add("Name", "PersonName");
                        vMappings.Add("Description", "Detail");
                        vMappings.Add("Image URL", "Photo");

                        DataTable dt = Utility.GetCSVData(vFilePath).AddDateColumn(vDate, "FileDate");

                        Utility.PrintMessage("Total Record Found " + dt.Rows.Count);

                        //execute bulk insert so insert statement is doesn't requred,Console.ForegroundColor its superfast technique to insert data.
                        dh.InsertBulkData("PolishRecord", vMappings, dt);
                    }
                    else if (vRecordType.ToLower().Equals("policeten7"))
                    {
                        
                    }
                }

            }
        }
    }
}
