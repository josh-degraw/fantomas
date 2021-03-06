module Fantomas.Tests.PatternMatchingTests

open NUnit.Framework
open FsUnit
open Fantomas.Tests.TestHelper

[<Test>]
let ``match expressions`` () =
    formatSourceString false """
    let filter123 x =
        match x with
        | 1 | 2 | 3 -> printfn "Found 1, 2, or 3!"
        | a -> printfn "%d" a""" config
    |> prepend newline
    |> should equal """
let filter123 x =
    match x with
    | 1
    | 2
    | 3 -> printfn "Found 1, 2, or 3!"
    | a -> printfn "%d" a
"""

[<Test>]
let ``function keyword`` () =
    formatSourceString false """
    let filterNumbers =
        function | 1 | 2 | 3 -> printfn "Found 1, 2, or 3!"
                 | a -> printfn "%d" a""" config
    |> prepend newline
    |> should equal """
let filterNumbers =
    function
    | 1
    | 2
    | 3 -> printfn "Found 1, 2, or 3!"
    | a -> printfn "%d" a
"""

[<Test>]
let ``when clauses and as patterns`` () =
    formatSourceString false """
let rangeTest testValue mid size =
    match testValue with
    | var1 when var1 >= mid - size/2 && var1 <= mid + size/2 -> printfn "The test value is in range."
    | _ -> printfn "The test value is out of range."

let (var1, var2) as tuple1 = (1, 2)
printfn "%d %d %A" var1 var2 tuple1""" config
    |> prepend newline
    |> should equal """
let rangeTest testValue mid size =
    match testValue with
    | var1 when var1 >= mid - size / 2 && var1 <= mid + size / 2 -> printfn "The test value is in range."
    | _ -> printfn "The test value is out of range."

let (var1, var2) as tuple1 = (1, 2)
printfn "%d %d %A" var1 var2 tuple1
"""

[<Test>]
let ``and & or patterns`` () =
    formatSourceString false """
let detectZeroOR point =
    match point with
    | (0, 0) | (0, _) | (_, 0) -> printfn "Zero found."
    | _ -> printfn "Both nonzero."

let detectZeroAND point =
    match point with
    | (0, 0) -> printfn "Both values zero."
    | (var1, var2) & (0, _) -> printfn "First value is 0 in (%d, %d)" var1 var2
    | (var1, var2)  & (_, 0) -> printfn "Second value is 0 in (%d, %d)" var1 var2
    | _ -> printfn "Both nonzero."
"""  config
    |> prepend newline
    |> should equal """
let detectZeroOR point =
    match point with
    | (0, 0)
    | (0, _)
    | (_, 0) -> printfn "Zero found."
    | _ -> printfn "Both nonzero."

let detectZeroAND point =
    match point with
    | (0, 0) -> printfn "Both values zero."
    | (var1, var2) & (0, _) -> printfn "First value is 0 in (%d, %d)" var1 var2
    | (var1, var2) & (_, 0) -> printfn "Second value is 0 in (%d, %d)" var1 var2
    | _ -> printfn "Both nonzero."
"""

[<Test>]
let ``paren and tuple patterns`` () =
    formatSourceString false """
let countValues list value =
    let rec checkList list acc =
       match list with
       | (elem1 & head) :: tail when elem1 = value -> checkList tail (acc + 1)
       | head :: tail -> checkList tail acc
       | [] -> acc
    checkList list 0

let detectZeroTuple point =
    match point with
    | (0, 0) -> printfn "Both values zero."
    | (0, var2) -> printfn "First value is 0 in (0, %d)" var2
    | (var1, 0) -> printfn "Second value is 0 in (%d, 0)" var1
    | _ -> printfn "Both nonzero."
"""  config
    |> prepend newline
    |> should equal """
let countValues list value =
    let rec checkList list acc =
        match list with
        | (elem1 & head) :: tail when elem1 = value -> checkList tail (acc + 1)
        | head :: tail -> checkList tail acc
        | [] -> acc

    checkList list 0

let detectZeroTuple point =
    match point with
    | (0, 0) -> printfn "Both values zero."
    | (0, var2) -> printfn "First value is 0 in (0, %d)" var2
    | (var1, 0) -> printfn "Second value is 0 in (%d, 0)" var1
    | _ -> printfn "Both nonzero."
"""

[<Test>]
let ``type test and null patterns`` () =
    formatSourceString false """
let detect1 x =
    match x with
    | 1 -> printfn "Found a 1!"
    | (var1 : int) -> printfn "%d" var1

let RegisterControl(control:Control) =
    match control with
    | :? Button as button -> button.Text <- "Registered."
    | :? CheckBox as checkbox -> checkbox.Text <- "Registered."
    | _ -> ()

let ReadFromFile (reader : System.IO.StreamReader) =
    match reader.ReadLine() with
    | null -> printfn "\n"; false
    | line -> printfn "%s" line; true""" config
    |> prepend newline
    |> should equal """
let detect1 x =
    match x with
    | 1 -> printfn "Found a 1!"
    | (var1: int) -> printfn "%d" var1

let RegisterControl (control: Control) =
    match control with
    | :? Button as button -> button.Text <- "Registered."
    | :? CheckBox as checkbox -> checkbox.Text <- "Registered."
    | _ -> ()

let ReadFromFile (reader: System.IO.StreamReader) =
    match reader.ReadLine() with
    | null ->
        printfn "\n"
        false
    | line ->
        printfn "%s" line
        true
"""

[<Test>]
let ``record patterns`` () =
    formatSourceString false """
type MyRecord = { Name: string; ID: int }

let IsMatchByName record1 (name: string) =
    match record1 with
    | { MyRecord.Name = nameFound; ID = _; } when nameFound = name -> true
    | _ -> false """ config
    |> prepend newline
    |> should equal """
type MyRecord = { Name: string; ID: int }

let IsMatchByName record1 (name: string) =
    match record1 with
    | { MyRecord.Name = nameFound; ID = _ } when nameFound = name -> true
    | _ -> false
"""

[<Test>]
let ``desugared lambdas`` () =
    formatSourceString false """
try
    fst(find (fun (s, (s', ty): int * int) ->
                s' = s0 && can (type_match ty ty0) []) (!the_interface))
with
| Failure _ -> s0""" { config with MaxLineLength = 80 }
    |> prepend newline
    |> should equal """
try
    fst (
        find
            (fun (s, (s', ty): int * int) ->
                s' = s0 && can (type_match ty ty0) [])
            (!the_interface)
    )
with Failure _ -> s0
"""

[<Test>]
let ``another case of desugared lambdas`` () =
    formatSourceString false """
find (fun (Ident op) x y -> Combp(Combp(Varp(op,dpty),x),y)) "term after binary operator" inp
"""  config
    |> prepend newline
    |> should equal """
find (fun (Ident op) x y -> Combp(Combp(Varp(op, dpty), x), y)) "term after binary operator" inp
"""

[<Test>]
let ``yet another case of desugared lambdas`` () =
    formatSourceString false """
let UNIFY_ACCEPT_TAC mvs th (asl, w) =
    let insts = term_unify mvs (concl th) w
    ([], insts), [],
    let th' = INSTANTIATE insts th
    fun i [] -> INSTANTIATE i th'""" config
    |> prepend newline
    |> should equal """
let UNIFY_ACCEPT_TAC mvs th (asl, w) =
    let insts = term_unify mvs (concl th) w

    ([], insts),
    [],
    let th' = INSTANTIATE insts th
    fun i [] -> INSTANTIATE i th'
"""

[<Test>]
let ``desugared lambdas again`` () =
    formatSourceString false """
fun P -> T""" config
    |> prepend newline
    |> should equal """
fun P -> T
"""

[<Test>]
let ``should consume spaces before inserting comments`` () =
    formatSourceString false """
let f x =
  a || // other case
        match n with
        | 17 -> false
        | _ -> true""" config
    |> prepend newline
    |> should equal """
let f x =
    a
    || // other case
    match n with
    | 17 -> false
    | _ -> true
"""

[<Test>]
let ``should not remove parentheses in patterns`` () =
    formatSourceString false """
let x =
    match y with
    | Start(-1) -> true
    | _ -> false""" config
    |> prepend newline
    |> should equal """
let x =
    match y with
    | Start (-1) -> true
    | _ -> false
"""

[<Test>]
let ``should indent function keyword in function application`` () =
    formatSourceString false """
let v =
    List.tryPick (function 1 -> Some 1 | _ -> None) [1; 2; 3]""" config
    |> prepend newline
    |> should equal """
let v =
    List.tryPick
        (function
        | 1 -> Some 1
        | _ -> None)
        [ 1; 2; 3 ]
"""

[<Test>]
let ``should put brackets around tuples in type tests`` () =
    formatSourceString false """
match item.Item with
| :? FSharpToolTipText as titem -> ()
| :? (string * XmlDoc) as tip -> ()
| _ -> ()""" config
    |> prepend newline
    |> should equal """
match item.Item with
| :? FSharpToolTipText as titem -> ()
| :? (string * XmlDoc) as tip -> ()
| _ -> ()
"""

[<Test>]
let ``should put brackets around app type tests`` () =
    formatSourceString false """
match item.Item with
| :? (Instruction seq) -> ()""" config
    |> prepend newline
    |> should equal """
match item.Item with
| :? (Instruction seq) -> ()
"""

[<Test>]
let ``should put brackets around array type tests`` () =
    formatSourceString false """
match item.Item with
| :? (Instruction []) -> ()""" config
    |> prepend newline
    |> should equal """
match item.Item with
| :? (Instruction []) -> ()
"""

[<Test>]
let ``should support rational powers on units of measures`` () =
    formatSourceString false """
[<Measure>] type X = cm^(1/2)/W""" config
    |> prepend newline
    |> should equal """
[<Measure>]
type X = cm^(1/2) / W
"""

let ``should add each case on newline`` () =
    formatSourceString false """
let (|OneLine|MultiLine|) b =
    match b with
    | Red
    | Green
    | Blue ->
        OneLinerBinding b

    | _ -> MultilineBinding b
"""  config
    |> prepend newline
    |> should equal """
let (|OneLine|MultiLine|) b =
    match b with
    | Red
    | Green
    | Blue -> OneLinerBinding b
    | _ -> MultilineBinding b
"""

[<Test>]
let ``each pattern should be on newline`` () =
    formatSourceString false """
let (|OneLinerBinding|MultilineBinding|) b =
    match b with
    | LetBinding([], PreXmlDoc [||], _, _, _, _, OneLinerExpr _)
    | DoBinding([], PreXmlDoc [||], OneLinerExpr _)
    | MemberBinding([], PreXmlDoc [||], _, _, _, _, OneLinerExpr _)
    | PropertyBinding([], PreXmlDoc [||], _, _, _, _, OneLinerExpr _)
    | ExplicitCtor([], PreXmlDoc [||], _, _, OneLinerExpr _, _) ->
        OneLinerBinding b

    | _ -> MultilineBinding b
"""  config
    |> prepend newline
    |> should equal """
let (|OneLinerBinding|MultilineBinding|) b =
    match b with
    | LetBinding ([], PreXmlDoc [||], _, _, _, _, OneLinerExpr _)
    | DoBinding ([], PreXmlDoc [||], OneLinerExpr _)
    | MemberBinding ([], PreXmlDoc [||], _, _, _, _, OneLinerExpr _)
    | PropertyBinding ([], PreXmlDoc [||], _, _, _, _, OneLinerExpr _)
    | ExplicitCtor ([], PreXmlDoc [||], _, _, OneLinerExpr _, _) -> OneLinerBinding b

    | _ -> MultilineBinding b
"""

[<Test>]
let ``should split constructor and function call correctly, double formatting`` () =
    let config80 = { config with MaxLineLength = 80 }

    let original = """
let update msg model =
    let res =
        match msg with
        | AMessage -> { model with AFieldWithAVeryVeryVeryLooooooongName = 10 }.RecalculateTotal()
        | AnotherMessage -> model
    res
"""

    let afterFirstFormat =
        formatSourceString false original config80

    formatSourceString false afterFirstFormat config80
    |> prepend newline
    |> should equal """
let update msg model =
    let res =
        match msg with
        | AMessage ->
            { model with
                  AFieldWithAVeryVeryVeryLooooooongName = 10 }
                .RecalculateTotal()
        | AnotherMessage -> model

    res
"""

[<Test>]
let ``updated record with function call remains be on same line, because short enough`` () =
    formatSourceString false """
let x =  { Value = 36 }.Times(9)

match b with
| _ -> { Value = 42 }.Times(8)
"""  config
    |> prepend newline
    |> should equal """
let x = { Value = 36 }.Times(9)

match b with
| _ -> { Value = 42 }.Times(8)
"""

[<Test>]
let ``with clause drop-through, 465`` () =
    formatSourceString false """
let internal ImageLoadResilient (f: unit -> 'a) (tidy: unit -> 'a) =
    try
      f()
    with
    | :? BadImageFormatException
    | :? ArgumentException
    | :? IOException -> tidy()
"""  config
    |> prepend newline
    |> should equal """
let internal ImageLoadResilient (f: unit -> 'a) (tidy: unit -> 'a) =
    try
        f ()
    with
    | :? BadImageFormatException
    | :? ArgumentException
    | :? IOException -> tidy ()
"""

[<Test>]
let ``pattern match 2 space indent`` () =
    formatSourceString false """
match x with
| Some y ->
    let z = 1
    Some(y + z)
| None -> None
"""  { config with IndentSize = 2 }
    |> prepend newline
    |> should equal """
match x with
| Some y ->
    let z = 1
    Some(y + z)
| None -> None
"""

[<Test>]
let ``should preserve a new line between single and multi-pattern cases`` () =
    formatSourceString false """
let f x =
    match x with
    | A

    | B -> Some()

    | C -> None

    | _ -> None
"""  config
    |> prepend newline
    |> should equal """
let f x =
    match x with
    | A

    | B -> Some()

    | C -> None

    | _ -> None
"""

[<Test>]
let ``very long match clause with many lambdas`` () =
    formatSourceString false """
let MethInfoIsUnseen g m ty minfo =
    let isUnseenByObsoleteAttrib () =
        match BindMethInfoAttributes m minfo
                (fun ilAttribs -> Some foo)
                (fun fsAttribs -> Some bar)
                (fun provAttribs -> Some(CheckProvidedAttributesForUnseen provAttribs m))
                (fun _provAttribs -> None)
                    with
        | Some res -> res
        | None -> false

    ()
"""  config
    |> prepend newline
    |> should equal """
let MethInfoIsUnseen g m ty minfo =
    let isUnseenByObsoleteAttrib () =
        match BindMethInfoAttributes
                  m
                  minfo
                  (fun ilAttribs -> Some foo)
                  (fun fsAttribs -> Some bar)
                  (fun provAttribs -> Some(CheckProvidedAttributesForUnseen provAttribs m))
                  (fun _provAttribs -> None) with
        | Some res -> res
        | None -> false

    ()
"""

[<Test>]
let ``very long match clause with many lambdas mixed with defines, 976`` () =
    formatSourceString false """
let MethInfoIsUnseen g m ty minfo =
    let isUnseenByObsoleteAttrib () =
        match BindMethInfoAttributes m minfo
                (fun ilAttribs -> Some foo)
                (fun fsAttribs -> Some bar)
#if !NO_EXTENSIONTYPING
                (fun provAttribs -> Some(CheckProvidedAttributesForUnseen provAttribs m))
#else
                (fun _provAttribs -> None)
#endif
                    with
        | Some res -> res
        | None -> false

    ()
"""  config
    |> prepend newline
    |> should equal """
let MethInfoIsUnseen g m ty minfo =
    let isUnseenByObsoleteAttrib () =
        match BindMethInfoAttributes
                  m
                  minfo
                  (fun ilAttribs -> Some foo)
                  (fun fsAttribs -> Some bar)
#if !NO_EXTENSIONTYPING
                  (fun provAttribs -> Some(CheckProvidedAttributesForUnseen provAttribs m))
#else
                  (fun _provAttribs -> None)
#endif
              with
        | Some res -> res
        | None -> false

    ()
"""

[<Test>]
let ``trivia after arrow, 1010`` () =
    formatSourceString false """
let f () =
    match lol with
    | 1 -> // comment 1
        ()
    |> function
    | 3 -> ()
"""  config
    |> prepend newline
    |> should equal """
let f () =
    match lol with
    | 1 -> // comment 1
        ()
    |> function
    | 3 -> ()
"""

[<Test>]
let ``trivia after function keyword, 1010`` () =
    formatSourceString false """
let f () =
    match lol with
    | 1 -> // comment 1
        () // comment 2
    |> function // comment 3
    | 3 -> ()
"""  config
    |> prepend newline
    |> should equal """
let f () =
    match lol with
    | 1 -> // comment 1
        () // comment 2
    |> function // comment 3
    | 3 -> ()
"""

[<Test>]
let ``don't add additional newline before match`` () =
    formatSourceString false """
let private userNameDecoder (get : Decode.IGetters) =
    let givenName =
        get.Optional.Field "given_name" Decode.string

    let familyName =
        get.Optional.Field "family_name" Decode.string

    match givenName, familyName with
    | Some g, Some f -> sprintf "%s %c" g f.[0]
    | _ -> get.Required.Field "nickname" Decode.string
"""  { config with SpaceBeforeColon = true }
    |> prepend newline
    |> should equal """
let private userNameDecoder (get : Decode.IGetters) =
    let givenName =
        get.Optional.Field "given_name" Decode.string

    let familyName =
        get.Optional.Field "family_name" Decode.string

    match givenName, familyName with
    | Some g, Some f -> sprintf "%s %c" g f.[0]
    | _ -> get.Required.Field "nickname" Decode.string
"""

[<Test>]
let ``don't add newline before tuple return in clause`` () =
    formatSourceString false """
let private update onSubmit msg model =
    match msg with
    | UpdateName n -> ({ model with Name = n } : Model), Cmd.none
    | UpdatePrice p -> { model with Price = p }, Cmd.none
    | UpdateCurrency c -> { model with Currency = c }, Cmd.none
    | UpdateLocation (lat, lng) ->
        { model with
              Latitude = lat
              Longitude = lng },
        Cmd.none
    | UpdateIsDraft d -> { model with IsDraft = d }, Cmd.none
    | UpdateRemark r -> { model with Remark = r }, Cmd.none
    | UpdateLocationError isError ->
        let errors =
            if isError then
                model.Errors
                |> Map.add "distance" [ "De gekozen locatie is te ver van jouw locatie! Das de bedoeling niet veugel." ]
            else
                Map.remove "distance" model.Errors

        { model with Errors = errors }, Cmd.none
"""  { config with SpaceBeforeColon = true }
    |> prepend newline
    |> should equal """
let private update onSubmit msg model =
    match msg with
    | UpdateName n -> ({ model with Name = n } : Model), Cmd.none
    | UpdatePrice p -> { model with Price = p }, Cmd.none
    | UpdateCurrency c -> { model with Currency = c }, Cmd.none
    | UpdateLocation (lat, lng) ->
        { model with
              Latitude = lat
              Longitude = lng },
        Cmd.none
    | UpdateIsDraft d -> { model with IsDraft = d }, Cmd.none
    | UpdateRemark r -> { model with Remark = r }, Cmd.none
    | UpdateLocationError isError ->
        let errors =
            if isError then
                model.Errors
                |> Map.add "distance" [ "De gekozen locatie is te ver van jouw locatie! Das de bedoeling niet veugel." ]
            else
                Map.remove "distance" model.Errors

        { model with Errors = errors }, Cmd.none
"""

[<Test>]
let ``keep new line before function match, 1074`` () =
    formatSourceString false """
    let (|AndExpr|_|) =
        let chooser =
            function
            | (ExprPat e1, ExprPat e2) -> Some(e1, e2)
            | _ -> None

        function
        | ListSplitPick "&&" chooser (e1, e2) -> Some(BoolExpr.And(e1, e2))
        | _ -> None
"""  config
    |> prepend newline
    |> should equal """
let (|AndExpr|_|) =
    let chooser =
        function
        | (ExprPat e1, ExprPat e2) -> Some(e1, e2)
        | _ -> None

    function
    | ListSplitPick "&&" chooser (e1, e2) -> Some(BoolExpr.And(e1, e2))
    | _ -> None
"""

[<Test>]
let ``comment after arrow should not be duplicated, 1082`` () =
    formatSourceString false """
List.tryFind(fun { Type = t; Range = r }  ->
                    match t with
                    | MainNode SynMemberDefn_Member
                    | MainNode SynMemberSig_Member -> // trying to get AST trivia
                        RangeHelpers.``range contains`` r rangeOfBindingAndRhs

                    | Token(MEMBER, _) -> // trying to get token trivia
                        r.StartLine = rangeOfBindingAndRhs.StartLine

                    | _ -> false
                )
"""  config
    |> prepend newline
    |> should equal """
List.tryFind
    (fun { Type = t; Range = r } ->
        match t with
        | MainNode SynMemberDefn_Member
        | MainNode SynMemberSig_Member -> // trying to get AST trivia
            RangeHelpers.``range contains`` r rangeOfBindingAndRhs

        | Token (MEMBER, _) -> // trying to get token trivia
            r.StartLine = rangeOfBindingAndRhs.StartLine

        | _ -> false)
"""

[<Test>]
let ``trivia before pipe should not be repeated for each pipe, 1083`` () =
    formatSourceString false """
Seq.takeWhile
               (function
                         | Write ""
                         // for example:
                         // type Foo =
                         //     static member Bar () = ...
                         | IndentBy _
                         | WriteLine
                         | SetAtColumn _
                         | Write " -> "
                         | CommentOrDefineEvent _ -> true
                         | _ -> false)
"""  config
    |> prepend newline
    |> should equal """
Seq.takeWhile
    (function
    | Write ""
    // for example:
    // type Foo =
    //     static member Bar () = ...
    | IndentBy _
    | WriteLine
    | SetAtColumn _
    | Write " -> "
    | CommentOrDefineEvent _ -> true
    | _ -> false)
"""

[<Test>]
let ``or pattern in destructed record should stay in one line, 1252`` () =
    formatSourceString false """
let draftToken =
    match lastToken with
    | Some { Kind = GenericTypeParameter | StaticallyResolvedTypeParameter as kind } when isIdentifier token ->
            DraftToken.Create kind { token with LeftColumn = token.LeftColumn - 1
                                                FullMatchedLength = token.FullMatchedLength + 1 }
    | Some ( { Kind = SymbolKind.ActivePattern } as ap) when token.Tag = FSharpTokenTag.RPAREN ->
            DraftToken.Create SymbolKind.Ident ap.Token
    | _ ->
        let kind =
            if isOperator token then Operator
            elif isIdentifier token then Ident
            elif isKeyword token then Keyword
            elif isPunctuation token then Dot
            else Other
        DraftToken.Create kind token
"""  config
    |> prepend newline
    |> should equal """
let draftToken =
    match lastToken with
    | Some { Kind = GenericTypeParameter | StaticallyResolvedTypeParameter as kind } when isIdentifier token ->
        DraftToken.Create
            kind
            { token with
                  LeftColumn = token.LeftColumn - 1
                  FullMatchedLength = token.FullMatchedLength + 1 }
    | Some ({ Kind = SymbolKind.ActivePattern } as ap) when token.Tag = FSharpTokenTag.RPAREN ->
        DraftToken.Create SymbolKind.Ident ap.Token
    | _ ->
        let kind =
            if isOperator token then Operator
            elif isIdentifier token then Ident
            elif isKeyword token then Keyword
            elif isPunctuation token then Dot
            else Other

        DraftToken.Create kind token
"""

[<Test>]
let ``named pat or in clauses`` () =
    formatSourceString false """
let (|MFMember|MFStaticMember|MFConstructor|MFOverride|) (mf: MemberFlags) =
    match mf.MemberKind with
    | MemberKind.ClassConstructor
    | MemberKind.Constructor -> MFConstructor()
    | MemberKind.Member
    | MemberKind.PropertyGet
    | MemberKind.PropertySet
    | MemberKind.PropertyGetSet as mk ->
        if mf.IsInstance && mf.IsOverrideOrExplicitImpl
        then MFOverride mk
        elif mf.IsInstance
        then MFMember mk
        else MFStaticMember mk
"""  config
    |> prepend newline
    |> should equal """
let (|MFMember|MFStaticMember|MFConstructor|MFOverride|) (mf: MemberFlags) =
    match mf.MemberKind with
    | MemberKind.ClassConstructor
    | MemberKind.Constructor -> MFConstructor()
    | MemberKind.Member
    | MemberKind.PropertyGet
    | MemberKind.PropertySet
    | MemberKind.PropertyGetSet as mk ->
        if mf.IsInstance && mf.IsOverrideOrExplicitImpl
        then MFOverride mk
        elif mf.IsInstance
        then MFMember mk
        else MFStaticMember mk
"""

[<Test>]
let ``named pat or in function syntax`` () =
    formatSourceString false """
let rec (|DoExprAttributesL|_|) =
    function
    | DoExpr _
    | Attributes _ as x :: DoExprAttributesL (xs, ys) -> Some(x :: xs, ys)
    | DoExpr _
    | Attributes _ as x :: ys -> Some([ x ], ys)
    | _ -> None
"""  config
    |> prepend newline
    |> should equal """
let rec (|DoExprAttributesL|_|) =
    function
    | DoExpr _
    | Attributes _ as x :: DoExprAttributesL (xs, ys) -> Some(x :: xs, ys)
    | DoExpr _
    | Attributes _ as x :: ys -> Some([ x ], ys)
    | _ -> None
"""
