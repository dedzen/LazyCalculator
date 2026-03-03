// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Mime;
using System.Text;

public enum TokenType
{
    Number,
    Operator,
    Bracket
}

public class Token
{
    public required TokenType tokenType {get; set;}
    public required string content {get; set;}
    public override string ToString()
    {
        return $"Token: {tokenType} {content}";
    }
    public static string PrintList(Token[] input)
    {
        string result = "";
        foreach(Token t in input)
        {
            result+= t.content + " ";
        }
        return result.Substring(0, result.Length-1);
    }

}
class Program
{
    static int GetOpPriority(Token t)
    {
        int result;
        switch (t.content)
        {
            case "+":
                result=1;
                break;
            case "-":
                result=1;
                break;
            case "*":
                result=2;
                break;
            case "/":
                result=2;
                break;
            case "^":
                result=3;
                break;
            default:
                result=-1;
                break;
        }
        return result;
    } 
    static Token[] Tokenize(string input)
    {
        input += "|"; //I use this to terminate the string in parsing process
        
        List<Token> result = new List<Token>();
        
        string number = "";
        bool parsingNumber = false;
        string longOperator = "";
        bool parsingOperator = false;

        char[] allowed = ['m','i', 'n', 'a', 'x', 's', 'c', 'o'];  //characteers a function name may contain
        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                parsingNumber = true;
                number += c;
            }
            else
            {
                if (parsingNumber)
                {
                    parsingNumber = false;
                    result.Add(new Token{tokenType=TokenType.Number, content=number});
                    number="";
                }
                if (allowed.Contains(c))
                {
                    parsingOperator = true;
                    longOperator += c;
                }
                if(c!='|' && c!= ' ')
                {
                    if(c=='(' || c == ')')
                    {
                        if (parsingOperator)
                        {
                            parsingOperator = false;
                            result.Add(new Token{tokenType=TokenType.Operator, content=longOperator});
                            longOperator="";
                        }
                       
                        result.Add(new Token{tokenType=TokenType.Bracket, content=c.ToString()});
                    }
                    else if (!allowed.Contains(c))
                    {
                        result.Add(new Token{tokenType=TokenType.Operator, content=c.ToString()});
                    }
                
                    
                }
                
            }
        }
        return [.. result];
    }
    static Token[] SortAlgorithm(Token[] input)
    {
        Stack<Token> stack = new();
        Queue<Token> queue = new();
        foreach(Token t in input)
        {
            if (t.tokenType == TokenType.Number)
            {
                queue.Enqueue(t);
            }
            else if (t.tokenType == TokenType.Operator)
            {
                if (stack.Count == 0)
                {
                    stack.Push(t);
                    continue;
                }
                bool shouldPush = false;
                switch (stack.Peek().tokenType)
                {
                    case TokenType.Operator:
                        if (GetOpPriority(stack.Peek()) < GetOpPriority(t) || (GetOpPriority(stack.Peek())==GetOpPriority(t)  && t.content=="^")){shouldPush=true;}
                        break;
                    case TokenType.Bracket:
                        shouldPush=true;
                        break;
                }
                
                if (!shouldPush)
                {
                    while (true)
                    {
                        if (stack.TryPeek(out Token top))
                        {
                            if (top.tokenType == TokenType.Bracket){
                                stack.Push(t);
                                break;
                            }
                            if (GetOpPriority(top) > GetOpPriority(t) || (GetOpPriority(top)==GetOpPriority(t) && t.content!="^"))
                            {
                                queue.Enqueue(stack.Pop());
                                if (stack.Count == 0)
                                {
                                    stack.Push(t);
                                    break;
                                }
                            }
                            else
                            {
                                stack.Push(t);
                                break;
                            }
                        }
                    }
                    
                }
                else
                {
                    stack.Push(t);
                }
                
            }
            else if (t.tokenType == TokenType.Bracket)
            {
                if (t.content == "(")
                {
                    stack.Push(t);
                }
                if (t.content == ")")
                {
                    while (true)
                    {
                        Token top = stack.Pop();
                        if (top.content == "(")
                        {
                            break;
                        }
                        else
                        {
                            queue.Enqueue(top);
                        }
                    }
                }
            }
        }

        while(stack.Count != 0)
        {
            queue.Enqueue(stack.Pop());
        }
        return queue.ToArray();
        

    }
    
    static float Calculate(Token[] input)
    {
        Stack<float> stack = new();
        foreach (Token t in input)
        {
            if (t.tokenType == TokenType.Number)
            {
                stack.Push(float.Parse(t.content));
            }
            else
            {
                float t1 = stack.Pop();
                float t2 = stack.Pop();
                float result = 0;
                switch (t.content)
                {
                    case "+":
                        result = t2+t1;
                        break;
                    case "-":
                        result = t2-t1;
                        break;
                    case "*":
                        result = t2*t1;
                        break;
                    case "/":
                        result = t2/t1;
                        break;
                    case "^":
                        double d1 = (double)t1;
                        double d2 = (double)t2;
                        result = (float)Math.Pow(d2,d1);
                        break;
                    default:
                        break;
                }
                stack.Push(result);

            }
        }
        return stack.Pop();
    }
    
    
    static void Main(string[] args)
    {
        while (true)
        {
            Console.Write(">>> ");
            string? input = Console.ReadLine();
            if (input == "quit" || input == "exit")
            {
                break;
            }
            if(input?.Length >= 3)
            {
                Token[] tokenList = Tokenize(input);
                foreach(Token t in tokenList)
                {
                    Console.WriteLine(t);
                }
                tokenList = SortAlgorithm(tokenList);
                Console.WriteLine("Sorting..");
                Console.WriteLine(Token.PrintList(tokenList));
                Console.WriteLine(Calculate(tokenList));
            }
            
        }

        Console.WriteLine("Finished, terminating");
    }
}



