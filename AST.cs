public class Node
{
    public Token Content;
    public Node? Lchild{get;set;}
    public Node? Rchild{get;set;}
    public Node(Token content, Node? lchild = null, Node? rchild=null)
    {
        Content = content;
        Lchild = lchild;
        Rchild = rchild;
    }
    
}

public class AST
{
    public static Node ASTBuilder(Token[] tokens)
    {
        Stack<Node> stack = new();
        foreach(Token t in tokens)
        {
            if (t.tokenType == TokenType.Number || t.tokenType==TokenType.Variable) 
            {
                stack.Push(new Node(t));
            }
            else if(t.tokenType == TokenType.Operator || t.tokenType==TokenType.BinaryFunction)
            {
                Node rchild = stack.Pop();
                Node lchild = stack.Pop();
                stack.Push(new Node(t, lchild, rchild));
            }
            else if(t.tokenType == TokenType.UnaryFunction)
            {
                Node lchild = stack.Pop();
                stack.Push(new Node(t, lchild, null));
            }
        }
        return stack.Pop();
    }

    public static float ASTEval(Node root)
    {
        if (root.Content.tokenType == TokenType.Number)
        {
            return float.Parse(root.Content.content);
        }
        if (root.Content.tokenType == TokenType.Variable)
        {
            if (Memory.variables.TryGetValue(root.Content.content, out Node? value))
            {
                return ASTEval(value);
            }
            else
            {
                return float.NaN;
            }
        }
        else if(root.Content.tokenType == TokenType.Operator || root.Content.tokenType == TokenType.BinaryFunction)
        {

            float left = ASTEval(root.Lchild!);
            float right = ASTEval(root.Rchild!);
            switch (root.Content.content)
            {
                case "max":
                    return Math.Max(left, right);
                case "+":
                    return left+right;
                case "-":
                    return left-right;
                case "*":
                    return left*right;
                case "/":
                    return left/right;
                case "^":
                    double d1 = (double)left;
                    double d2 = (double)right;
                    return (float)Math.Pow(d2,d1);
                case "min":
                    return Math.Min(left, right);                
                case "lze":
                    return left>right?0f:1f;
                      
            }
        }
        else if(root.Content.tokenType == TokenType.UnaryFunction)
        {
            float left = ASTEval(root.Lchild!);
            double d1 = (double)left;
            switch (root.Content.content)
            {
                case "sin":
                    return (float)Math.Sin(d1);
                case "cos":
                    return (float)Math.Cos(d1);
                case "tan":
                    return (float)Math.Tan(d1);
                case "sqrt":
                    return (float)Math.Sqrt(d1);
                case "abs":
                    return (float)Math.Abs(d1);
                }
        }
        return -1;
    }
    public static void ASTRender(Node root)
    {
        Print(root, "", false);
    }
    private static void Print(Node node, string prefix, bool isLast)
    {
        if (node == null)
            return;

        Console.Write(prefix);
        Console.Write(isLast ? "└── " : "├── ");
        Console.WriteLine(node.Content.content);

        // Collect non-null children
        if (node.Lchild != null && node.Rchild != null)
        {
            Print(node.Lchild,
                  prefix + (isLast ? "    " : "│   "),
                  false);
            Print(node.Rchild,
                  prefix + (isLast ? "    " : "│   "),
                  true);
        }
        else if (node.Lchild != null)
        {
            Print(node.Lchild,
                  prefix + (isLast ? "    " : "│   "),
                  true);
        }
        else if (node.Rchild != null)
        {
            Print(node.Rchild,
                  prefix + (isLast ? "    " : "│   "),
                  true);
        }
    
    }
    public static Node ASTResolver(Node node)  // Simplifies variables here
    {
        
        if (node.Content.tokenType == TokenType.UnaryFunction)
        {
            if (ASTResolver(node.Lchild!).Content.tokenType == TokenType.Number)  // If the child is resolved, evaluate here
            {
                float resolvedN = ASTEval(node.Lchild!);
                double d = (double)resolvedN;
                float result = 0;
                switch (node.Content.content)
                {
                case "sin":
                    result = (float)Math.Sin(d);
                    break;
                case "cos":
                    result =  (float)Math.Cos(d);
                    break;
                case "tan":
                    result = (float)Math.Tan(d);
                    break;
                case "sqrt":
                    result = (float)Math.Sqrt(d);
                    break;
                case "abs":
                    result = (float)Math.Abs(d);
                    break;
                }
                return new Node(new Token(TokenType.Number, result.ToString()));
            }
            else  // The child wasn't resolved
            {
                node.Lchild = ASTResolver(node.Lchild!); // Try our best
                return node;
            }

        }
        else if (node.Content.tokenType == TokenType.BinaryFunction || node.Content.tokenType==TokenType.Operator)
        {
            if (ASTResolver(node.Lchild!).Content.tokenType == TokenType.Number && ASTResolver(node.Rchild!).Content.tokenType==TokenType.Number)  // If the child is resolved, evaluate here
            {
                float resolvedL = ASTEval(node.Lchild!);
                float resolvedR = ASTEval(node.Rchild!);
                float result = 0;
                switch (node.Content.content)
                {
                case "max":
                    result=  Math.Max(resolvedL, resolvedR);
                    break;
                case "+":
                    result = resolvedL+resolvedR;
                    break;
                case "-":
                    result = resolvedL-resolvedR;
                    break;
                case "*":
                    result = resolvedL*resolvedR;
                    break;
                case "/":
                    result = resolvedL/resolvedR;
                    break;
                case "^":
                    double d1 = (double)resolvedL;
                    double d2 = (double)resolvedR;
                    result = (float)Math.Pow(d2,d1);
                    break;
                case "min":
                    result = Math.Min(resolvedL, resolvedR);     
                    break;           
                case "lze":
                    result = resolvedL>resolvedR?0f:1f;
                    break;
                      
                }
                return new Node(new Token(TokenType.Number, result.ToString()));
            }
            else  // The children weren't resolved
            {
                node.Lchild = ASTResolver(node.Lchild!); // Try our best
                node.Rchild = ASTResolver(node.Rchild!); 
                return node;
            }

        }
        //if (node.Content.tokenType == TokenType.Number || node.Content.tokenType==TokenType.Variable): 
        return node;
    }
}
