# Rokuについて

Rokuはコンパイラである。

## 文法

変数への代入はvarにて行う。  
変数への再代入はできない(未実装)。  
関数名の後にカッコで引数を並べることで関数呼び出しを行う。  
シャープ(#)以降はコメントである。  
インデントをそろえる必要がある。

```
# comment
var n = 100
print(n)
```

## 関数呼び出しの構文糖衣

プロパティ形式 `式.関数名(引数)` の形で関数を呼び出した場合、`関数名(式, 引数)` として解釈される(一部未実装)。  
関数名は通常現在のスコープから探索されるがプロパティ形式で呼び出した場合は式の型のスコープを優先的に探索する。

## ブロックコメント

行頭にシャープ記号を3つ以上(###)で始めるとブロックコメントになる。  
次に同数のシャープが出るまでをコメントにする。  
インデントの数は揃っていないといけない。

```
print(1)
###
block
comment
###
print(2)
```

## 整数

数字を書くと整数として扱われる。  
数字の間にアンダーバー(_)を挟んでよく、実行時には無視される。  
先頭に0xで始めると16進数、0oで始めると8進数、0bで始めると2進数として扱われる。  
整数を書くと型は`[Int | Long | Short | Byte]`型となる。  
利用する側に応じて自動的に一つが選ばれる。  
特に定まらない場合はInt型が選ばれる。

## 小数

ピリオド(.)を含んだ少数を書くと小数として扱われる(未実装)。  
数字の間にアンダーバー(_)を挟んでよく、実行時には無視される。  
小数を書くと型は`[Double | Float]`型となる。  
利用する側に応じて自動的に一つが選ばれる。  
特に定まらない場合はDouble型が選ばれる。

## 式

演算子は全て中置式の左結合である。  
同じ式内に異なる演算子を使うことはできない(未実装)。  
右辺の値が必要ない場合は左辺をアンダーバー(_)にする。

```
_ = 1 + 2 + 3 + 4     # OK
_ = 1 + 2 * 3 - 4     # 構文エラー
_ = 1 + (2 * 3) - 4   # 構文エラー
_ = 1 + ((2 * 3) - 4) # OK
```

## 配列

配列は `[a, b, c]` で定義される。  
配列の要素は全て同じ型である必要がある(一部未実装)。

## 関数

関数定義はsubにて行う。  
仮引数は変数名、型名の順番で記述する。  
仮引数への再代入はできない(未実装)。  
関数名は英小文字で始める必要がある(未実装)。  
関数の戻り値はreturn関数で指定し、return関数の実行後は関数から脱出する。  
インデントを一段下げる必要がある。

```
sub add(a: Int, b: Int) Int
    return(a + b)

f(1, 2)
```

## 匿名関数

匿名関数を `=>`、または`(引数) =>`で定義する(未実装)。  
匿名関数が1行だけの場合は戻り値が自動判断される。  

```
var xs = [1, 2, 3].map(x => x * x)
xs.each((y) => {
        var z = y + 1
        print(z)
    })
```

## 関数型

関数型は `{引数型}`、`{引数型 => 戻り値型}` で定義される(未実装)。

```
sub map(xs: [a], f: {a => b})
    ...

var xs = [1, 2, 3].map(x => x + 1)
```

## 構造体

構造体定義はstructにて行う。  
構造体は初期値を設定することができる。  
構造体名と同名の関数が用意され、インスタンス作成を行う。  
構造体名は英大文字で始める必要がある(未実装)。  
インスタンス作成を行ったスコープ内でしか構造体メンバの変更ができない(未実装)。  
インデントを一段下げる必要がある。

```
struct Foo
    var n = 10
    var s = "hello"

var a = Foo()
print(a.s)
```

## 簡易構造体

簡易構造体定義は関数の仮引数、戻り値、列挙体定義などの限られたケースでstructにて行う。  
同名、引数の名前、引数の型が全て同じ簡易構造体は同じ型として扱われる。  
構造体名と同名の関数が用意され、インスタンス作成を行う。  
構造体名は英大文字で始める必要がある。  
構造体メンバの変更ができない。

```
sub area(a: struct Rect(width: Int, height: Int)) Int
    return(a.width * a.height)

var s = area(Rect(10, 5))
print(s)
```

ただし、引数名だけが違う簡易構造体は作成できない。(未実装)

```
sub area(a: struct Rect(width: Int)) Int
    return(a.width * a.width)

sub area(a: struct Rect(width: Int, height: Int)) Int
    return(a.width * a.height)

# NG
sub area(a: struct Rect(height: Int, width: Int)) Int

var s = area(Rect(10))
print(s)

var t = area(Rect(10, 5))
print(t)
```

## 列挙体

列挙体定義はenumにて行う(未実装)。  
列挙体名と列挙名は英大文字で始める必要がある。  
インデントを一段下げる必要がある。

```
enum Color
    Red
    Green
    Blue
    struct RGB(r: Int, g: Int, b: Int)

sub show_color(c: Color)
    print(c)

var red = Color.Red
var yellow = Color.RGB(255, 255, 0)
show_color(red)
show_color(yellow)
```

## 簡易列挙体

簡易列挙体定義は`[ | ]`にて行う(未実装)。  
列挙名は英大文字で始める必要がある。

```
sub ask(yn: [Yes | No])
    print(c)

ask(Yes)
ask(No)
```

## 分岐

分岐はif文にて行う。  
インデントを一段下げる必要がある。

```
if a > 100
    print("over 100")
else if a > 50
    print("over 50")
else
    print("other")
```

## 型の分岐

型の分岐は `if 変数名: 型 = 式` にて行う(一部未実装)。

```
sub foo(a: [Int | String])
    if n: Int = a
        print(n)
    else if s: String = a
        print(s)
```

## 複数の分岐

複数の分岐はswitch文にて行う(未実装)。  
分岐は変数の値と型によって分岐できる。  
いずれの分岐にも当てはまらない場合はアンダーバー(_)が実行される。  
インデントを一段下げる必要がある。

```
switch x
    0         => print("== 0")
    1         => print("== 1")
    n: Int    => print("int")
    s: String => print("string")
    f: Foo    => print("foo")
    _         => print("else case")
```

## クラス

クラス定義はclassにて行う(未実装)。  
クラス名は英大文字で始める必要がある。  
構造体が制約を満たす場合、構造体は自動的にクラスのインスタンスとして扱われる。  
インデントを一段下げる必要がある。

```
class List<t, r>
    sub first(t) r
    sub next<List<x, r>>(t) x
    sub isnull(t) Bool

sub add<List<a, b>>(xs: a) b
    return(first(xs) + next(xs).first())

print(add([1, 2]))
```

## ジェネリック型

関数と構造体とクラスにはジェネリック型を使用できる(一部未実装)。  
ジェネリック型は英小文字で始まる必要がある。  
関数は明示的なジェネリック型指定が不要である。

```
sub add(a: num, b: num) num
    return(a + b)

struct Foo<t>
    var n = 10
    var s: t

class List<t, r>
    sub first(t) r
    sub next<List<x, r>>(t) x
    sub isnull(t) Bool
```

## タプル

2値以上のタプルを作ることができる。  
タプルのメンバへのアクセスは1から順に数値で指定する。  
タプルの型定義は `[型, 型...]` と2個以上の型からなる。  
同個数で同順番のタプルは同じ型となる。

```
var tuple = (10, "hello")
print(tuple.1)

sub create_tuple(a: Int, b: String) [Int, String]
    return((a, b))
```

## 列挙関数

列挙関数は戻り値が配列で `yield`、`yields` を使用した関数である(未実装)。  
`yield` を使用すると配列の要素を取り出して処理が中断される。  
呼び出し元が次の配列要素へアクセスすると関数が `yield` の次から再開される。  
`yields`は配列を扱う。

```
sub foo(n: Int) [Int]
    print("one")
    yield(n + 1)
    print("two")
    yield(n + 2)
    print("three")
    yield(n + 3)

foo(10).each(x => print(x))
###=>
one
11
two
12
three
13
###
```

## .NETとの互換

.NETで定義されたクラス、メソッドを呼び出すことができる(一部未実装)。  
ただし、完全な互換性はない。  
また、実装方法の違いにより一部機能は利用できない。

対応できない例)
  * 英大文字で始まるメソッドをネームスペース無指定で呼び出す
  * 英小文字で始まる.NETのクラスをネームスペース無指定で使用する
  * 継承、インターフェース
  * .NETのジェネリックを使用する
