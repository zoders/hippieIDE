using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
namespace hippieIDE
{
    class Outputs
    {
        public int WordNum { get; set; }
        public string Text { get; set; }
        public bool ExitCode { get; set; }
        public Outputs(int wordNum, string text, bool exitCode)
        {
            WordNum = wordNum;
            Text = text;
            ExitCode = exitCode;
        }

    }

    class HippieLang
    {
        private readonly Dictionary<string, double> doubleVariables = new Dictionary<string, double>();
        private readonly Dictionary<int, string> labelVars = new Dictionary<int, string>();
        private string[] words;
        private bool Minus;
        private bool Real;
        public string InputOutput(string text, out int i, out string[] outwords, out bool exitCode, bool outputmode, 
            bool spaces, bool minus, bool real)
        {
            Minus = minus;
            Real = real;
            text = System.Text.RegularExpressions.Regex.Replace(text, @":", " : ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @";", " ; ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"=", " = ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\+", " + ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"-", " - ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*", " * ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\/", " / ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\^", " ^ ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\[", " [ ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\]", " ] ");
            if (spaces)
            {
                text = System.Text.RegularExpressions.Regex.Replace(text, @"cos", " cos ");
                text = System.Text.RegularExpressions.Regex.Replace(text, @"sin", " sin ");
            }
            text = text.ToLower().Replace('\n', ' ').Trim();
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            words = outwords = text.Split(' ');
            i = 0;
            if (words[0] != "start")
            {
                exitCode = false;
                return "Ошибка! Программа должна начинаться со слова \"start\"";
            }
            if (words.Length == 1)
            {
                exitCode = false;
                return "Ошибка! Ожидались метка или переменная " +
                            "(переменная -- две буквы, потом три цифры)";
            }
            Outputs operatorsOutput = OperatorsReader();
            i = operatorsOutput.WordNum;
            if (!operatorsOutput.ExitCode)
            {
                exitCode = false;
                return operatorsOutput.Text;
            }
            if (words.Length == operatorsOutput.WordNum)
            {
                i--;
                exitCode = false;
                //return "Ошибка! Ожидалось сочетание или продолжение объявления операторов";
                return "Ошибка! Ожидалось продолжение объявления операторов";
            }
            bool integer = false;
            string s = "----------Output----------\n";

            bool b = false;
            for (; i < words.Length; i++)
            {
                if (words[i] != "int" && words[i] != "real" && words[i] != "label")
                {
                    if (b)
                    {
                        
                        if (words[i] == "stop")
                            break;
                        exitCode = false;
                        return "Ошибка! Ожидалось сочетание или слово 'stop'";
                    }

                    exitCode = false;
                    return "Ошибка! Ожидалось сочетание или продолжение объявления операторов";
                }
                b = true;
                if (words[i] == "int" || words[i] == "real")
                {
                    if (words.Length - 1 == i)
                    {
                        exitCode = false;
                        return $"Ошибка! После {words[i]} ожидалась переменная (переменная -- две буквы, потом три цифры)";
                    }
                    if (words[i] == "int")
                        integer = true;
                    int countVars = 0;
                    for (i += 1; i < words.Length; i++)
                    {
                        if (doubleVariables.ContainsKey(words[i]))
                        {
                            string svalue;
                            if (integer)
                                svalue = Convert.ToInt64(doubleVariables[words[i]]).ToString();
                            else
                                svalue = doubleVariables[words[i]].ToString();
                            integer = false;
                            s += words[i] + '=' + svalue + '\n';
                            
                            countVars++;
                        }
                        else if (!IsVariable(words[i]))
                        {
                            if (countVars != 0)
                            {
                                if (words[i] == "stop")
                                    break;
                                else if (words[i] == "int" || words[i] == "real" || words[i] == "label")
                                {
                                    i--;
                                    break;
                                }
                            }
                            exitCode = false;
                            if (words[i] == "stop")
                                break;
                            return $"Ошибка! '{words[i]}' -- не переменная (переменная -- две буквы, потом три цифры)";
                        }
                        else
                        {
                            if (outputmode)
                            {
                                exitCode = false;
                                return $"Ошибка! Переменная '{words[i]}' не объявлена";
                            }
                        }

                        if (words.Length - 1 == i)
                        {
                            exitCode = false;
                            return "Ошибка! Ожидалась слово 'stop', новое сочетание или продолжение перечисления переменных";
                        }
                    }
                    if (words[i] == "stop")
                        break;
                    continue;
                }
                
                if (words[i] == "label")
                {
                    if (words.Length - 1 == i)
                    {
                        exitCode = false;
                        return $"Ошибка! После {words[i]} ожидалась метка";
                    }
                    i++;
                    if (!Int32.TryParse(words[i], out int n))
                    {
                        exitCode = false;
                        return $"Ошибка! '{words[i]}' -- не метка";
                    }
                    else if (labelVars.ContainsKey(n))
                    {
                        s += labelVars[n] + '=' + doubleVariables[labelVars[n]].ToString() + '\n';
                    }
                    else
                    {
                        if (outputmode)
                        {
                            exitCode = false;
                            return $"Ошибка! Метка '{words[i]}' не объявлена";
                        }
                    }
                    
                    if (words.Length - 1 == i)
                    {
                        exitCode = false;
                        return "Ошибка! Ожидалась слово 'stop' или новое сочетание";
                    }
                    if (words[i+1] == "int" || words[i+1] == "real" || words[i+1] == "label" || words[i+1] == "stop")
                        continue;
                    else
                    {
                        if (double.TryParse(words[i + 1], out double gkdg))
                        {
                            i++;
                            exitCode = false;
                            return "Ошибка! В сочетании не может быть больше одной метки";
                        }
                        i++;
                        exitCode = false;
                        return "Ошибка! Ожидалась слово 'stop' или новое сочетание";
                    }
                }
            }

            if (words[words.Length - 1] != "stop")
            {
                exitCode = false;
                i++;
                return "Ошибка! Программа должна заканчиваться словом \"stop\"";
            }
            if (words[words.Length - 1] == "stop" && words[words.Length - 2] == "stop")
            {
                i++;
                exitCode = false;
                return "Ошибка! Лишний \"stop\"";
            }
            if (!outputmode)
            {
                s = "----------Output----------\n";
                foreach (KeyValuePair<string, double> vr in doubleVariables)
                {
                    s += vr.Key + '=' + vr.Value + '\n';
                }
            }
            exitCode = true;
            return s;
        }
        private Outputs OperatorsReader()
        {
            Outputs output = new Outputs(0, "", true);
            int j = 1;
            int label = 0;
            bool twoplus = false;
            bool isNum = false;
            bool isVar;
            string variable = "";
            List<string> math = new List<string>();
            int i = 1;
            int equality = 0;
            for (; i < words.Length; i++)
            {
                if (j == 1)
                {
                    isNum = IsNumber(words[i]);
                    isVar = IsVariable(words[i]);
                    if (!isNum && !isVar)
                    {
                        string forCombination = "";
                        if (twoplus)
                        {
                            if ((words[i] != "int" && words[i] != "real" && words[i] != "label"))
                                forCombination = "сочетания, ";
                            if (words[i] == "int" || words[i] == "real" || words[i] == "label")
                            {
                                output.WordNum = i;
                                return output;
                            }
                        }
                        output.Text = $"Ошибка! Ожидались {forCombination}метка или переменная " +
                            "(переменная -- две буквы, потом три цифры)";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    if (isNum)
                    {
                        if (i == words.Length - 1)
                        {
                            output.Text = "Ошибка! После метки ожидался символ ':'";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return output;
                        }
                        int.TryParse(words[i], out int n);
                        label = n;
                        j = 2;
                        
                    }
                    if (isVar)
                    {
                        if (i == words.Length - 1)
                        {
                            output.Text = "Ошибка! После переменной ожидался знак '='";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return output;
                        }
                        j = 4;
                        variable = words[i];
                    }
                    continue;
                }
                if (j == 2)
                {
                    if (IsNumber(words[i]))
                    {
                        continue;
                    }
                    if (words[i] != ":")
                    {
                        output.Text = "Ошибка! После метки ожидался символ ':'";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    if (i == words.Length - 1)
                    {
                        output.Text = "Ошибка! После объявления метки ожидалась переменная " +
                            "(переменная -- две буквы, потом три цифры)";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    j = 3;
                    continue;
                }
                if (j == 3)
                {
                    if (!IsVariable(words[i]))
                    {
                        output.Text = "Ошибка! После объявления метки ожидалась переменная " +
                            "(переменная -- две буквы, потом три цифры)";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    variable = words[i];
                    if (i == words.Length - 1)
                    {
                        output.Text = "Ошибка! После переменной ожидался знак '='";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    if (isNum)
                    {
                        if (!labelVars.ContainsKey(label))
                            labelVars.Add(label, variable);
                        else
                            labelVars[label] = variable;
                    }
                    j = 4;
                    
                    continue;
                }
                if (j == 4)
                {
                    if (words[i] != "=")
                    {
                        output.Text = "Ошибка! После переменной ожидался знак '='";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    if (i == words.Length - 1)
                    {
                        output.Text = "Ошибка! После '=' ожидались число, функции, объявленные переменные или знаки '-' и '['";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    equality = i;
                    j = 5;
                    continue;
                }
                if (j == 5)
                {
                    if (words[i] != ";" && words[i] != "label" && words[i] != "int" && words[i] != "real")
                    {
                        math.Add(words[i]);
                        if (i == words.Length - 1)
                        {
                            output.WordNum = i;
                            RightPartReader(math, variable, ref output, equality);
                            if (!output.ExitCode)
                                return output;
                            
                            output.Text = "Ошибка! Ожидались символы '+', '-', '*', '/', '^' или открывающая скобка" +
                                " или прекращение объявления правой части";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return output;
                        }
                        continue;
                    }
                    if (i + 1 <= words.Length - 1)
                    {
                        if (words[i + 1] == "label" || words[i + 1] == "int" || words[i + 1] == "real")
                        {
                            output.Text = "Ошибка! Последний оператор не заканчивается символом ';'";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return output;
                        }
                    }
                    if (words[i] == "label" || words[i] == "int" || words[i] == "real")
                    {
                        output.WordNum = i;
                        RightPartReader(math, variable, ref output, equality);
                        if (!output.ExitCode)
                            return output;
                        output.WordNum = i;
                        output.ExitCode = true;
                        return output;
                    }
                    j = 6;
                }
                if (j == 6)
                {
                    if (!math.Any())
                    {
                        output.Text = "Ошибка! После '=' ожидались число, функции, объявленные переменные или знаки '-' и '['";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return output;
                    }
                    output.WordNum = i;
                    RightPartReader(math, variable, ref output, equality);
                    if (!output.ExitCode)
                        return output;
                    j = 1;
                    twoplus = true;
                    math.Clear();
                }
            }
            if (math.Any())
            {
                output.WordNum = i;
                RightPartReader(math, variable, ref output, equality);
                if (!output.ExitCode)
                    return output;
            }
            output.WordNum = i;
            return output;
        }
        private void RightPartReader(List<string> math, string variable, ref Outputs output, int i)
        {
            
            double sum = 0;
            int funbr = 0;
            int funfunbrackets = 0;
            string prev = "";
            string expression = "";
            int first = i;
            bool funminus = false;
            Stack<int> brackets = new Stack<int>();
            foreach (string m in math)
            {

                //double d;
                i++;

                bool isNum = double.TryParse(System.Text.RegularExpressions.Regex.Replace(m, @",", "."), out double d);
                bool isPrevNum = double.TryParse(System.Text.RegularExpressions.Regex.Replace(prev, @",", "."), out d);
                if (doubleVariables.ContainsKey(m))
                {
                    isNum = true;
                }
                if (doubleVariables.ContainsKey(prev))
                {
                    isPrevNum = true;
                }
                if (funminus && Minus)
                {
                    if (IsVariable(m))
                    {
                        if (doubleVariables.ContainsKey(m))
                        {
                            expression += $"(" + doubleVariables[m] + ")";
                            prev = m;
                            while (funfunbrackets != 0)
                            {
                                expression += ")";
                                funfunbrackets--;
                            }
                            funminus = false;
                            //isPrevNum = true;
                            continue;
                        }
                        else
                        {
                            output.Text = $"Ошибка! Переменная '{m}' не объявлена";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                    }
                    if (isNum)
                    {
                        expression += $"(" + m + ")";
                        if (m[m.Length - 1] == '.' || m[m.Length - 1] == ',')
                        {
                            output.Text = "Ошибка! У числа отсутвует дробная часть";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if (m[0] == '.' || m[0] == ',')
                        {
                            output.Text = "Ошибка! У числа отсутвует целая часть";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        prev = m;
                        //isPrevNum = true;
                        while (funfunbrackets != 0)
                        {
                            expression += ")";
                            funfunbrackets--;
                        }
                        funminus = false;
                        continue;
                    }
                }

                if (prev != "")
                {
                    if ((prev == "sin") || (prev == "cos"))
                    {
                        if (m == "-" && Minus)
                        {
                            if (i <= words.Length - 2)
                            {
                                if (double.TryParse(words[i + 1], out double lala) || IsVariable(words[i+1]))
                                {
                                    funminus = true;
                                    continue;
                                }
                                else
                                {
                                    output.Text = $"Ошибка! После минуса ожидалось число или объявленная переменная";
                                    output.WordNum = i+1;
                                    output.ExitCode = false;
                                    return;
                                }
                            }
                           
                            if (i == words.Length - 1)
                            {
                                output.Text = $"Ошибка! После минуса ожидалось число или объявленная переменная";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                        }
                        else if (m == "-")
                        {
                            output.Text = $"Ошибка! После функции {prev} не может быть минуса!!!";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if (m == "+" || m == "*" || m == "/" || m == "^")
                        {
                            output.Text = $"Ошибка! После функции {prev} не может быть знака '{m}'!!!";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if ((m == "sin") || (m == "cos"))
                        {
                            expression += $"({m}";
                            funfunbrackets++;
                            continue;
                        }
                        if (IsVariable(m))
                        {
                            if (doubleVariables.ContainsKey(m))
                            {
                                expression += $"(" + doubleVariables[m] + ")";
                                prev = m;
                                while (funfunbrackets != 0)
                                {
                                    expression += ")";
                                    funfunbrackets--;
                                }
                                //isPrevNum = true;
                                continue;
                            }
                            else
                            {
                                output.Text = $"Ошибка! Переменная '{m}' не объявлена";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                        }
                        if (m != "[" && !isNum && m != "cos" && m != "sin")
                        {
                            output.Text = $"Ошибка! После функции '{prev}' ожидались '[', функция, объявленная переменная или число";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        
                        if (isNum) 
                        {
                            if (Real)
                            {
                                int pnts = 0;
                                for (int v = 0; v < m.Length; v++)
                                {
                                    if (m[v] == '.' || m[v] == ',')
                                        pnts++;
                                }
                                if (pnts == 0)
                                {
                                    output.Text = "Ошибка! У числа должны быть целая и дробная часть, разделенные точкой";
                                    output.WordNum = i;
                                    output.ExitCode = false;
                                    return;
                                }
                            }
                            expression +=  $"(" + m + ")";
                            if (m[m.Length - 1] == '.' || m[m.Length - 1] == ',')
                            {
                                output.Text = "Ошибка! У числа отсутвует дробная часть";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                            if (m[0] == '.' || m[0] == ',')
                            {
                                output.Text = "Ошибка! У числа отсутвует целая часть";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                            prev = m;
                            //isPrevNum = true;
                            while (funfunbrackets != 0)
                            {
                                expression += ")";
                                funfunbrackets--;
                            }
                            continue;
                        }
                        if (m == "[")
                        {
                            expression += '(';
                            brackets.Push(i);
                            prev = "[";
                            funbr = i;
                            continue;
                        }

                    }

                    if (prev == "[")
                    {
                        if (!isNum && m != "[" && m != "-" && (m != "sin") && (m != "cos"))
                        {
                            output.Text = "Ошибка! После '[' ожидалось число, функция, знак '-' или '['";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if (m == "[")
                        {
                            expression += '(';
                            brackets.Push(i);
                            prev = "[";
                            continue;
                        }
                    }
                    if (prev == "]")
                    {
                        if ((m != "+") && (m != "-") && (m != "*") && (m != "/") && (m != "^") && (m != "]"))
                        {
                            output.Text = "Ошибка! После ']' ожидались знаки '+', '-', '*', '/','^', ']'";
                            // +  " или прекращение ввода правой части";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if (m == "]")
                        {
                            if (brackets.Count > 0)
                            {
                                if (brackets.Peek() == funbr)
                                {
                                    while (funfunbrackets != 0)
                                    {
                                        expression += ")";
                                        funfunbrackets--;
                                    }
                                }
                                brackets.Pop();
                            }
                            else
                            {
                                output.Text = "Ошибка! Лишняя закрывающая скобка";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                        }
                    }
                    if (isPrevNum)
                    {
                        int k = i;
                        if (words.Length - 1 >= k + 1)
                        {
                            if (IsVariable(words[k]) && words[k + 1] == "=")
                            {
                                if (IsVariable(words[i - 1]))
                                {
                                    output.Text = $"Ошибка! Поставьте ';' переменной и объявлением переменной";
                                    output.WordNum = i;
                                    output.ExitCode = false;
                                    return;
                                }
                                output.Text = $"Ошибка! Поставьте ';' между числом и объявлением переменной";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                        }
                        if (isNum)
                        {
                            int j = i;
                            while (j != words.Length - 1)
                            {
                                if (double.TryParse(words[j], out double dudik))
                                    j++;
                                else
                                    break;



                            }
                            if (words[j] == ":" && j != 0)
                            {
                                if (IsVariable(words[i - 1]))
                                {
                                    output.Text = $"Ошибка! Поставьте ';' между меткой и переменной";
                                    output.WordNum = i;
                                    output.ExitCode = false;
                                    return;
                                }
                                output.Text = $"Ошибка! Поставьте ';' между меткой и числом";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                            output.Text = $"Ошибка! Между '{words[i - 1]}' и '{words[i]}' ожидались знаки '+', '-', '*', '/', '^'";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if ((m != "+") && (m != "-") && (m != "*") && (m != "/") && (m != "^") && m != "]")
                        {
                            string closebr = " или прекращение ввода правой части";
                            if (brackets.Count > 0)
                                closebr = " или закрывающая скобка";
                            string varnum = "числа";
                            if (IsVariable(prev))
                                varnum = "переменной";
                            output.Text = $"Ошибка! После {varnum} ожидались знаки '+', '-', '*', '/', '^'," + closebr;
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if (m == "]")
                        {
                            if (brackets.Count > 0)
                            {
                                if (brackets.Peek() == funbr)
                                {
                                    while (funfunbrackets != 0)
                                    {
                                        expression += ")";
                                        funfunbrackets--;
                                    }
                                }
                                brackets.Pop();
                            }
                            else
                            {
                                output.Text = "Ошибка! Лишняя закрывающая скобка";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                            int z = i + 1;
                            if (words.Length - 1 >= z + 1)
                            {
                                if (IsVariable(words[z]) && words[z + 1] == "=")
                                {
                                    output.Text = $"Ошибка! Поставьте ';' между ']' и объявлением переменной";
                                    output.WordNum = i;
                                    output.ExitCode = false;
                                    return;
                                }
                            }
                            int j = i + 1;
                            while (j != words.Length - 1)
                            {
                                if (double.TryParse(words[j], out double dudik))
                                    j++;
                                else
                                    break;



                            }
                            if (words[j] == ":" && j != 0)
                            {
                                output.Text = $"Ошибка! Поставьте ';' между меткой и ']'";
                                output.WordNum = i + 1;
                                output.ExitCode = false;
                                return;
                            }
                        }
                    }
                    if (prev == "+" || prev == "-" || prev == "*" || prev == "/" || prev == "^")
                    {
                        if (m == "+" || m == "-" || m == "*" || m == "/" || m == "^")
                        {
                            output.Text = $"Ошибка! Два арифметических знака '{prev}' и '{m}' подряд";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if ((m == "sin") || (m == "cos"))
                        {
                            expression += m;
                            prev = m;
                            continue;
                        }
                        if (m == "[")
                        {
                            expression += '(';
                            brackets.Push(i);
                            prev = "[";
                            continue;
                        }
                        if (!isNum && m != "[")
                        {
                            if (IsVariable(m) && !doubleVariables.ContainsKey(m))
                            {
                                output.Text = $"Ошибка! Переменная '{m}' не объявлена";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                            output.Text = $"Ошибка! После знака '{prev}' ожидались число, функции, объявленные переменные или открывающая скобка";// + i.ToString();
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        if (prev == "/")
                        {
                            double.TryParse(m, out double zero);
                            if (zero == 0)
                            {
                                output.Text = $"Ошибка! Деление на ноль";
                                output.WordNum = i;
                                output.ExitCode = false;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if ((m != "-") && (!isNum) && (m != "[") && (m != "sin") && (m != "cos"))
                    {
                        if (IsVariable(m) && !doubleVariables.ContainsKey(m))
                        {
                            output.Text = $"Ошибка! Переменная '{m}' не объявлена";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                        output.Text = "Ошибка! После '=' ожидались число, функции, объявленные переменные или знаки '-' и '['";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return;
                    }
                    if ((m == "sin") || (m == "cos"))
                    {
                        expression += m;
                        prev = m;
                        continue;
                    }
                    if (m == "[")
                        brackets.Push(i);

                }
                prev = m;
                if (doubleVariables.ContainsKey(m))
                {
                    if (prev == "/")
                    {
                        if (doubleVariables[m] == 0)
                        {
                            output.Text = $"Ошибка! Деление на ноль";
                            output.WordNum = i;
                            output.ExitCode = false;
                            return;
                        }
                    }
                    expression += doubleVariables[m].ToString();
                    continue;
                }

                if (m == "[")
                {
                    expression += '(';
                    continue;
                }
                if (m == "]")
                {
                    expression += ')';
                    continue;
                }
                if (Real && double.TryParse(m, out double vfjvfd))
                {
                    int pnts = 0;
                    for (int v = 0; v < m.Length; v++)
                    {
                        if (m[v] == '.' || m[v] == ',')
                            pnts++;
                    }
                    if (pnts == 0)
                    {
                        output.Text = "Ошибка! У числа должны быть целая и дробная часть, разделенные точкой";
                        output.WordNum = i;
                        output.ExitCode = false;
                        return;
                    }
                }
                if (m[m.Length - 1] == '.' || m[m.Length - 1] == ',')
                {
                    output.Text = "Ошибка! У числа отсутвует дробная часть";
                    output.WordNum = i;
                    output.ExitCode = false;
                    return;
                }
                if (m[0] == '.' || m[0] == ',')
                {
                    output.Text = "Ошибка! У числа отсутвует целая часть";
                    output.WordNum = i;
                    output.ExitCode = false;
                    return;
                }
                expression += m;
                
            }
            if (!double.TryParse(prev, out double p) && prev != "]" && !IsVariable(prev))
            {
                if (IsVariable(prev) && !doubleVariables.ContainsKey(prev))
                {
                    output.Text = $"Ошибка! Переменная '{prev}' не объявлена";
                    output.WordNum = i;
                    output.ExitCode = false;
                    return;
                }
                output.Text = $"Ошибка! После знака '{prev}' ожидались число, функция, объявленные переменные или открывющая скобка";
                output.WordNum = i;
                //output.WordNum = i + 1;
                output.ExitCode = false;
                return;
                
            }
            if (brackets.Count != 0)
            {
                output.Text = "Ошибка! Обнаружена незакрытая скобка";
                output.WordNum = brackets.Peek();
                output.ExitCode = false;
                return;
            }
            //expression = System.Text.RegularExpressions.Regex.Replace(expression, @"\(", "((");
            //expression = System.Text.RegularExpressions.Regex.Replace(expression, @"\)", "))");
            try {
                expression = System.Text.RegularExpressions.Regex.Replace(expression, @",", ".");
                sum = Parse(expression); 
            }
            catch (FormatException) {
                //expression = ex.Message;
                //try
                //{
                //    sum = Parse(expression);
                //}
                //catch (FormatException) {
                output.Text = $"Ошибка! {words[first - 1]} = NaN, т.к. невозможно вычислить степень с отрицательным аргументом и дробной степенью";
                output.WordNum = first - 1;
                output.ExitCode = false;
                return;
                //}
            }
            if (double.IsNaN(sum))
            {
                output.Text = $"Ошибка! {words[first - 1]} = NaN, т.к. невозможно вычислить степень с отрицательным аргументом и дробной степенью";
                output.WordNum = first-1;
                output.ExitCode = false;
                return;
            }
            if (!doubleVariables.ContainsKey(variable))
                doubleVariables.Add(variable, sum);
            else
                doubleVariables[variable] = sum;
            output.Text = sum.ToString() + '\n';
            }

        private bool IsVariable(string vr)
        {
            if (vr.Length == 5)
            {
                if (
                        ('a' <= vr[0] && vr[0] <= 'z') &&
                        ('a' <= vr[1] && vr[1] <= 'z') &&
                        ('0' <= vr[2] && vr[2] <= '9') &&
                        ('0' <= vr[3] && vr[3] <= '9') &&
                        ('0' <= vr[4] && vr[4] <= '9')
                    )
                    return true;
            }
            return false;
        }
        private bool IsNumber(string num)
        {
            return int.TryParse(num, out int n);
        }

        public static double Parse(string str)
        {
            string[] func = { "sin", "cos", "ctan", "tan" };
            for (int i = 0; i < func.Length; i++)
            {
                Match matchFunc = Regex.Match(str, string.Format(@"{0}\(({1})\)", func[i], @"[1234567890\.\+\-\*\/^%]*"));
                if (matchFunc.Groups.Count > 1)
                {
                    string inner = matchFunc.Groups[0].Value.Substring(1 + func[i].Length, matchFunc.Groups[0].Value.Trim().Length - 2 - func[i].Length);
                    string left = str.Substring(0, matchFunc.Index);
                    string right = str.Substring(matchFunc.Index + matchFunc.Length);

                    switch (i)
                    {
                        case 0:
                            return Parse(left + Math.Sin(Parse(inner)) + right);

                        case 1:
                            return Parse(left + Math.Cos(Parse(inner)) + right);

                        case 2:
                            return Parse(left + Math.Tan(Parse(inner)) + right);

                        case 3:
                            return Parse(left + 1.0 / Math.Tan(Parse(inner)) + right);
                    }
                }
            }
            Match matchSk = Regex.Match(str, string.Format(@"\(({0})\)", @"[1234567890\.\+\-\*\/^%]*"));
            if (matchSk.Groups.Count > 1)
            {
                string inner = matchSk.Groups[0].Value.Substring(1, matchSk.Groups[0].Value.Trim().Length - 2);
                string left = str.Substring(0, matchSk.Index);
                string right = str.Substring(matchSk.Index + matchSk.Length);
                if (left != "" && Parse(inner).ToString() != "")
                {
                    if (left[left.Length - 1] == '-' && Parse(inner).ToString()[0] == '-')
                    {
                        inner = inner.Remove(0, 1);
                        left = left.Remove(left.Length - 1, 1);
                    }

                    if (right != "" && Parse(inner).ToString() != "")
                    {
                        if (Parse(inner).ToString()[Parse(inner).ToString().Length - 1] == '-' && right[0] == '-')
                        {
                            inner = inner.Remove(0, 1);
                            right = left.Remove(left.Length - 1, 1);
                        }

                    }
                }
                return Parse(left + Parse(inner) + right);
            }
            Match matchMulOp = Regex.Match(str, string.Format(@"({0})\s?({1})\s?({0})\s?", RegexNum, RegexMulOp));
            Match matchAddOp = Regex.Match(str, string.Format(@"({0})\s?({1})\s?({2})\s?", RegexNum, RegexAddOp, RegexNum));
            var match = (matchMulOp.Groups.Count > 1) ? matchMulOp : (matchAddOp.Groups.Count > 1) ? matchAddOp : null;
            if (match != null)
            {
                string left = str.Substring(0, match.Index);
                string right = str.Substring(match.Index + match.Length);
                string val = ParseAct(match).ToString();
                return Parse(string.Format("{0}{1}{2}", left, val, right));
            }
            try
            {
                return double.Parse(str);
            }
            catch (FormatException)
            {
                throw new FormatException(string.Format("{0}", str));
            }
        }

        private const string RegexNum = @"[-]?\d+\.?\d*";
        private const string RegexMulOp = @"[\*\/^]";
        private const string RegexAddOp = @"[\+\-]";

        private static double ParseAct(Match match)
        {
            double a = double.Parse(match.Groups[1].Value);
            double b = double.Parse(match.Groups[3].Value);

            switch (match.Groups[2].Value)
            {
                case "+":
                    return a + b;

                case "-":
                    return a - b;

                case "*":
                    return a * b;

                case "/":
                    return a / b;

                case "^":
                    return Math.Pow(a, b);
                default:
                    throw new FormatException(string.Format("Неверная входная строка '{0}'", match.Value));
            }
        }
    }
}
