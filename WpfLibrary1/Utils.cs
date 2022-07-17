using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WpfLibrary1
{
    public class Utils
    {
        public static bool IsBinary(string filePath, int requiredConsecutiveNul = 1)
        {
            const int charsToCheck = 8000;
            const char nulChar = '\0';

            int nulCount = 0;

            using (var streamReader = new System.IO.StreamReader(filePath))
            {
                for (var i = 0; i < charsToCheck; i++)
                {
                    if (streamReader.EndOfStream)
                        return false;

                    if ((char)streamReader.Read() == nulChar)
                    {
                        nulCount++;

                        if (nulCount >= requiredConsecutiveNul)
                            return true;
                    }
                    else
                    {
                        nulCount = 0;
                    }
                }
            }
            return false;
        }
        public static string ListToString(List<string> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var item in list)
            {
                stringBuilder.Append(item);
                stringBuilder.Append('\n');
            }
            return stringBuilder.ToString();
        }
        public static string ListToString(List<FileInfo> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in list)
            {
                stringBuilder.Append(item.FullName);
                stringBuilder.Append('\n');
            }
            return stringBuilder.ToString();
        }
    }
}
