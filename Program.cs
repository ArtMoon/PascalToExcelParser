using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace PasParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new PascalParser(args[0]);
            parser.Parse();
        }


        private class PascalParser
        {
            private string _filePath = string.Empty;
            public PascalParser(string path)
            {
                _filePath = path;
            }

            public void Parse()
            {
                var checker = new TextChecker();
                try
                {
                    using(var stream = new StreamReader(_filePath))
                    {
                        string line = string.Empty;
                        while(!(line = stream.ReadLine()).Contains("implementation"))
                        {
                            var matches = checker.CheckText(KeyWordType.CLASS,line);
                            if(matches.Count ==0) matches = checker.CheckText(KeyWordType.FIELD,line);
                            if(matches.Count ==0) matches = checker.CheckText(KeyWordType.PROCEDURE,line);
                            if(matches.Count ==0) matches = checker.CheckText(KeyWordType.FUNCTION,line);
                            if(matches.Count ==0) continue;
                            foreach(Match val in matches)
                            {
                                var str = SplitWords(val.Value);
                                ExcelExporter.AppendLine(str);
                            }

                        }
                    }
                }
                catch(Exception e)
                {
                    throw e;
                }
            }

            private string[] SplitWords(string val)
            {
                char[] punktMarks = {'.',',','=',':','(',')','-',';'};
                foreach(var mark in punktMarks)
                {
                    val = val.Replace(mark.ToString()," ");
                }
                return val.Split(' ');
            } 
        }

        private static class ExcelExporter
        {
            private static int _currentLine = 0;

            public static void AppendLine(params string[] values)
            {
                _currentLine++;
                var fileInfo = new FileInfo("Unit.xlsx");

                using(var package = new ExcelPackage(fileInfo))
                {
                    int i = 1;
                    ExcelWorksheet worksheet = null;
                    if(package.Workbook.Worksheets.Count == 0)
                    {
                        worksheet = package.Workbook.Worksheets.Add("New");
                    }
                    else
                    {                 
                        worksheet = package.Workbook.Worksheets["New"];                 
                    }
                    
       
                     
                    foreach(var val in values)
                    {
                        worksheet.Cells[_currentLine,i].Value = val;       
                        i++;
                    }
                   
                    package.Save();
                }
            }
        }

        private class TextChecker
        {
            private Dictionary<KeyWordType,string> _keyWords = new Dictionary<KeyWordType, string>
            {
                {KeyWordType.UNIT,@"^unit\s*\w*;"},
                {KeyWordType.CLASS,@"^\w*\s*=\s*class\w*"},
                {KeyWordType.PROCEDURE,@"^procedure\s*\D*;"},
                {KeyWordType.FUNCTION,@"^function\s*\w*\s*:\s*\w*;"},
                {KeyWordType.FIELD,@"^\w*\s*:\s*\w*;"}
            };

            public MatchCollection CheckText(KeyWordType wordType,string text)
            {
                var reg = string.Empty;
                text = text.ToLower().Trim();
                _keyWords.TryGetValue(wordType,out reg);

                if(reg != null)
                {
                    var regex = new Regex(reg);
                    var matches = regex.Matches(text);
                    return matches;
                }
                else
                {
                    return null;
                }
            }
        }

        private enum KeyWordType
        {
            USING,
            UNIT,
            CLASS,
            PROCEDURE,
            FUNCTION,
            FIELD
        }
    }
}
