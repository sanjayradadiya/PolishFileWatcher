using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System.Threading;

namespace PolishFileWatcher
{
    public static class Utility
    {
        public static DataTable GetCSVData(string pPath)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(pPath))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        if (fieldData.Length <= colFields.Length)
                        {
                            for (int i = 0; i < fieldData.Length; i++)
                            {
                                if (fieldData[i] == "")
                                {
                                    fieldData[i] = null;
                                }
                            }
                            csvData.Rows.Add(fieldData);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return csvData;
        }

        public static DataTable AddDateColumn(this DataTable pDt,string pVal,string pColname)
        {
            DataTable csvData = pDt;
            csvData.Columns.Add(new DataColumn() { DataType = typeof(DateTime), AllowDBNull = false, ColumnName = pColname, DefaultValue = pVal });
            return csvData;
        }

        public static void PrintMessage(string pMessage, ConsoleColor pColor= ConsoleColor.White)
        {
            Console.ForegroundColor = pColor;
            Console.WriteLine(pMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void Loading()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Please Wait ");
            Random r = new Random(1);
            for (int i = 0; i < 5; i++)
            {
                Console.Write("*");
                Thread.Sleep(500);
            }
            ClearCurrentConsoleLine();
        }
    }
}