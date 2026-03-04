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

class Program
{
    // Formatting
    private const string bold = "\x1b[1m";
    private const string ubold = "\x1b[0m";
    private const string line = "\x1b[4m";
    private const string uline = "\x1b[24m";
    static void PrintHelp()
    {
        Console.WriteLine("This is a CLI interactive calculator with variable support.");
        Console.WriteLine($"{bold}Functions{ubold} supported: +, -, *, /, ^\nsin, cos, tan, sqrt, abs, max, min, lze(less than or equal).\n{bold}Constants{ubold}: pi, G, eul.\n");
        Console.WriteLine($"{bold}Commands{ubold} (don't include tics):\nCtrl+C or `exit` to exit; `render` to toggle AST rendering\n`help` to get this message\n`flush` to clear variable memory\n{bold}`examples` to see examples {"\x1b[38;5;93m"}(this is important){ubold}\n{bold}`lazy` to toggle the evaluator{ubold} - {line}in lazy mode(default) there are variables, lazy evaluation, AST resolver. In non-lazy there is no memory, only constants may be used.{uline}");
        Console.WriteLine($"{bold}Variables are expressions{ubold}, ranging from a = 5, to something like foo = abs(cos(b))/2. Always defined with `name` = `expession`.\nDon't include tics\n{line}To eval the variable, enter it's name.{uline}");
    }
    static void PrintExamples()
    {
        string[] basics = [
            "> 2+3*8              = 26",
            "> (5*(10-2))+6       = 46",
            "> max(2, sin(0))/2   = 1",
            "> sqrt(abs(0-100))+1 = 11",
            "> tan(pi/3)          = 1.73205",
            "> sqrt(3)            = 1.73205",
            "> (sqrt(4)^sqrt(9))  = 8"
        ];
        string[] variables = [
            "> a = 24-5",
            "> a",
            ": a is 19",
            "> a = 26*2^sqrt(4)-b",
            "> a",
            ": a is 104-b          <- lazy evaluation here",
            "> b = 3",
            "> a",
            ": a is 101",
            "> b = 104",
            "> a",
            ": a is 0               <- and here",
            "> x = x + 2",
            ": Exception: Circular defiinition",
            "> x = p^2",
            "> p = x-2",
            ": Exception: Circular defiinition"
        ];
        Console.WriteLine($"{bold}Basic evaluations, work in both modes: {ubold}");
        foreach(string s in basics)
        {
            Console.WriteLine(s);
        }
        Console.WriteLine($"{bold}Some examples of variables in lazy mode: {ubold}");
        foreach (string s in variables)
        {
            if (s.StartsWith(":"))
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(s);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(s);
            }
        }

    }
    static bool render = true;
    static bool lazy = true;

    static Token[] Tokenize(string input)
    {
        input += "|"; //I use this to terminate the string in parsing process

        ListX<Token> result = new ListX<Token>();
        bool parsingNumber = false;
        bool parsingLitelar = false;
        string tokenContent = "";

        char[] tokenStop = ['(', ')', ' ', '|', ','];  //literals and numbers stop on these chars
        char[] binaryOps = ['+', '-', '*', '/', '^']; //allowed infix binary operators
        char[] allowedSpecials = tokenStop.Concat(binaryOps).ToArray(); //all allowed special characters
        foreach (char c in input)
        {
            if (char.IsAsciiDigit(c) || c == '.')
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
                else if (c == '(' || c == ')')
                {
                    result.Add(new Token(TokenType.Bracket, c.ToString()));
                }
                else if (c == ',')
                {
                    result.Add(new Token(TokenType.Comma, c.ToString()));
                }
            }
        }
        return result.ToArray();
    }
    static Token[] SortAlgorithm(Token[] input)
    {
        StackX<Token> stack = new();
        QueueX<Token> queue = new();

        foreach (Token t in input)
        {
            if (t.tokenType == TokenType.Number || t.tokenType == TokenType.Variable)
            {
                queue.Enqueue(t);
            }
            else if (t.tokenType == TokenType.Operator || t.tokenType == TokenType.BinaryFunction || t.tokenType == TokenType.UnaryFunction)
            {
                while (stack.Count > 0 && Token.TryGetOpPriority(stack.Peek(), out int priorityStack))
                {
                    Token.TryGetOpPriority(t, out int priorityCurrent);
                    int PriorityDiff = priorityStack - priorityCurrent;
                    if (PriorityDiff > 0 || (t.content != "^" && PriorityDiff == 0))
                    {
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
                    while (stack.Count > 0)
                    {
                        Token top = stack.Pop();
                        if (top.content == "(")
                        {
                            if (stack.Count != 0){
                                if (stack.Peek().tokenType == TokenType.UnaryFunction || stack.Peek().tokenType == TokenType.BinaryFunction)
                                {
                                    queue.Enqueue(stack.Pop());
                                }
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

        while (stack.Count != 0)
        {
            queue.Enqueue(stack.Pop());
        }
        return queue.ToArray();


    }

    //Regular calculation based on RPN, no lazy evaluation, no variable support
    static float PlainEval(Token[] input)
    {
        StackX<float> stack = new();
        foreach (Token t in input)
        {
            if (t.tokenType == TokenType.Number)
            {
                stack.Push(float.Parse(t.content));
            }
            if (t.tokenType == TokenType.Operator || t.tokenType == TokenType.BinaryFunction)
            {
                float t1 = stack.Pop();
                float t2 = stack.Pop();
                float result = 0;
                switch (t.content)
                {
                    case "+":
                        result = t2 + t1;
                        break;
                    case "-":
                        result = t2 - t1;
                        break;
                    case "*":
                        result = t2 * t1;
                        break;
                    case "/":
                        result = t2 / t1;
                        break;
                    case "^":
                        double d1 = (double)t1;
                        double d2 = (double)t2;
                        result = (float)Math.Pow(d2, d1);
                        break;
                    case "max":
                        result = Math.Max(t1, t2);
                        break;
                    case "min":
                        result = Math.Min(t1, t2);
                        break;
                    case "lze":
                        result = t1 > t2 ? 1f : 0f;
                        break;
                    default:
                        break;
                }
                stack.Push(result);
            }
            else if (t.tokenType == TokenType.UnaryFunction)
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

    static void CommandExec(string input)
    {
        switch (input)
        {
            case "exit":
                Console.WriteLine("Finished, terminating");
                Environment.Exit(0);
                break;
            case "help":
                PrintHelp();
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
            case "examples":
                PrintExamples();
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
            Node ASTRoot = AST.ASTBuilder(tokenList, varName);
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
                Console.WriteLine("Result: " + AST.ASTEval(root, varName));
            }
            else
            {
                Console.WriteLine("No such variable");
            }
        }
        else
        {
            Token[] tokenList = Tokenize(input);
            // Console.WriteLine(Token.PrintList(tokenList));
            tokenList = SortAlgorithm(tokenList);
            Node ASTRoot = AST.ASTBuilder(tokenList);
            Console.WriteLine("Result: " + AST.ASTEval(ASTRoot));
            if (render) { AST.ASTRender(ASTRoot); }
        }
    }
    static void PlainExec(string input) // Operate when in non-lazy mode (single expression mode)
    {
        if (input.Contains('='))
        {
            throw new ArgumentException("No variables in non-lazy mode");
        }
        Token[] tokenList = Tokenize(input);
        tokenList = SortAlgorithm(tokenList);
        // Console.WriteLine(Token.PrintList(tokenList));
        Console.WriteLine("Result: " + PlainEval(tokenList));
        if (render)
        {
            Node ASTRoot = AST.ASTBuilder(tokenList);
            AST.ASTRender(ASTRoot);
        }
    }
    static void Main(string[] args)
    {
        string[] commands = ["help", "lazy", "flush", "render", "exit", "examples"];
        PrintHelp();
        while (true)
        {
            Console.Write(">>> ");
            string? input = Console.ReadLine();
            if (input == null || input.Length <= 0)
            {
                continue;
            }
            if (commands.Contains(input))  // If input is one of the commands
            {
                CommandExec(input);
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
