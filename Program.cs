using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Style;
using System.Drawing;

namespace PasParser
{
    class Program
    {
        static void Main(string[] args)
        {

            //Path to file should be in args
            if(args.Length != 0)
            {
                var parser = new PascalParser(args[0]);
                parser.Parse();
            }

        }


        private class PascalParser
        {
            private string _filePath = string.Empty;
            private KeyWordType _currentKeyWord = KeyWordType.NULL;
            private KeyWordType _currentAccessType = KeyWordType.PRIVATE;
            public PascalParser(string path)
            {
                _filePath = path;
            }

            public void Parse()
            {
              
                try
                {
                    using(var stream = new StreamReader(_filePath))
                    {
                        string line = string.Empty;
                        while(!(line = stream.ReadLine()).Contains("implementation"))
                        {
                            if(ParseCodeLine(KeyWordType.CLASS,line,ClassDocumentation)) continue;
                            if(ParseCodeLine(KeyWordType.FIELD,line,FieldDocumentation)) continue;
                            if(ParseCodeLine(KeyWordType.PROCEDURE,line,ProcedureDocumentation)) continue;
                            if(ParseCodeLine(KeyWordType.FUNCTION,line,ProcedureDocumentation)) continue;
                            if(ParseCodeLine(KeyWordType.PUBLIC,line,(x)=>{_currentAccessType = KeyWordType.PUBLIC;} )) continue;
                            if(ParseCodeLine(KeyWordType.PRIVATE,line,(x)=>{_currentAccessType = KeyWordType.PRIVATE;} )) continue;
                            if(ParseCodeLine(KeyWordType.PROTECTED,line,(x)=>{_currentAccessType = KeyWordType.PROTECTED;} )) continue;
                            
                        }
                    }
                }
                catch(Exception e)
                {
                    throw e;
                }
            }

            private bool ParseCodeLine(KeyWordType wordType,string line,Action<string> documentationAction)
            {
                var checker = new TextChecker();
                var matches = checker.CheckText(wordType,line);   
                if(matches.Count > 0)
                {              
                    foreach(Match val in matches)
                    {                            
                        documentationAction(val.Value);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private void ProcedureDocumentation(string val)
            {
                if(_currentKeyWord != KeyWordType.PROCEDURE)
                {
                    string[] headers = {"Тип","Название","Аргументы","Возвращаемое значение","Видимость","Описание"};
                    ExcelExporter.AppendLine(Color.GreenYellow,headers);
                    _currentKeyWord = KeyWordType.PROCEDURE;
                }
                val = val.Replace(";",string.Empty);
                var docParams = new List<string>(); 
                var strings = val.Split('(');
                if(strings.Length > 1)
                {
                    var typeAndName = strings[0].Split(' ');  
                    docParams.Add(typeAndName[0]);
                    docParams.Add(typeAndName[1]);  

                    var args = strings[1].Split(')'); 
                    docParams.Add(args[0]);   

                    if(args.Length > 1) 
                    {
                        var res = args[1].Split(':');
                        if(res.Length > 1)
                        {
                            docParams.Add(res[1]);
                        }
                        else
                        {
                            docParams.Add("-");
                        }
                    }
                }
                else
                {
                    var res = strings[0].Split(':');
                    var typeAndName = res[0].Split(' '); 
                    docParams.Add(typeAndName[0]);
                    docParams.Add(typeAndName[1]);  
                    docParams.Add("-");
                    if(res.Length > 1)
                    {  
                        docParams.Add(res[1]);
                    }
                    else
                    {
                        docParams.Add("-");
                    }
                   
                    
                }
                docParams.Add(_currentAccessType.ToString().ToLower());
                ExcelExporter.AppendLine(Color.White,docParams.ToArray());

            }

            private void FieldDocumentation(string val)
            {
                if(_currentKeyWord != KeyWordType.FIELD)
                {
                    string[] headers = {"Название","Тип","Видимость","Описание"};
                    ExcelExporter.AppendLine(Color.GreenYellow,headers);
                    _currentKeyWord = KeyWordType.FIELD;
                }
                val = val.Replace(";",string.Empty);
                var docParams = new List<string>(); 
                var res = val.Split(':');
                docParams.Add(res[0]);
                docParams.Add(res[1]);
                docParams.Add(_currentAccessType.ToString().ToLower());
                ExcelExporter.AppendLine(Color.White,docParams.ToArray());
            }

            
            private void ClassDocumentation(string val)
            {
                if(_currentKeyWord != KeyWordType.CLASS)
                {
                    string[] headers = {"Название","Тип","Базовый класс и интерфейсы","Описание"};
                    ExcelExporter.AppendLine(Color.GreenYellow,headers);
                    _currentKeyWord = KeyWordType.CLASS;
                    _currentAccessType = KeyWordType.PRIVATE;
                }
                val = val.Replace(";",string.Empty);
                var docParams = new List<string>(); 
                var typeAndName = val.Split('=');
                docParams.Add(typeAndName[0]);
                var parent = typeAndName[1].Split('(');
                docParams.Add(parent[0]);
                if(parent.Length > 1) docParams.Add(parent[1].Replace(")",string.Empty));

                ExcelExporter.AppendLine(Color.White,docParams.ToArray());
            }
        }

        private static class ExcelExporter
        {
            private static int _currentLine = 0;

            public static void AppendLine(Color fillColor,params string[] values)
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
                        worksheet.Cells[_currentLine,i].Style.Fill.PatternType =ExcelFillStyle.Solid;
                        worksheet.Cells[_currentLine,i].Style.Fill.BackgroundColor.SetColor(fillColor);
                        worksheet.Cells[_currentLine,i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    
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
                {KeyWordType.CLASS,@"^\w*\s*=\s*class\D*"},
                {KeyWordType.PROCEDURE,@"^procedure\s*\D*"},
                {KeyWordType.FUNCTION,@"^function\s*\D*"},
                {KeyWordType.FIELD,@"^\w*\s*:\s*\w*"},
                {KeyWordType.PUBLIC,@"^public"},
                {KeyWordType.PROTECTED,@"^protected"},
                {KeyWordType.PRIVATE,@"^private"}
            };

            public MatchCollection CheckText(KeyWordType wordType,string text)
            {
                var reg = string.Empty;
                text = text.Trim();
                _keyWords.TryGetValue(wordType,out reg);

                if(reg != null)
                {
                    var regex = new Regex(reg,RegexOptions.IgnoreCase);
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
            UNIT,
            CLASS,
            PROCEDURE,
            FUNCTION,
            FIELD,
            PUBLIC,
            PRIVATE,
            PROTECTED,
            NULL
        }
    }
}
