
public enum TokenType
{
    Number,  //number
    Operator, //binary infix operators (+)
    Bracket, //()
    UnaryFunction, //sin
    BinaryFunction, //max
    Variable, //variable or constant
    Comma
}

public static class Memory
{
    public static Dictionary<string, float> constants = new Dictionary<string, float>
    {
        {"pi", (float)Math.PI},
        {"G", 9.81f},
        {"eul", 2.71828f}
    }; 
    public static Dictionary<string, Node> variables = new Dictionary<string, Node>();
}

public class Token
{
    public TokenType tokenType {get; set;}
    public string content {get; set;}
    public Token(TokenType _tokenType, string _content)
    {
        tokenType = _tokenType;
        content = _content;
    }
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
    public static Token parseLiteral(string input)
    {
        string[] unaryFunctions = ["sin", "cos", "tan", "sqrt", "abs"];
        string[] binaryFunctions = ["max", "min", "lze"];
        if (unaryFunctions.Contains(input))
        {
            return new Token(TokenType.UnaryFunction, input);
        }
        else if (binaryFunctions.Contains(input))
        {
            return new Token(TokenType.BinaryFunction, input);
        }
        else
        {
            if (Memory.constants.TryGetValue(input, out float value))
            {
                return new Token(TokenType.Number, value.ToString());
            }
            else{return new Token(TokenType.Variable, input);}
            
        }
    }
    public static bool TryGetOpPriority(Token t, out int priority)
    {
        Dictionary<string, int> pairs = new Dictionary<string, int>
        {
            {"+", 1},
            {"-", 1},
            {"*", 2},
            {"/", 2},
            {"^", 3},
            {"sin", 10},
            {"cos", 10},
            {"tan", 10},
            {"abs", 10},
            {"sqrt", 10},
            {"max", 10},
            {"min", 10},
            {"lze", 10}
        };
        return pairs.TryGetValue(t.content, out priority);
    } 

}


class Program
{
    static bool render = true;
    static bool lazy = true;

    static Token[] Tokenize(string input)
    {
        input += "|"; //I use this to terminate the string in parsing process
        
        List<Token> result = new List<Token>();   
        bool parsingNumber = false;
        bool parsingLitelar = false;
        string tokenContent = "";

        char[] tokenStop = ['(',')', ' ', '|', ','];  //literals and numbers stop on these chars
        char[] binaryOps = ['+', '-', '*', '/', '^']; //allowed 'middle' binary operators
        char[] allowedSpecials = tokenStop.Concat(binaryOps).ToArray(); //all allowed special characters
        foreach (char c in input)
        {
            if (char.IsAsciiDigit(c) || c=='.')
            {
                parsingNumber = true;
                tokenContent += c;
            }
            else if (char.IsAsciiLetter(c))
            {
                parsingLitelar = true;
                tokenContent += c;
            }
            else
            {
                if (!allowedSpecials.Contains(c))
                {
                    throw new ArgumentException($"Unknown literal {c}");
                }
                if (parsingNumber)
                    {
                        result.Add(new Token(TokenType.Number, tokenContent));
                        tokenContent = "";
                        parsingNumber = false;
                    }
                else if (parsingLitelar)
                {
                    result.Add(Token.parseLiteral(tokenContent));
                    tokenContent = "";
                    parsingLitelar = false;
                }
                if (binaryOps.Contains(c))
                {
                    result.Add(new Token(TokenType.Operator, c.ToString()));
                }
                else if (c=='(' || c==')')
                {
                    result.Add(new Token(TokenType.Bracket, c.ToString()));
                }
                else if (c == ',')
                {
                    result.Add(new Token(TokenType.Comma, c.ToString()));
                }
            }
        }
            return [.. result];
    }
    static Token[] SortAlgorithm(Token[] input)
    {
        Stack<Token> stack = new();
        Queue<Token> queue = new();
        string[] operators = ["+", "-", "*", "/", "^"];
        
        foreach(Token t in input)
        {
            if (t.tokenType == TokenType.Number || t.tokenType==TokenType.Variable)
            {
                queue.Enqueue(t);
            }
            else if (t.tokenType == TokenType.Operator || t.tokenType == TokenType.BinaryFunction || t.tokenType == TokenType.UnaryFunction)
            {
                while (stack.Count>0 && Token.TryGetOpPriority(stack.Peek(), out int priorityStack))
                {
                    Token.TryGetOpPriority(t, out int priorityCurrent);
                    int PriorityDiff = priorityStack-priorityCurrent;
                    if (PriorityDiff>0 || (t.content!="^" && PriorityDiff == 0)){
                        queue.Enqueue(stack.Pop());       
                    }
                    else
                    {
                        break;
                    }
                }
                stack.Push(t);
            }
    
            else if (t.tokenType == TokenType.Bracket)
            {
                if (t.content == "(")
                {
                    stack.Push(t);
                }
                if (t.content == ")")
                {
                    while (stack.Count>0)
                    {
                        Token top = stack.Pop();
                        if (top.content == "(")
                        {
                            if (stack.Peek().tokenType==TokenType.UnaryFunction || stack.Peek().tokenType == TokenType.BinaryFunction)
                            {
                                queue.Enqueue(stack.Pop());
                            }
                            break;
                        }
                        else
                        {
                            queue.Enqueue(top);
                        }
                    }
                }
            }
            else if (t.tokenType == TokenType.Comma)
            {
                while (stack.Peek().content != "(")
                {
                    queue.Enqueue(stack.Pop());
                }
            }
        }

        while(stack.Count != 0)
        {
            queue.Enqueue(stack.Pop());
        }
        return queue.ToArray();
        

    }
    
    //Regular calculation based on RPN, no lazy evaluation, no variable support
    static float PlainEval(Token[] input)
    {
        Stack<float> stack = new();
        foreach (Token t in input)
        {
            if (t.tokenType == TokenType.Number)
            {
                stack.Push(float.Parse(t.content));
            }
            if (t.tokenType==TokenType.Operator || t.tokenType == TokenType.BinaryFunction)
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
                    case "max":
                        result = Math.Max(t1, t2);
                        break;
                    case "min":
                        result = Math.Min(t1, t2);
                        break;
                    case "lze":
                        result = t1>t2?1f:0f;
                        break;
                    default:
                        break;
                }
                stack.Push(result);
            }
            else if (t.tokenType==TokenType.UnaryFunction)
            {
                double t1 = stack.Pop();
                float result = 0;
                switch (t.content)
                {
                    case "sin":
                        result = (float)Math.Sin(t1);
                        break;
                    case "cos":
                        result = (float)Math.Cos(t1);
                        break;
                    case "tan":
                        result = (float)Math.Tan(t1);
                        break;
                    case "sqrt":
                        result = (float)Math.Sqrt(t1);
                        break;
                    case "abs":
                        result = (float)Math.Abs(t1);
                        break;
                    default:
                        break; 
                }
                stack.Push(result);
            }
        }
        return stack.Pop();
    }
   
    static void ExecCommand(string input, string help)
    {
        switch (input)
        {
            case "exit":
                Console.WriteLine("Finished, terminating");
                Environment.Exit(0);
                break;
            case "help":
                Console.WriteLine(help);
                break;
            case "render":
                Console.WriteLine("AST rendering " + (render ? "OFF" : "ON"));
                render = !render;
                break;
            case "flush":
                Memory.variables.Clear();
                Console.WriteLine("Cleared variables");
                break;
            case "lazy":
                Console.WriteLine("Lazy evaluation and variable support " + (lazy ? "OFF" : "ON"));
                lazy = !lazy;
                break;
        }
    }

    static void LazyExec(string input)  // Operate when in lazy(full) mode
    {
        if (input.Contains('='))   //Variable assigment and reasignment
        {
            string[] exp = input.Split('=');
            string varName = exp[0].Trim();
            input = exp[1];

            Token[] tokenList = Tokenize(input);
            tokenList = SortAlgorithm(tokenList);
            // Console.WriteLine(Token.PrintList(tokenList));
            Node ASTRoot = AST.ASTBuilder(tokenList);
            Node ASTRootResolved = AST.ASTResolver(ASTRoot); // Lazy evaluate here
            if (!Memory.variables.TryGetValue(varName, out Node? _))
            {
                Memory.variables.Add(varName, ASTRootResolved);
            }
            else
            {
                Memory.variables[varName] = ASTRootResolved;
            }
            if (render)
            {
                AST.ASTRender(ASTRoot);
            }
        }
        else if (input.Split(' ').Length == 1 && input.All(ch => char.IsAsciiLetter(ch)))   //Single variable evaluation
        {
            string varName = input.Trim();
            if (Memory.variables.TryGetValue(varName, out Node? root))
            {
                if (render) { AST.ASTRender(root); }
                Console.WriteLine("Result: " + AST.ASTEval(root));
            }
            else
            {
                Console.WriteLine("No such variable");
            }
        }
        else
        {
            Token[] tokenList = Tokenize(input);
            tokenList = SortAlgorithm(tokenList);
            // Console.WriteLine(Token.PrintList(tokenList));
            Node ASTRoot = AST.ASTBuilder(tokenList);
            Console.WriteLine(AST.ASTEval(ASTRoot));
            if (render) { AST.ASTRender(ASTRoot); }
        }
    }
    static void PlainExec(string input) // Operate when in non-lazy mode (single expression mode)
    {
        if (input.Contains('-'))
        {
            throw new ArgumentException("No variables in non-lazy mode");
        }
        Token[] tokenList = Tokenize(input);
        tokenList = SortAlgorithm(tokenList);
        // Console.WriteLine(Token.PrintList(tokenList));
        Node ASTRoot = AST.ASTBuilder(tokenList);
        Console.WriteLine(AST.ASTEval(ASTRoot));
        if (render) { AST.ASTRender(ASTRoot); }
    } 
    static void Main(string[] args)
    {
        string[] commands = ["help", "lazy", "flush", "render", "exit"];
        string help = @"This is a CLI interactive calculator with variable support.
Functions supported: +, -, *, /, ^
sin, cos, tan, sqrt, abs, max, min, lze(less than or equal).
Constants: pi, G, eul.

Commands (don't include tics): Ctrl+C or `exit` to exit; `render` to toggle AST rendering;
`help` to get this message;
`flush` to clear variable memory
`lazy` to toggle the evaluator - in lazy mode(default) there are variables, lazy evaluation, AST resolver. In non-lazy there is no memory, only constants may be used.

Variables are expressions, ranging from a = 5, to something like foo = abs(cos(b))/2. Always defined with `name` = `expession`.
Don't include tics
To eval the variable, enter it's name.
";
        Console.WriteLine(help);
        while (true)
        {
            Console.Write(">>> ");
            string? input = Console.ReadLine();
            if(input==null || input.Length <= 0)
            {
                continue;
            }
            if (commands.Contains(input))  // If input is one of the commands
            {
                ExecCommand(input, help);
            }
            else
            {
                if (lazy)
                {
                    LazyExec(input);
                }
                else
                {
                    PlainExec(input);
                }
            }
        }
    }
}
