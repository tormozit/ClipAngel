using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClipAngel
{
    class SyntaxHighlighter
    {
        // Истина, если текущий символ находится в строке " "
        bool InLine;
        List<string> keyWords = new List<string>()
        {
            "if",
            "если",
            "then",
            "тогда",
            "elsif",
            "иначеесли",
            "else",
            "иначе",
            "endif",
            "конецесли",
            "do",
            "цикл",
            "for",
            "для",
            "to",
            "по",
            "each",
            "каждого",
            "in",
            "из",
            "while",
            "пока",
            "enddo",
            "конеццикла",
            "procedure",
            "процедура",
            "endprocedure",
            "конецпроцедуры",
            "function",
            "функция",
            "endfunction",
            "конецфункции",
            "var",
            "перем",
            "export",
            "экспорт",
            "goto",
            "перейти",
            "and",
            "и",
            "or",
            "или",
            "not",
            "не",
            "val",
            "знач",
            "break",
            "прервать",
            "continue",
            "продолжить",
            "return",
            "возврат",
            "try",
            "попытка",
            "except",
            "исключение",
            "endtry",
            "конецпопытки",
            "raise",
            "вызватьисключение",
            "false",
            "ложь",
            "true",
            "истина",
            "undefined",
            "неопределено",
            "null",
            "new",
            "новый",
            "execute",
            "выполнить"
        };
        private string keyWordPattern = "";
        private string operatorPattern = @"[\+\*\/\%\=\>\<]";
        private string notLetterPattern = @"(?:[^ёа-яa-z]|^|$)";
        private string funcCallPattern = @"[_ёа-яa-z]\(";

        public SyntaxHighlighter()
        {
            foreach (var keyWord in keyWords)
            {
                if (false
                    || keyWord == "false"
                    || keyWord == "true"
                    || keyWord.Length < 4)
                {
                    continue;
                }
                if (!String.IsNullOrEmpty(keyWordPattern))
                {
                    keyWordPattern += "|";
                }
                keyWordPattern += keyWord;
            }
            keyWordPattern = notLetterPattern + "(?:" + keyWordPattern + ")" + notLetterPattern;
        }

        // Возвращается строка CSS
        //
        // Возвращаемое значение:
        //   Строка CSS
        //
        private string StylePart()
        {
            return
            @"pre
            {
             font - family: Courier;
             color: #0000FF;
             font - size: 9pt;
            }
            .k
            {
             color: red;
            }
            .c
            {
             color: green;
            }
            .s
            {
             color: black;
            }
            .n
            {
             color: black;
            }
            .p
            {
             color: brown;
            }
            ";
        }

        // var получения символа из строки в заданной позиции
        // 
        // Параметры:
        //   Строка - Строка, из которой берется символ
        //   Position    - Позиция получаемого символа в строке
        //
        // Возвращаемое значение:
        //   Symbol из запрашиваемой позиции
        //
        string GetSymbol(string Line, int Position)
        {
            if (Line.Length > Position)
                return Line.Substring(Position, 1);
            return "";
        }

        public int isLineLike1C(string Line)
        {
            Line = Line.TrimStart();
            if (String.IsNullOrEmpty(Line))
                return 0;
            if (Line.StartsWith("//") || Line.StartsWith("|"))
                return 1;
            int qoutePos = Line.IndexOf(@"""");
            if (qoutePos >=0 )
                Line = Line.Remove(qoutePos);
            if (String.IsNullOrEmpty(Line))
                return 0;
            if (Line.EndsWith(".")||Line.Contains("><"))
                return -1;
            if (Regex.Match(Line, keyWordPattern).Success)
                return 1;
            if (Regex.IsMatch(Line, operatorPattern) || Regex.IsMatch(Line, funcCallPattern) || Line.Contains(");"))
                return 0;
            return -1;
        }

        // Token проверяется на принадлежность к ключевым словам 
        // встроенного языка 1С:Предприятие 8.0
        //
        // Параметры:
        //   _Токен - проверяемый токен
        //
        // Возвращаемое значение:
        //   Истина, если токен является ключевым словом встроенного языка, Ложь - не является.
        //
        bool IsKeyword(string _Токен)
        {
            string Token = _Токен.ToLower();
            return keyWords.Contains(Token);
        }

        // Проверяется символ на принадлежность к специальным символам
        //
        // Параметры:
        //   _Symbol - Проверяемый символ
        //
        // Возвращаемое значение:
        //   Истина, если _Symbol является специальным символом, Ложь - не является.
        //
        bool IsSpecialSymbol(string _Symbol)
        {
            string Symbol = _Symbol.ToLower();
            return false ||
                   Symbol == ")" ||
                   Symbol == "(" ||
                   Symbol == "[" ||
                   Symbol == "]" ||
                   Symbol == "." ||
                   Symbol == "," ||
                   Symbol == "=" ||
                   Symbol == "+" ||
                   Symbol == "-" ||
                   Symbol == "<" ||
                   Symbol == ">" ||
                   Symbol == ";" ||
                   Symbol == "?" ||
                   Symbol == "*";
        }

        // Процедура раскраски токена
        //
        // Параметры:
        //   codeLine - Текущая строка кода
        //   Token      - Token, который окрашивается
        //   Position        - Позиция начала Токена в текущей строке
        //   Класс      - Класс, к которому принадлежит токен
        //
        void ProcessToken(ref string codeLine, string Token, ref int Position, string Класс)
        {
            int tokenLength = Token.Length;
            codeLine = codeLine.Substring(0, Position - tokenLength + 1) +
                         "<span class=" + Класс + ">" +
                         codeLine.Substring(Position - tokenLength + 1, tokenLength) +
                         "</span>" +
                         codeLine.Remove(0, Position + 1);
            Position = Position + ("<span class=>" + "</span>" + Класс).Length;
        }

        // Основная функция раскрашивания кода
        // 
        // Параметры:
        //   codeLine - Раскрашивание происходит построчно, этот параметр - текущая строка
        //
        // Возвращаемое значение:
        //   codeLine - раскрашенная строка кода
        //
        string ProcessLine(string codeLine)
        {
            int Position = 0;
            int State = 0;
            string Token = "";
            int LineBegin = 0;
            // Последовательно перебираются все символы строки кода
            while (Position != codeLine.Length)
            {
                string CurrentSymbol = GetSymbol(codeLine, Position);
                if (CurrentSymbol == "/")
                    State = 1;
                else if (CurrentSymbol == "\t" || CurrentSymbol == " ")
                    State = 2;
                else if (CurrentSymbol == "\"")
                    State = 3;
                else if (CurrentSymbol == "")
                    State = 5;
                else if (IsSpecialSymbol(CurrentSymbol))
                    State = 6;
                else if (CurrentSymbol == "#" || CurrentSymbol == "&")
                    State = 8;
                else
                    State = 4;

                // Проверяется на комментарий или на символ деления
                if (State == 1)
                {
                    if (!InLine)
                    {
                        if (GetSymbol(codeLine, Position + 1) == "/")
                        {
                            // Окрашиваем комментарий
                            codeLine = codeLine.Substring(0, Position) +
                                       "<span class=c>" +
                                       System.Web.HttpUtility.HtmlEncode(codeLine.Remove(0, Position)) +
                                       "</span>";
                            return codeLine;
                        }
                        else
                        {
                            // Это символ деления
                            ProcessToken(ref codeLine, CurrentSymbol, ref Position, "k");
                            Token = "";
                        }
                    }
                }
                // Операции при встрече символа табуляции или пробела
                else if (State == 2)
                {
                    if (!InLine)
                    {
                        if (!String.IsNullOrWhiteSpace(Token))
                        {
                            Position = Position - 1;
                            // Пробел после после токена, значит
                            // токен - ключевое слово...
                            if (IsKeyword(Token))
                            {
                                ProcessToken(ref codeLine, Token, ref Position, "k");
                            }
                            else
                            {
                                // ... или число
                                bool success = false;
                                try
                                {
                                    int dummy = Convert.ToInt32(Token);
                                    success = true;
                                }
                                catch
                                { }
                                if (success)
                                    ProcessToken(ref codeLine, Token, ref Position, "n");
                            }
                            Position = Position + 1;
                            Token = "";
                        }
                    }
                }
                // Операции встрече символа "кавычка"
                else if (State == 3)
                {
                    // Нашли парную кавычку - окрашиваем как строку
                    if (InLine)
                    {
                        ProcessStringLiteral(LineBegin, ref codeLine, ref Position);
                        Token = "";
                        InLine = false;
                    }
                    // Первая кавычка, запоминаем позицию и взводим флаг нахождения в строке
                    else
                    {
                        LineBegin = Position;
                        InLine = true;
                    }
                }
                // Встретился один из специальных символов
                else if (State == 6)
                {
                    if (!InLine)
                    {
                        if (!String.IsNullOrWhiteSpace(Token))
                        {
                            Position = Position - 1;
                            // Дабы избежать окраски метода объекта с совпадающим
                            // именем с одним из ключевых слов, проверяем текущий символ
                            // на "."
                            if (IsKeyword(Token) && (CurrentSymbol != "."))
                            {
                                ProcessToken(ref codeLine, Token, ref Position, "k");
                            }
                            else
                            {
                                // Не ключевое слово - значит число
                                bool success = false;
                                try
                                {
                                    int dummy = Convert.ToInt32(Token);
                                    success = true;
                                }
                                catch
                                {}
                                if (success)
                                    ProcessToken(ref codeLine, Token, ref Position, "n");
                            }
                            Position = Position + 1;
                            Token = "";
                        }
                        // Один из специальных символов
                        ProcessToken(ref codeLine, CurrentSymbol, ref Position, "k");
                    }
                }
                // Встретился символ препроцессора
                else if (State == 8)
                {
                    if (!InLine)
                    {
                        Position = codeLine.Length - 1;
                        ProcessToken(ref codeLine, codeLine, ref Position, "p");
                    }
                }
                // Остальные символы
                else if (State == 4)
                {
                    Token = Token + CurrentSymbol;
                }
                else if (State == 5)
                    break;
                Position = Position + 1;
            }

            // Многострочная строка
            if (InLine)
            {
                //codeLine = codeLine.Substring(0, LineBegin) +
                //             "<span class=s>" +
                //             System.Web.HttpUtility.HtmlEncode(codeLine.Substring(LineBegin, Position - LineBegin)) +
                //             "</span>" +
                //             codeLine.Remove(0, Position);
                //Position = Position + ("<span class=s>" + "</span>").Length;
                ProcessStringLiteral(LineBegin, ref codeLine, ref Position);
                Token = "";
            }

            // Анализируем последний токен строки кода
            if (!String.IsNullOrWhiteSpace(Token))
            {
                if (IsKeyword(Token))
                {
                    Position = Position - 1;
                    ProcessToken(ref codeLine, Token, ref Position, "k");
                    Position = Position + 1;
                }
            }
            return codeLine;
        }

        private static string ProcessStringLiteral(int LineBegin, ref string codeLine, ref int Position)
        {
            string literal = codeLine.Substring(LineBegin, Position - LineBegin);
            string newLiteral = "<span class=s>" + System.Web.HttpUtility.HtmlEncode(literal) + "</span>";
            codeLine = codeLine.Substring(0, LineBegin) + newLiteral + codeLine.Remove(0, Position);
            Position = Position + newLiteral.Length - literal.Length;
            return codeLine;
        }

        // var последовательно перебирает все строки переданного кода
        // 
        // Параметры:
        //   Код - Код, который подлежит окраске
        //
        // Возвращаемое значение:
        //   Буфер - Окрашенный код, заключенный в тег <pre>
        //
        public string ProcessCode(string code)
        {
            string result = "";
            //result += @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">" + "\n";
            result += "<html>";
            result += "<head>\n";
            result += "<style type=text/css>";
            result += StylePart();
            result += "</style>\n";
            result += "</head>\n";
            result += "<body>";
            result += "<!--StartFragment-->";
            result += "<pre>\n";
            // Последовательно перебираются все строки кода, окрашиваются
            // и записываются в буфер
            code = code.Replace("\t", "    ");
            code = Regex.Replace(code, "(\r\n|\n\r|\n|\r)", "\n");
            string[] lines = code.Split("\n"[0]);
            for (int index0 = 0; index0 < lines.Length; index0++)
            {
                string codeLine = lines[index0];
                result += ProcessLine(codeLine) + "\n";
            }
            result += "</pre>";
            result += "<!--EndFragment-->";
            result += "</body>";
            result += "</html>";
            return result;
        }
    }
}
