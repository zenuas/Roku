
%{
using Extensions;
using Roku.Node;
%}

%default INode
%define YYNAMESPACE Roku.Parser
%define YYTOKEN     Token

%type<IScopeNode>               block stmt
%type<IStatementNode>           line
%type<LetNode>                  let
%type<FunctionNode>             sub sub_block
%type<IEvaluableNode>           expr
%type<DeclareNode>              decla
%type<ListNode<DeclareNode>>    args argn
%type<ListNode<IEvaluableNode>> list listn list2n
%type<TypeNode>                 nsvar type typev
%type<TypeNode?>                typex
%type<VariableNode>             var varx fn
%type<NumericNode>              num
%type<StringNode>               str

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
     | stmt line {$$ = $1.Return(x => { if ($2 is { }) x.Statements.Add($2); });}

line : call EOL
     | let  EOL
     | sub       {Scopes.Peek().Functions.Add($1);}


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

########## sub ##########
sub    : SUB fn where '(' args ')' typex EOL sub_block {$$ = CreateFunctionNode($9, $2, $5, $7, $3);}

sub_block : sub_begin stmt END {$$ = Scopes.Pop();}
sub_begin : BEGIN              {Scopes.Push(new FunctionNode { LineNumber = $1.LineNumber });}

fn     : var
where  : void
args   : void           {$$ = CreateListNode<DeclareNode>();}
       | argn extra
argn   : decla          {$$ = CreateListNode($1);}
       | argn ',' decla {$$ = $1.Return(x => x.List.Add($3));}
decla  : var ':' type   {$$ = new DeclareNode($1, $3);}
type   : typev
typev  : nsvar
nsvar  : varx           {$$ = new TypeNode { Name = $1.Name } .R($1);}
typex  : void
       | type

########## other ##########
var    : VAR     {$$ = CreateVariableNode($1);}
varx   : var
       | SUB     {$$ = CreateVariableNode($1);}
       | LET     {$$ = CreateVariableNode($1);}
num    : NUM     {$$ = $1;}
str    : STR     {$$ = new StringNode { Value = $1.Name }.R($1);}
       | str STR {$$ = $1.Return(x => x.Value += $2.Name);}

extra  : void
       | ','

void   : {$$ = null;}
