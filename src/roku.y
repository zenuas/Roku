
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
%type<ITypeNode>                type
%type<TypeNode>                 nsvar typev
%type<ITypeNode?>               typex
%type<IIfNode>                  if ifthen elseif
%type<StructNode>               struct struct_block
%type<VariableNode>             var varx fvar fn
%type<NumericNode>              num
%type<StringNode>               str

%left  VAR STR NULL TRUE FALSE IF LET SUB IGNORE
%token<NumericNode> NUM
%left  EQ
%left  OPE OR LT GT
%left<TokenNode> ope nope
%left<TokenNode> or
%left  OR2
%left  AND2
%left<TokenNode> and
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
     | struct    {Scopes.Peek().Structs.Add($1);}
     | sub       {Scopes.Peek().Functions.Add($1);}
     | if

block : begin stmt END {$$ = Scopes.Pop();}
begin : BEGIN          {Scopes.Push(new BlockNode().R($1));}

########## expr ##########
expr : var
     | str
     | num
     | call
     | '[' list ']'           {$$ = $2;}
     | expr '.' fvar          {$$ = CreatePropertyNode($1, $3).R($2);}
     | ope expr %prec UNARY   {$$ = CreateFunctionCallNode($1, $2);}
     | expr nope expr         {$$ = CreateFunctionCallNode($2, $1, $3);}
     | expr LT expr           {$$ = CreateFunctionCallNode($2, $1, $3);}
     | expr GT expr           {$$ = CreateFunctionCallNode($2, $1, $3);}
     | expr '[' expr ']'      {$$ = CreateFunctionCallNode(CreateVariableNode("[]", $2), $1, $3);}

call : expr '(' list ')' {$$ = CreateFunctionCallNode($1, $3.List.ToArray());}

list   : void            {$$ = CreateListNode<IEvaluableNode>();}
       | listn extra
listn  : expr            {$$ = CreateListNode($1);}
       | list2n
list2n : expr ',' expr   {$$ = CreateListNode($1, $3);}
       | list2n ',' expr {$$ = $1.Return(x => x.List.Add($3));}

########## let ##########
let : LET var EQ expr    {$$ = CreateLetNode($2, $4);}

########## struct ##########
struct : STRUCT var              EOL struct_block {$$ = $4.Return(x => x.Name = $2);}
#       | STRUCT var LT atvarn GT EOL struct_block {$$ = $7.Return(x => x.Name = $2.Name).Return(x => x.Generics.AddRange($4.List));}

struct_block : struct_begin define END {$$ = Scopes.Pop();}
struct_begin : BEGIN                   {Scopes.Push(new StructNode().R($1));}

define : void
       | define LET var ':' type EOL   {Scopes.Peek().Statements.Add(CreateLetNode($3, $5));}
       | define LET var EQ  expr EOL   {Scopes.Peek().Statements.Add(CreateLetNode($3, $5));}
#       | define sub

#atvarn : atvar                         {$$ = CreateListNode($1);}
#       | atvarn ',' atvar              {$$ = $1.Return(x => x.List.Add($3));}

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
decla  : var ':' type   {$$ = new DeclareNode($1, $3).R($1);}
type   : typev
       | '[' type ']'   {$$ = new TypeArrayNode($2).R($1);}
typev  : nsvar
nsvar  : varx           {$$ = new TypeNode { Name = $1.Name }.R($1);}
typex  : void
       | type

########## if ##########
if     : ifthen
       | elseif
       | ifthen ELSE EOL block                        {$$ = AddElse($1, $4);}
       | elseif ELSE EOL block                        {$$ = AddElse($1, $4);}
       | IF expr                 THEN NOTEOL expr EOL {$$ = CreateIfNode($2, ToStatementBlock($5));}
       | IF var ':' type EQ expr THEN NOTEOL expr EOL {$$ = CreateIfCastNode($2, $4, $6, ToStatementBlock($9));}
ifthen : IF expr EOL block                            {$$ = CreateIfNode($2, $4);}
       | IF var ':' type EQ expr EOL block            {$$ = CreateIfCastNode($2, $4, $6, $8);}
elseif : ifthen ELSE ifthen                           {$$ = $1.Return(x => x.ElseIf.Add($3));}
       | elseif ELSE ifthen                           {$$ = $1.Return(x => x.ElseIf.Add($3));}

########## other ##########
var    : VAR     {$$ = CreateVariableNode($1);}
varx   : var
       | SUB     {$$ = CreateVariableNode($1);}
       | LET     {$$ = CreateVariableNode($1);}
       | IF      {$$ = CreateVariableNode($1);}
       | THEN    {$$ = CreateVariableNode($1);}
       | ELSE    {$$ = CreateVariableNode($1);}
fvar   : varx
       | NUM     {$$ = CreateVariableNode($1.Format).R($1);}
num    : NUM     {$$ = $1;}
str    : STR     {$$ = new StringNode { Value = $1.Name }.R($1);}
       | str STR {$$ = $1.Return(x => x.Value += $2.Name);}
ope    : nope
       | and
       | or
       | LT      {$$ = new TokenNode { Token = $1 }.R($1);}
       | GT      {$$ = new TokenNode { Token = $1 }.R($1);}
nope   : OPE     {$$ = new TokenNode { Token = $1 }.R($1);}
       | OR      {$$ = new TokenNode { Token = $1 }.R($1);}
and    : AND2    {$$ = new TokenNode { Token = $1 }.R($1);}
or     : OR2     {$$ = new TokenNode { Token = $1 }.R($1);}

extra  : void
       | ','

NOTEOL : EOL     {SyntaxError($1, "not eol");}
       | void

void   : {$$ = null;}
