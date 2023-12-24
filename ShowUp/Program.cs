using ExpRecognizer.EvaluationContext;
using ExpRecognizer.ExpressionTreeCreator;
using ExpRecognizer.Tokenizer;

namespace ShowUp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // brackets as function allows to make dynamic params function
            var context = Context.CreateDefault();
            var tokens = new Tokenizer().Tokenize("4 + max( 222, 15, 37) -2", context);
            var polish = new PolishEntryConverter().TryConvert(tokens, context);
            var result = polish;

            foreach (var item in result) //"2+2.2^2- asdf+sin((2 +3),a, b, 3)-456 * 7 + (6+7821 *8 / (3+4))^2"))
            {
                Console.WriteLine(item.Value + " " + item.Type.ToString());
            }
            Console.ReadLine();
        }
    }
}
