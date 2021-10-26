
%{
using Extensions;
using Roku.Node;
%}

%default INode
%define YYNAMESPACE Roku.Parser
%define YYTOKEN     Token

%type<IScopeNode>                   block stmt
%type<IStatementNode>               line let
%type<ITupleBind>                   tuplevar
%type<ListNode<ITupleBind>>         tuplevar2n
%type<FunctionNode>                 sub sub_block cond
%type<ListNode<FunctionNode>>       condn class_block
%type<LambdaExpressionNode>         lambda_func
%type<IEvaluableNode>               expr num
%type<DeclareNode>                  decla
%type<IDeclareNode>                 lambda_arg
%type<ListNode<DeclareNode>>        args argn
%type<ListNode<IDeclareNode>>       lambda_args lambda_argn
%type<ListNode<IEvaluableNode>>     list listn list2n
%type<ITypeNode>                    type gen
%type<SpecializationNode>           spec
%type<ListNode<SpecializationNode>> where wheren
%type<ListNode<ITypeNode>>          types typen type2n genn typeor
%type<TypeNode>                     nsvar
%type<ITypeNode>                    typev
%type<ITypeNode?>                   typex
%type<IIfNode>                      if ifthen elseif
%type<StructNode>                   struct struct_block
%type<ClassNode>                    class
%type<VariableNode>                 var varx fvar fn
%type<StringNode>                   str
%type<BooleanNode>                  bool

%left  VAR STR NULL TRUE FALSE IF LET SUB IGNORE ARROW IS
%token<NumericNode> NUM
%token<FloatingNumericNode> FLOAT
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
%right TYPE_PARAM

%%

start :                        {$$ = new ProgramNode();}
      | program_begin stmt END {$$ = Scopes.Pop();}
program_begin : BEGIN          {Scopes.Push(new ProgramNode());}

stmt : void      {$$ = Scopes.Peek();}
     | stmt line {$$ = $1.Return(x => { if ($2 is { }) x.Statements.Add($2); });}

line : call EOL
     | let  EOL
     | class     {Scopes.Peek().Classes.Add($1);}
     | struct    {Scopes.Peek().Structs.Add($1);}
     | sub       {Scopes.Peek().Functions.Add($1);}
     | if

block : begin stmt END {$$ = Scopes.Pop();}
begin : BEGIN          {Scopes.Push(new BlockNode().R($1));}

########## expr ##########
expr : var
     | str
     | num
     | bool
     | null
     | call
     | lambda
     | '[' list ']'                               {$$ = $2;}
     | '(' expr ')'                               {$$ = $2;}
     | '(' list2n ')'                             {$$ = CreateTupleNode($2).R($1);}
     | expr '.' fvar                              {$$ = CreatePropertyNode($1, $3).R($2);}
     | ope expr %prec UNARY                       {$$ = CreateFunctionCallNode($1, $2);}
     | expr nope expr                             {$$ = CreateFunctionCallNode($2, $1, $3);}
     | expr LT expr                               {$$ = CreateFunctionCallNode($2, $1, $3);}
     | expr GT expr                               {$$ = CreateFunctionCallNode($2, $1, $3);}
     | expr LT expr GT           %prec TYPE_PARAM {$$ = CreateSpecialization($1, $3);}
     | expr LT expr ',' typen GT %prec TYPE_PARAM {$$ = CreateSpecialization($1, $3, $5.List.ToArray());}
     | expr '[' expr ']'                          {$$ = CreateFunctionCallNode(CreatePropertyNode($1, CreateVariableNode("[]")), $3);}
     | expr IS type                               {$$ = CreateFunctionCallNode($2, $1, $3);}

call : expr '(' list ')' {$$ = CreateFunctionCallNode($1, $3.List.ToArray());}

list   : void            {$$ = CreateListNode<IEvaluableNode>();}
       | listn extra
listn  : expr            {$$ = CreateListNode($1);}
       | list2n
list2n : expr ',' expr   {$$ = CreateListNode($1, $3);}
       | list2n ',' expr {$$ = $1.Return(x => x.List.Add($3));}

########## let ##########
let : LET var EQ expr        {$$ = CreateLetNode($2, $4);}
    | expr '.' fvar EQ expr  {$$ = CreateLetNode($1, $3, $5);}
    | LET tuplevar2n EQ expr {$$ = CreateLetTupleNode($2, $4);}

tuplevar2n : tuplevar   ',' tuplevar {$$ = CreateListNode($1, $3);}
           | tuplevar2n ',' tuplevar {$$ = $1.Return(x => x.List.Add($3));}
tuplevar   : var                     {$$ = CreateLetNode($1);}
           | IGNORE                  {$$ = CreateLetIgnoreNode($1);}

########## struct ##########
struct : STRUCT var            EOL struct_block {$$ = $4.Return(x => x.Name = $2);}
       | STRUCT var LT genn GT EOL struct_block {$$ = $7.Return(x => x.Name = $2).Return(x => x.Generics.AddRange($4.List));}

struct_block : struct_begin define END {$$ = Scopes.Pop();}
struct_begin : BEGIN                   {Scopes.Push(new StructNode().R($1));}

define : void
       | define LET varx ':' type EOL  {Scopes.Peek().Statements.Add(CreateLetNode($3, $5));}
       | define LET varx EQ  expr EOL  {Scopes.Peek().Statements.Add(CreateLetNode($3, $5));}
#       | define sub

gen    : var                           {$$ = new TypeNode { Name = $1.Name }.R($1);}
genn   : gen                           {$$ = CreateListNode($1);}
       | genn ',' gen                  {$$ = $1.Return(x => x.List.Add($3));}

########## class ##########
class : CLASS var LT genn GT EOL class_block {$$ = CreateClassNode($2, $4, $7);}

class_block : BEGIN condn END {$$ = $2;}

cond  : SUB fn where '(' args        ')' typex EOL {$$ = CreateFunctionNode($2, $5, $7, $3);}
      | SUB fn where '(' typen extra ')' typex EOL {$$ = CreateFunctionNode($2, $5, $8, $3);}
condn : cond                                       {$$ = CreateListNode($1);}
      | condn cond                                 {$$ = $1.Return(x => x.List.Add($2));}

########## sub ##########
sub    : SUB fn where '(' args ')' typex EOL sub_block {$$ = CreateFunctionNode($9, $2, $5, $7, $3);}

sub_block : sub_begin stmt END {$$ = Scopes.Pop();}
sub_begin : BEGIN              {Scopes.Push(new FunctionNode { LineNumber = $1.LineNumber });}

fn     : var
where  : void                    {$$ = CreateListNode<SpecializationNode>();}
       | LT wheren GT            {$$ = $2;}
wheren : spec                    {$$ = CreateListNode($1);}
       | wheren ',' spec         {$$ = $1.Return(x => x.List.Add($3));}
spec   : varx LT genn extra GT   {$$ = CreateSpecialization($1, $3);}

args   : void                    {$$ = CreateListNode<DeclareNode>();}
       | argn extra
argn   : decla                   {$$ = CreateListNode($1);}
       | argn ',' decla          {$$ = $1.Return(x => x.List.Add($3));}
decla  : var ':' type            {$$ = new DeclareNode($1, $3).R($1);}
type   : typev
       | typev '?'               {$$ = CreateNullable($1);}
typev  : nsvar
       | nsvar LT typen extra GT {$$ = CreateSpecialization($1, $3);}
       | STRUCT var '(' args ')' {$$ = CreateTypeStructNode($2, $4);}
       | '[' type   ']'          {$$ = new TypeArrayNode($2).R($1);}
       | '[' type2n ']'          {$$ = new TypeTupleNode($2).R($1);}
       | '[' typeor ']'          {$$ = new EnumNode($2).R($1);}
       | '{' types  '}'          {$$ = CreateTypeFunctionNode($2);}
       | '{' types ARROW type'}' {$$ = CreateTypeFunctionNode($2, $4);}
nsvar  : varx                    {$$ = new TypeNode { Name = $1.Name }.R($1);}
       | nsvar '.' varx          {$$ = $1.Return(x => { x.Namespace.Add(x.Name); x.Name = $3.Name; });}
typex  : void
       | type
types  : void                    {$$ = CreateListNode<ITypeNode>();}
       | typen extra
typen  : type                    {$$ = CreateListNode($1);}
       | typen ',' type          {$$ = $1.Return(x => x.List.Add($3));}
type2n : type ',' typen          {$$ = $3.Return(x => x.List.Insert(0, $1));}
typeor : type OR type            {$$ = CreateListNode($1, $3);}
       | typeor OR type          {$$ = $1.Return(x => x.List.Add($3));}

########## lambda ##########
lambda       : var           ARROW lambda_func {$$ = CreateLambdaFunction($3, CreateListNode<IDeclareNode>(new ImplicitDeclareNode($1)), null, true);}
             | '(' ')' typex ARROW lambda_func {$$ = CreateLambdaFunction($5, CreateListNode<IDeclareNode>(), $3, false);}
             |               ARROW lambda_func {$$ = CreateLambdaFunction($2, CreateListNode<IDeclareNode>(), null, true);}
#            | lambda_arg                ARROW lambda_func {$$ = CreateLambdaFunction($3, CreateListNode($1), null, true);}
#            | '(' lambda_args ')' typex ARROW lambda_func {$$ = CreateLambdaFunction($6, $3, $4, false);}
lambda_func  : expr                       {$$ = ToLambdaExpression($1);}
             | EOL lambda_begin stmt END  {$$ = $3;}
lambda_begin : BEGIN                      {Scopes.Push(new LambdaExpressionNode().R($1));}
lambda_arg   : var                        {$$ = new ImplicitDeclareNode($1);}
             | decla
lambda_args  : void                       {$$ = CreateListNode<IDeclareNode>();}
             | lambda_argn extra
lambda_argn  : lambda_arg                 {$$ = CreateListNode($1);}
             | lambda_argn ',' lambda_arg {$$ = $1.Return(x => x.List.Add($3));}

########## if ##########
if     : ifthen
       | elseif
       | ifthen ELSE EOL block                        {$$ = AddElse($1, $4);}
       | elseif ELSE EOL block                        {$$ = AddElse($1, $4);}
       | IF expr                 THEN NOTEOL expr EOL {$$ = CreateIfNode($2, ToStatementBlock($5));}
       | IF var ':' type EQ expr THEN NOTEOL expr EOL {$$ = CreateIfCastNode($2, $4, $6, ToStatementBlock($9));}
       | IF '[' list ']' EQ expr THEN NOTEOL expr EOL {$$ = CreateIfArrayCastNode(ToArrayPattern($3), $6, ToStatementBlock($9));}
ifthen : IF expr EOL block                            {$$ = CreateIfNode($2, $4);}
       | IF var ':' type EQ expr EOL block            {$$ = CreateIfCastNode($2, $4, $6, $8);}
       | IF '[' list ']' EQ expr EOL block            {$$ = CreateIfArrayCastNode(ToArrayPattern($3), $6, $8);}
elseif : ifthen ELSE ifthen                           {$$ = $1.Return(x => x.ElseIf.Add($3));}
       | elseif ELSE ifthen                           {$$ = $1.Return(x => x.ElseIf.Add($3));}

########## switch ##########
switch     : SWITCH expr EOL case_block
case_block : BEGIN casen END
casen      : case
           | casen case
case       : case_expr ARROW expr EOL
           | case_expr ARROW EOL block
case_expr  : var ':' type
           | '[' array_pattern ']'
           | '(' tuple_pattern ')'
           | ELSE

array_pattern : void
              | patternn extra
tuple_pattern : pattern ',' patternn extra
patternn      : pattern
              | patternn ',' pattern
pattern       : var

########## other ##########
var    : VAR     {$$ = CreateVariableNode($1);}
varx   : var
       | LET     {$$ = CreateVariableNode($1);}
       | STRUCT  {$$ = CreateVariableNode($1);}
       | CLASS   {$$ = CreateVariableNode($1);}
       | SUB     {$$ = CreateVariableNode($1);}
       | IF      {$$ = CreateVariableNode($1);}
       | THEN    {$$ = CreateVariableNode($1);}
       | ELSE    {$$ = CreateVariableNode($1);}
       | SWITCH  {$$ = CreateVariableNode($1);}
       | TRUE    {$$ = CreateVariableNode($1);}
       | FALSE   {$$ = CreateVariableNode($1);}
       | NULL    {$$ = CreateVariableNode($1);}
       | IS      {$$ = CreateVariableNode($1);}
fvar   : varx
       | NUM     {$$ = CreateVariableNode($1.Format).R($1);}
num    : NUM     {$$ = $1;}
       | FLOAT   {$$ = $1;}
bool   : TRUE    {$$ = new BooleanNode { Value = true }.R($1);}
       | FALSE   {$$ = new BooleanNode { Value = false }.R($1);}
null   : NULL    {$$ = new NullNode().R($1);}
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
