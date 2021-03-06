using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MonadicParserCombinator;

namespace MonadicParserCombinator.Samples.Lisp.Old
{
    public abstract class LispExpression
    {
        public abstract void Accept(IVisitor visitor);
    }

    public class LispList : LispExpression
    {

        public IEnumerable<LispExpression> expressions;

        public LispList(IEnumerable<LispExpression> expr)
        {
            this.expressions = expr;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class EmptyLispList : LispExpression
    {
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LispTerm : LispExpression
    {
        public int value;

        public LispTerm(int term)
        {
            this.value = term;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LispSymbol : LispExpression
    {
        public string symbol;

        public LispSymbol(string symbol)
        {
            this.symbol = symbol;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LispDefine : LispExpression
    {
        public LispSymbol parameter;
        public LispExpression expression;

        public LispDefine(LispSymbol par, LispExpression expr)
        {
            this.parameter = par;
            this.expression = expr;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LispLambda : LispExpression
    {
        public LispSymbol parameter;
        public LispExpression expression;

        public LispLambda(LispSymbol par, LispExpression expr)
        {
            this.parameter = par;
            this.expression = expr;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public static class LispParser
    {
        //static Parser<IEnumerable<char>> SpaceNewline =
        //    from s in Parser.EitherOf(Space, Char('\n')).Many()
        //    select s;

        static Parser<char> Space =
            from s in Parser.Char(' ')
            select s;

        static Parser<IEnumerable<char>> Spaces =
            from s in Space.ManyOne()
            select s;

        static Parser<char> LeftParen =
            from lp in Parser.Char('(')
            select lp;

        static Parser<char> RightParen =
            from rp in Parser.Char(')')
            select rp;

        static Parser<char> Newline =
            from nl in Parser.Char(')')
            select nl;

         public static Parser<char> NumberParser =
            from n in IntParser
            select n;

        public static Parser<char> IntParser =
            from x in Parser.Char('1')
            select x;

        public static Parser<LispExpression> TermParser =
            from x in Parser.Integer.Text()
            select (LispExpression)new LispTerm(int.Parse(x));

        static Parser<LispExpression> SymbolParser =
            from x in Parser.MatchRegex(new Regex(@"a-z*"))
            select (LispExpression)new LispSymbol(x);

        public static Parser<LispExpression> DefineParser =
            from lp in LeftParen
            from define in Parser.MatchString("define")
            from s1 in Space.ManyOne()
            from lp2 in LeftParen
            from s2 in Space.ManyOne()
            from n in SymbolParser
            from s3 in Space.ManyOne()
            from rp2 in RightParen
            from s4 in Space.ManyOne()
            from expr in ListParserInner
            from s5 in Space.ManyOne()
            from rp in RightParen
            from s6 in Space.ManyOne()
            select (LispExpression)new LispDefine((LispSymbol)n, expr);

        public static Parser<LispExpression> LambdaParser =
            from define in Parser.MatchString("lambda")
            from s1 in Spaces
            from n in SymbolParser
            from s2 in Spaces
            from expr in ListParserInner
            select (LispExpression)new LispLambda((LispSymbol)n, expr);

        static Parser<LispExpression> EmptyListParser =
            from lp in LeftParen
            from ss in Space.Many()
            from rp in RightParen
            select (LispExpression)new EmptyLispList();

        static Parser<LispExpression> ListParserInner =
            from lp in LeftParen
            from s1 in Space.Many()
            from expr in Parser.SeperatedBy<LispExpression, IEnumerable<char>>(
                            Parser.EitherOf(new List<Parser<LispExpression>>(
                                new Parser<LispExpression>[]{
                                    DefineParser, LambdaParser,
                                    EmptyListParser, ListParserInner,
                                    TermParser, SymbolParser})),
                                Space.ManyOne())
            from s2 in Space.Many()
            from rp in RightParen
            select (LispExpression)new LispList(expr);

        static Parser<LispExpression> ListParser =
            from lp in LeftParen
            from s1 in Space.Many()
            from expr in Parser.EitherOf(new List<Parser<LispExpression>>(
                            new Parser<LispExpression>[]{
                                DefineParser, LambdaParser,
                                EmptyListParser, ListParserInner,
                                TermParser, SymbolParser}))
            from s2 in Space.Many()
            from rp in RightParen
            select (LispExpression)new LispList(expr.ReturnIEnumerable());

        public static Parser<IEnumerable<LispExpression>> LispProgramParser =
                                                            Parser.SeperatedBy(
                                                                ListParserInner,
                                                                Space.ManyOne())
                                                            .EndOfInput();

        //public static Parser<LispExpression> FormParser =

        public static Parser<LispExpression> DefinitionParser =
            from v in VariableDefinitionParser
            select v;

        public static Parser<LispExpression> VariableDefinitionParser =
            from d in Parser.EitherOf(new List<Parser<LispExpression>>(
                            new Parser<LispExpression>[]{
                                DefineParser, LambdaParser,
                                EmptyListParser, ListParserInner,
                                TermParser, SymbolParser}))
            select d;

        public static Parser<LispExpression> BodyParser =
            from d in Parser.EitherOf(new List<Parser<LispExpression>>(
                            new Parser<LispExpression>[]{
                                DefineParser, LambdaParser,
                                EmptyListParser, ListParserInner,
                                TermParser, SymbolParser}))
            select d;
    }
}
