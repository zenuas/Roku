
%{
using Roku.Node;
%}

%default INode
%define YYNAMESPACE Roku.Parser

%left  VAR STR NULL TRUE FALSE IF LET SUB IGNORE
%token<NumericNode> NUM
%left  EQ
%right UNARY
%left  '.'

%left  ','
%left  '(' '[' '{'
%left  EOL EOF

%%

start: {$$ = new ProgramNode();}
