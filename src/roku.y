
%{
using Extensions;
using Roku.Node;
using IEvaluableListNode = Roku.Node.ListNode<Roku.Node.IEvaluableNode>;
%}

%default INode
%define YYNAMESPACE Roku.Parser
%define YYTOKEN     Token

%type<IScopeNode>         block stmt
%type<IStatementNode>     line
%type<LetNode>            let
%type<IEvaluableNode>     expr
%type<IEvaluableListNode> list listn list2n
%type<VariableNode>       var
%type<NumericNode>        num
%type<StringNode>         str

%left  VAR STR NULL TRUE FALSE IF LET SUB IGNORE
%token<NumericNode> NUM
%left  EQ
%right UNARY
%left  '.'

%left  ','
%left  '(' '[' '{'
%left  EOL

%%

start :                        {$$ = new ProgramNode();}
      | program_begin stmt END {$$ = Scopes.Pop();}
program_begin : BEGIN          {Scopes.Push(new ProgramNode());}

stmt : void      {$$ = Scopes.Peek();}
     | stmt line {$1.Statements.Add($2);}

line : call EOL
     | let  EOL


########## expr ##########
expr : var
     | str
     | num
     | call

call : expr '(' list ')' {$$ = CreateFunctionCallNode($1, $3.List.ToArray());}

list   : void
       | listn extra
listn  : expr            {$$ = CreateListNode($1);}
       | list2n
list2n : expr ',' expr   {$$ = CreateListNode($1, $3);}
       | list2n ',' expr {$$ = $1.Return(x => x.List.Add($3));}

########## let ##########
let : LET var EQ expr    {$$ = CreateLetNode($2, $4);}

########## other ##########
var    : VAR         {$$ = CreateVariableNode($1);}
num    : NUM         {$$ = $1;}
str    : STR         {$$ = new StringNode { Value = $1.Name }.R($1);}
       | str STR     {$$ = $1.Return(x => x.Value += $2.Name);}

void : {$$ = null;}
