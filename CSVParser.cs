using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DialB
{
    public class CSVParser
    {
        private StreamReader sr;
        private string fileName;
        private Dictionary<string, int> headTitleMap = new Dictionary<string, int>();
        private string[] data;

        public int Row = 0;

        public CSVParser(string fileName)
        {
            this.fileName = fileName;
            Row = 0;
            headTitleMap.Clear();
        }

        #region file operation
        public void Open()
        {
            if (sr != null)
            {
                ErrorMessage.Exit("File " + fileName + " has been opened.");
            }
            else
            {
                try
                {
                    sr = new StreamReader(fileName);
                }
                catch (Exception ee)
                {
                    ErrorMessage.Exit("File: " + fileName + " does not exist.");
                }
            }

        }

        public void Close()
        {
            if (sr == null)
            {
                ErrorMessage.Exit("File " + fileName + " has been closed.");
            }
            else
            {
                sr.Close();
                sr.Dispose();
                sr = null;
                Row = 0;
            }
        }
        public bool IsEndOfStream()
        {
            return sr.EndOfStream;
        }
        #endregion

        #region read data
        public void ReadHeadTitle()
        {
            if (Row != 0)
            {
                ErrorMessage.Exit("Cannot read head title at row # " + Row.ToString() + " in file " + fileName + " .");
            }
            else
            {
                //read head
                string[] data = sr.ReadLine().Split(',');
                for (int i = 0; i < data.Count(); i++)
                {
                    headTitleMap.Add(data[i], i);
                }
                Row++;
            }
        }
        public void ReadDataByLine()
        {
            Row++;
            data = sr.ReadLine().Split(',');
        }
        #region convert data
        public void GetFieldValue(string name, out string value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string ss = data[headTitleMap[name]];
                value = ss;
            }
            else
            {
                value = "";
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out double value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string ss = data[headTitleMap[name]];
                if (ss == "")
                {
                    value = 0;
                }
                else
                {
                    bool isConvert = Double.TryParse(ss, out value);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss + ") to double value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = 0;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out float value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string ss = data[headTitleMap[name]];
                if (ss == "")
                {
                    value = 0;
                }
                else
                {
                    bool isConvert = Single.TryParse(ss, out value);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss + ") to float value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = 0;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out int value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string ss = data[headTitleMap[name]];
                if (ss == "")
                {
                    value = 0;
                }
                else
                {
                    bool isConvert = Int32.TryParse(ss, out value);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss + ") to int32 value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = 0;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out long value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string ss = data[headTitleMap[name]];
                if (ss == "")
                {
                    value = 0;
                }
                else
                {
                    bool isConvert = Int64.TryParse(ss, out value);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss + ") to int64 value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = 0;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out bool value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string ss = data[headTitleMap[name]];
                if (ss == "")
                {
                    value = false;
                }
                else
                {
                    bool isConvert = Boolean.TryParse(ss, out value);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss + ") to boolean value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = false;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }

        public void GetFieldValue(string name, out double[] value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string[] ss = data[headTitleMap[name]].Split(',');
                value = new double[ss.Count()];

                for (int i = 0; i < ss.Count(); i++)
                {
                    bool isConvert = Double.TryParse(ss[i], out value[i]);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss[i] + ") to double value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = null;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out float[] value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string[] ss = data[headTitleMap[name]].Split(',');
                value = new float[ss.Count()];

                for (int i = 0; i < ss.Count(); i++)
                {
                    bool isConvert = Single.TryParse(ss[i], out value[i]);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss[i] + ") to float value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = null;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out int[] value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string[] ss = data[headTitleMap[name]].Split(',');
                value = new int[ss.Count()];

                for (int i = 0; i < ss.Count(); i++)
                {
                    bool isConvert = Int32.TryParse(ss[i], out value[i]);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss[i] + ") to int32 value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = null;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        public void GetFieldValue(string name, out long[] value)
        {
            if (headTitleMap.ContainsKey(name))
            {
                string[] ss = data[headTitleMap[name]].Split(',');
                value = new long[ss.Count()];

                for (int i = 0; i < ss.Count(); i++)
                {
                    bool isConvert = Int64.TryParse(ss[i], out value[i]);
                    if (!isConvert)
                    {
                        ErrorMessage.Exit("Cannot convert string (" + ss[i] + ") to Int64 value. In file: " + fileName + " in row: " + Row.ToString());
                    }
                }
            }
            else
            {
                value = null;
                ErrorMessage.Exit("Cannot find field name: " + name + " in file: " + fileName + " .");
            }
        }
        #endregion
        #endregion

    }

    public static class ErrorMessage
    {
        public static void Exit(string errorMessage)
        {
            //Console.WriteLine(errorMessage);
            //Console.WriteLine("---------program will stop-----------");
            //Console.ReadKey();
            //Environment.Exit(0);
            throw new Exception(errorMessage);
        }

        public static void Warning(string warningMessage)
        {
            Console.WriteLine(warningMessage);
            //Console.WriteLine("---------program will stop-----------");
            //Console.ReadKey();
            //Environment.Exit(0);
            //throw new Exception(errorMessage);
        }
    }
}
