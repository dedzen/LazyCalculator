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
public class Token
{
    public TokenType tokenType { get; set; }
    public string content { get; set; }
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
        foreach (Token t in input)
        {
            result += t.content + " ";
        }
        return result.Substring(0, result.Length - 1);
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
            else { return new Token(TokenType.Variable, input); }

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