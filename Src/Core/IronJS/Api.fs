﻿namespace IronJS.Api

open System
open IronJS
open IronJS.Aliases
open IronJS.Utils.Patterns

module Reflected =

  open System.Reflection
  open System.Collections.Concurrent

  let private apiTypes = ConcurrentDictionary<string, System.Type>()
  let private bindingFlags = BindingFlags.Static ||| BindingFlags.Public

  let private assembly = 
    AppDomain.CurrentDomain.GetAssemblies() 
      |> Array.find (fun x -> x.FullName.StartsWith("IronJS,"))

  let rec getApiMethodInfo type' method' =
    let found, typeObj = apiTypes.TryGetValue type'
    if found then typeObj.GetMethod(method', bindingFlags)
    else
      match assembly.GetType("IronJS.Api." + type', false) with
      | null -> null
      | typeObj ->
        apiTypes.TryAdd(type', typeObj) |> ignore
        getApiMethodInfo type' method'

module Extensions = 

  type Object with 

    member o.put (name, v:IjsBox) =
      o.Methods.PutBoxProperty.Invoke(o, name, v)

    member o.put (name, v:IjsBool) =
      let v = if v then TaggedBools.True else TaggedBools.False
      o.Methods.PutValProperty.Invoke(o, name, v)

    member o.put (name, v:IjsNum) =
      o.Methods.PutValProperty.Invoke(o, name, v)

    member o.put (name, v:HostObject) =
      o.Methods.PutRefProperty.Invoke(o, name, v, TypeCodes.Clr)

    member o.put (name, v:IjsStr) =
      o.Methods.PutRefProperty.Invoke(o, name, v, TypeCodes.String)

    member o.put (name, v:Undefined) =
      o.Methods.PutRefProperty.Invoke(o, name, v, TypeCodes.Undefined)

    member o.put (name, v:IjsObj) =
      o.Methods.PutRefProperty.Invoke(o, name, v, TypeCodes.Object)

    member o.put (name, v:IjsFunc) =
      o.Methods.PutRefProperty.Invoke(o, name, v, TypeCodes.Function)

    member o.put (name, v:IjsRef, tc:TypeCode) =
      o.Methods.PutRefProperty.Invoke(o, name, v, tc)

    member o.get (name) =
      o.Methods.GetProperty.Invoke(o, name)

    member o.has (name) =
      o.Methods.HasProperty.Invoke(o, name)

    member o.delete (name) =
      o.Methods.DeleteProperty.Invoke(o, name)
      
    member o.put (index, v:IjsBox) =
      o.Methods.PutBoxIndex.Invoke(o, index, v)

    member o.put (index, v:IjsBool) =
      let v = if v then TaggedBools.True else TaggedBools.False
      o.Methods.PutValIndex.Invoke(o, index, v)

    member o.put (index, v:IjsNum) =
      o.Methods.PutValIndex.Invoke(o, index, v)

    member o.put (index, v:HostObject) =
      o.Methods.PutRefIndex.Invoke(o, index, v, TypeCodes.Clr)

    member o.put (index, v:IjsStr) =
      o.Methods.PutRefIndex.Invoke(o, index, v, TypeCodes.String)

    member o.put (index, v:Undefined) =
      o.Methods.PutRefIndex.Invoke(o, index, v, TypeCodes.Undefined)

    member o.put (index, v:IjsObj) =
      o.Methods.PutRefIndex.Invoke(o, index, v, TypeCodes.Object)

    member o.put (index, v:IjsFunc) =
      o.Methods.PutRefIndex.Invoke(o, index, v, TypeCodes.Function)

    member o.put (index, v:IjsRef, tc:TypeCode) =
      o.Methods.PutRefIndex.Invoke(o, index, v, tc)

    member o.get (index) =
      o.Methods.GetIndex.Invoke(o, index)

    member o.has (index) =
      o.Methods.HasIndex.Invoke(o, index)

    member o.delete (index) =
      o.Methods.DeleteIndex.Invoke(o, index)

module Environment =

  open Extensions

  //----------------------------------------------------------------------------
  let hasCompiler (env:IjsEnv) funcId =
    env.Compilers.ContainsKey funcId

  //----------------------------------------------------------------------------
  let addCompilerId (env:IjsEnv) funId compiler =
    if hasCompiler env funId |> not then
      env.Compilers.Add(funId, CachedCompiler compiler)

  //----------------------------------------------------------------------------
  let addCompiler (env:IjsEnv) (f:IjsFunc) compiler =
    if hasCompiler env f.FunctionId |> not then
      f.Compiler <- CachedCompiler compiler
      env.Compilers.Add(f.FunctionId, f.Compiler)
    
  //----------------------------------------------------------------------------
  let createObject (env:IjsEnv) =
    let o = IjsObj(env.Base_Class, env.Object_prototype, Classes.Object, 0u)
    o.Methods <- env.Object_methods
    o

  //----------------------------------------------------------------------------
  let createObjectWithMap (env:IjsEnv) map =
    let o = IjsObj(map, env.Object_prototype, Classes.Object, 0u)
    o.Methods <- env.Object_methods
    o

  //----------------------------------------------------------------------------
  let createArray (env:IjsEnv) size =
    let o = IjsObj(env.Array_Class, env.Array_prototype, Classes.Array, size)
    o.Methods <- env.Object_methods
    o.Methods.PutValProperty.Invoke(o, "length", double size)
    o
    
  //----------------------------------------------------------------------------
  let createString (env:IjsEnv) (s:IjsStr) =
    let o = IjsObj(env.String_Class, env.String_prototype, Classes.String, 0u)
    o.Methods <- env.Object_methods
    o.Methods.PutValProperty.Invoke(o, "length", double s.Length)
    o.Value.Box.Clr <-s
    o.Value.Box.Type <- TypeCodes.String
    o
    
  //----------------------------------------------------------------------------
  let createNumber (env:IjsEnv) n =
    let o = IjsObj(env.Number_Class, env.Number_prototype, Classes.Number, 0u)
    o.Methods <- env.Object_methods
    o.Value.Box.Double <- n
    o
    
  //----------------------------------------------------------------------------
  let createBoolean (env:IjsEnv) b =
    let o = IjsObj(env.Boolean_Class, env.Boolean_prototype, Classes.Boolean, 0u)
    o.Methods <- env.Object_methods
    o.Value.Box.Bool <- b
    o.Value.Box.Type <- TypeCodes.Bool
    o
  
  //----------------------------------------------------------------------------
  let createPrototype (env:IjsEnv) =
    let o = IjsObj(env.Prototype_Class, env.Object_prototype, Classes.Object, 0u)
    o.Methods <- env.Object_methods
    o
  
  //----------------------------------------------------------------------------
  let createFunction env id (args:int) chain dc =
    let proto = createPrototype env
    let func = IjsFunc(env, id, chain, dc)

    (func :> IjsObj).Methods <- env.Object_methods
    func.ConstructorMode <- ConstructorModes.User

    proto.put("constructor", func)
    func.put("prototype", proto)
    func.put("length", double args)

    func
    
  //----------------------------------------------------------------------------
  module MethodInfo =
    let createObject = 
      Reflected.getApiMethodInfo "Environment" "createObject"

    let createObjectWithMap = 
      Reflected.getApiMethodInfo "Environment" "createObjectWithMap"

    let createArray = 
      Reflected.getApiMethodInfo "Environment" "createArray"

    let createString = 
      Reflected.getApiMethodInfo "Environment" "createString"

    let createNumber = 
      Reflected.getApiMethodInfo "Environment" "createNumber"

    let createBoolean = 
      Reflected.getApiMethodInfo "Environment" "createBoolean"

    let createPrototype = 
      Reflected.getApiMethodInfo "Environment" "createPrototype"

    let createFunction = 
      Reflected.getApiMethodInfo "Environment" "createFunction"

//------------------------------------------------------------------------------
// Static class containing all type conversions
//------------------------------------------------------------------------------
type TypeConverter =

  //----------------------------------------------------------------------------
  static member toBox(b:IjsBox) = b
  static member toBox(d:IjsNum) = Utils.boxDouble d
  static member toBox(b:IjsBool) = Utils.boxBool b
  static member toBox(s:IjsStr) = Utils.boxString s
  static member toBox(o:IjsObj) = Utils.boxObject o
  static member toBox(f:IjsFunc) = Utils.boxFunction f
  static member toBox(c:HostObject) = Utils.boxClr c
  static member toBox(expr:Dlr.Expr) = 
    Dlr.callStaticT<TypeConverter> "toBox" [expr]
    
  //----------------------------------------------------------------------------
  static member toHostObject(d:IjsNum) = box d
  static member toHostObject(b:IjsBool) = box b
  static member toHostObject(s:IjsStr) = box s
  static member toHostObject(o:IjsObj) = box o
  static member toHostObject(f:IjsFunc) = box f
  static member toHostObject(c:HostObject) = c
  static member toHostObject(b:IjsBox) =
    match b with
    | Number -> b.Double :> HostObject
    | Undefined -> null
    | String -> b.String :> HostObject
    | Boolean -> b.Bool :> HostObject
    | Host -> b.Clr
    | Object -> b.Object :> HostObject
    | Function -> b.Func :> HostObject

  static member toHostObject (expr:Dlr.Expr) =
    Dlr.callStaticT<TypeConverter> "toHostObject" [expr]

  //----------------------------------------------------------------------------
  static member toString (b:IjsBool) = if b then "true" else "false"
  static member toString (s:IjsStr) = s
  static member toString (u:Undefined) = "undefined"
  static member toString (b:IjsBox) =
    match b with
    | Undefined -> "undefined"
    | String -> b.String
    | Number -> TypeConverter.toString b.Double
    | Boolean -> TypeConverter.toString b.Bool
    | Host -> TypeConverter.toString b.Clr
    | Object -> TypeConverter.toString b.Object
    | Function -> TypeConverter.toString (b.Func :> IjsObj)

  static member toString (o:IjsObj) = 
    let mutable v = o.Methods.Default.Invoke(o, DefaultValue.String)
    TypeConverter.toString(v)

  static member toString (d:IjsNum) = 
    if System.Double.IsInfinity d then "Infinity" else d.ToString()

  static member toString (c:HostObject) = 
    if c = null then "null" else c.ToString()

  static member toString (expr:Dlr.Expr) =
    Dlr.callStaticT<TypeConverter> "toString" [expr]
      
  //----------------------------------------------------------------------------
  static member toPrimitive (b:IjsBool, _:byte) = Utils.boxBool b
  static member toPrimitive (d:IjsNum, _:byte) = Utils.boxDouble d
  static member toPrimitive (s:IjsStr, _:byte) = Utils.boxString s
  static member toPrimitive (u:Undefined, _:byte) = Utils.boxUndefined u
  static member toPrimitive (o:IjsObj, h:byte) = o.Methods.Default.Invoke(o, h)
  static member toPrimitive (o:IjsObj) = o.Methods.Default.Invoke(o, 0uy)
  static member toPrimitive (b:IjsBox, h:byte) =
    match b with
    | Number
    | Boolean
    | String
    | Undefined -> b
    | Host -> TypeConverter.toPrimitive(b.Clr, h)
    | Object
    | Function -> b.Object.Methods.Default.Invoke(b.Object, h)
  
  static member toPrimitive (c:HostObject, _:byte) = 
    Utils.boxClr (if c = null then null else c.ToString())

  static member toPrimitive (expr:Dlr.Expr) =
    Dlr.callStaticT<TypeConverter> "toPrimitive" [expr]
      
  //----------------------------------------------------------------------------
  static member toBoolean (b:IjsBool) = b
  static member toBoolean (d:IjsNum) = d > 0.0 || d < 0.0
  static member toBoolean (c:HostObject) = if c = null then false else true
  static member toBoolean (s:IjsStr) = s.Length > 0
  static member toBoolean (u:Undefined) = false
  static member toBoolean (o:IjsObj) = true
  static member toBoolean (b:IjsBox) =
    match b with 
    | Number -> TypeConverter.toBoolean b
    | Boolean -> b.Bool
    | Undefined -> false
    | String -> b.String.Length > 0
    | Host -> TypeConverter.toBoolean b.Clr
    | Object 
    | Function -> true
    
  static member toBoolean (expr:Dlr.Expr) =
    Dlr.callStaticT<TypeConverter> "toBoolean" [expr]

  //----------------------------------------------------------------------------
  static member toNumber (b:IjsBool) : double = if b then 1.0 else 0.0
  static member toNumber (d:IjsNum) = d
  static member toNumber (c:HostObject) = if c = null then 0.0 else 1.0
  static member toNumber (u:Undefined) = Number.NaN
  static member toNumber (b:IjsBox) =
    match b with
    | Number -> b.Double
    | Boolean -> if b.Bool then 1.0 else 0.0
    | String -> TypeConverter.toNumber(b.String)
    | Undefined -> NaN
    | Host -> TypeConverter.toNumber b.Clr
    | Object 
    | Function -> TypeConverter.toNumber(b.Object)

  static member toNumber (o:IjsObj) : Number = 
    TypeConverter.toNumber(o.Methods.Default.Invoke(o, DefaultValue.Number))

  static member toNumber (s:IjsStr) = 
    let mutable d = 0.0
    if Double.TryParse(s, anyNumber, invariantCulture, &d) 
      then d
      else NaN

  static member toNumber (expr:Dlr.Expr) = 
    Dlr.callStaticT<TypeConverter> "toNumber" [expr]
        
  //----------------------------------------------------------------------------
  static member toObject (env:IjsEnv, o:IjsObj) = o
  static member toObject (env:IjsEnv, b:Box) =
    match b with
    | Function
    | Object -> b.Object
    | Undefined
    | Host -> Errors.Generic.notImplemented()
    | Number -> Environment.createNumber env b.Double
    | String -> Environment.createString env b.String
    | Boolean -> Environment.createBoolean env b.Bool

  static member toObject (env:Dlr.Expr, expr:Dlr.Expr) =
    Dlr.callStaticT<TypeConverter> "toObject" [env; expr]
      
  //----------------------------------------------------------------------------
  static member toInt32 (d:IjsNum) = int d
  static member toUInt32 (d:IjsNum) = uint32 d
  static member toUInt16 (d:IjsNum) = uint16 d
  static member toInteger (d:IjsNum) : double = 
    if d = NaN
      then 0.0
      elif d = 0.0 || d = NegInf || d = PosInf
        then d
        else double (Math.Sign d) * Math.Floor(Math.Abs d)
                
  //-------------------------------------------------------------------------
  static member convertTo (env:Dlr.Expr) (expr:Dlr.Expr) (t:System.Type) =
    if Object.ReferenceEquals(expr.Type, t) then expr
    elif t.IsAssignableFrom(expr.Type) then Dlr.cast t expr
    else 
      if   t = typeof<IjsNum> then TypeConverter.toNumber expr
      elif t = typeof<IjsStr> then TypeConverter.toString expr
      elif t = typeof<IjsBool> then TypeConverter.toBoolean expr
      elif t = typeof<IjsBox> then TypeConverter.toBox expr
      elif t = typeof<IjsObj> then TypeConverter.toObject(env, expr)
      elif t = typeof<HostObject> then TypeConverter.toHostObject expr
      else Errors.Generic.noConversion expr.Type t

  static member convertToT<'a> env expr = 
    TypeConverter.convertTo env expr typeof<'a>
    
//------------------------------------------------------------------------------
// Operators
//------------------------------------------------------------------------------
and Operators =

  //----------------------------------------------------------------------------
  // Unary
  //----------------------------------------------------------------------------

  //----------------------------------------------------------------------------
  // typeof
  static member typeOf (o:IjsBox) = TypeCodes.Names.[o.Type]
  static member typeOf expr = Dlr.callStaticT<Operators> "typeOf" [expr]
  
  //----------------------------------------------------------------------------
  // !
  static member not (o) = Dlr.callStaticT<Operators> "not" [o]
  static member not (o:IjsBox) =
    not (TypeConverter.toBoolean o)
    
  //----------------------------------------------------------------------------
  // ~
  static member bitCmpl (o) = Dlr.callStaticT<Operators> "bitCmpl" [o]
  static member bitCmpl (o:IjsBox) =
    let o = TypeConverter.toNumber o
    let o = TypeConverter.toInt32 o
    Utils.boxDouble (double (~~~ o))
      
  //----------------------------------------------------------------------------
  // + (unary)
  static member plus (l, r) = Dlr.callStaticT<Operators> "plus" [l; r]
  static member plus (o:IjsBox) =
    Utils.boxDouble (TypeConverter.toNumber o)
    
  //----------------------------------------------------------------------------
  // - (unary)
  static member minus (l, r) = Dlr.callStaticT<Operators> "minus" [l; r]
  static member minus (o:IjsBox) =
    Utils.boxDouble ((TypeConverter.toNumber o) * -1.0)

  //----------------------------------------------------------------------------
  // Binary
  //----------------------------------------------------------------------------
    
  //----------------------------------------------------------------------------
  // <
  static member lt (l, r) = Dlr.callStaticT<Operators> "lt" [l; r]
  static member lt (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker
      then l.Double < r.Double
      elif l.Type = TypeCodes.String && r.Type = TypeCodes.String
        then l.String < r.String
        else TypeConverter.toNumber l < TypeConverter.toNumber r
        
  //----------------------------------------------------------------------------
  // <=
  static member ltEq (l, r) = Dlr.callStaticT<Operators> "ltEq" [l; r]
  static member ltEq (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker
      then l.Double <= r.Double
      elif l.Type = TypeCodes.String && r.Type = TypeCodes.String
        then l.String <= r.String
        else TypeConverter.toNumber l <= TypeConverter.toNumber r
        
  //----------------------------------------------------------------------------
  // >
  static member gt (l, r) = Dlr.callStaticT<Operators> "gt" [l; r]
  static member gt (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker
      then l.Double > r.Double
      elif l.Type = TypeCodes.String && r.Type = TypeCodes.String
        then l.String > r.String
        else TypeConverter.toNumber l > TypeConverter.toNumber r
        
  //----------------------------------------------------------------------------
  // >=
  static member gtEq (l, r) = Dlr.callStaticT<Operators> "gtEq" [l; r]
  static member gtEq (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker
      then l.Double >= r.Double
      elif l.Type = TypeCodes.String && r.Type = TypeCodes.String
        then l.String >= r.String
        else TypeConverter.toNumber l >= TypeConverter.toNumber r
        
  //----------------------------------------------------------------------------
  // ==
  static member eq (l, r) = Dlr.callStaticT<Operators> "eq" [l; r]
  static member eq (l:IjsBox, r:IjsBox) = 
    if Utils.Box.isNumber l.Marker && Utils.Box.isNumber r.Marker then
      l.Double = r.Double

    elif l.Type = r.Type then
      match l.Type with
      | TypeCodes.Undefined -> true
      | TypeCodes.String -> l.String = r.String
      | TypeCodes.Bool -> l.Bool = r.Bool
      | TypeCodes.Clr
      | TypeCodes.Function
      | TypeCodes.Object -> Object.ReferenceEquals(l.Clr, r.Clr)
      | _ -> false

    else
      if l.Type = TypeCodes.Clr 
        && l.Clr = null 
        && (r.Type = TypeCodes.Undefined 
            || r.Type = TypeCodes.Empty) then true
      
      elif r.Type = TypeCodes.Clr 
        && r.Clr = null 
        && (l.Type = TypeCodes.Undefined 
            || l.Type = TypeCodes.Empty) then true

      elif Utils.Box.isNumber l.Marker && r.Type = TypeCodes.String then
        l.Double = TypeConverter.toNumber r.String
        
      elif r.Type = TypeCodes.String && Utils.Box.isNumber r.Marker then
        TypeConverter.toNumber l.String = r.Double

      elif l.Type = TypeCodes.Bool then
        let mutable l = Utils.boxDouble(TypeConverter.toNumber l)
        Operators.eq(l, r)

      elif r.Type = TypeCodes.Bool then
        let mutable r = Utils.boxDouble(TypeConverter.toNumber r)
        Operators.eq(l, r)

      elif r.Type >= TypeCodes.Object then
        match l.Type with
        | TypeCodes.String -> 
          let mutable r = TypeConverter.toPrimitive(r.Object)
          Operators.eq(l, r)

        | _ -> 
          if Utils.Box.isNumber l.Marker then
            let mutable r = TypeConverter.toPrimitive(r.Object)
            Operators.eq(l, r)
          else
            false

      elif l.Type >= TypeCodes.Object then
        match r.Type with
        | TypeCodes.String -> 
          let mutable l = TypeConverter.toPrimitive(l.Object)
          Operators.eq(l, r)

        | _ -> 
          if Utils.Box.isNumber r.Marker then
            let mutable l = TypeConverter.toPrimitive(l.Object)
            Operators.eq(l, r)
          else
            false

      else
        false
        
  //----------------------------------------------------------------------------
  // !=
  static member notEq (l, r) = Dlr.callStaticT<Operators> "notEq" [l; r]
  static member notEq (l:IjsBox, r:IjsBox) = not (Operators.eq(l, r))
  
  //----------------------------------------------------------------------------
  // ===
  static member same (l, r) = Dlr.callStaticT<Operators> "same" [l; r]
  static member same (l:IjsBox, r:IjsBox) = 
    if Utils.Box.isBothNumber l.Marker r.Marker then
      l.Double = r.Double

    elif l.Tag = r.Tag then
      match l.Type with
      | TypeCodes.Undefined -> true
      | TypeCodes.String -> l.String = r.String
      | TypeCodes.Bool -> l.Bool = r.Bool
      | TypeCodes.Clr
      | TypeCodes.Function
      | TypeCodes.Object -> Object.ReferenceEquals(l.Clr, r.Clr)
      | _ -> false

    else
      false
      
  //----------------------------------------------------------------------------
  // !==
  static member notSame (l, r) = Dlr.callStaticT<Operators> "notSame" [l; r]
  static member notSame (l:IjsBox, r:IjsBox) =
    not (Operators.same(l, r))
    
  //----------------------------------------------------------------------------
  // +
  static member add (l, r) = Dlr.callStaticT<Operators> "add" [l; r]
  static member add (l:IjsBox, r:IjsBox) = 
    if Utils.Box.isBothNumber l.Marker r.Marker then
      Utils.boxDouble (l.Double + r.Double)

    elif l.Tag = TypeTags.String || r.Tag = TypeTags.String then
      Utils.boxString (TypeConverter.toString(l) + TypeConverter.toString(r))

    else
      Utils.boxDouble (TypeConverter.toNumber(l) + TypeConverter.toNumber(r))
      
  //----------------------------------------------------------------------------
  // -
  static member sub (l, r) = Dlr.callStaticT<Operators> "sub" [l; r]
  static member sub (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker then
      Utils.boxDouble (l.Double - r.Double)

    else
      Utils.boxDouble (TypeConverter.toNumber(l) - TypeConverter.toNumber(r))
      
  //----------------------------------------------------------------------------
  // /
  static member div (l, r) = Dlr.callStaticT<Operators> "div" [l; r]
  static member div (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker then
      Utils.boxDouble (l.Double / r.Double)

    else
      Utils.boxDouble (TypeConverter.toNumber(l) / TypeConverter.toNumber(r))
      
  //----------------------------------------------------------------------------
  // *
  static member mul (l, r) = Dlr.callStaticT<Operators> "mul" [l; r]
  static member mul (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker then
      Utils.boxDouble (l.Double * r.Double)

    else
      Utils.boxDouble (TypeConverter.toNumber(l) * TypeConverter.toNumber(r))
      
  //----------------------------------------------------------------------------
  // %
  static member mod' (l, r) = Dlr.callStaticT<Operators> "mod'" [l; r]
  static member mod' (l:IjsBox, r:IjsBox) =
    if Utils.Box.isBothNumber l.Marker r.Marker then
      Utils.boxDouble (l.Double % r.Double)

    else
      Utils.boxDouble (TypeConverter.toNumber l % TypeConverter.toNumber r)
    
  //----------------------------------------------------------------------------
  // &
  static member bitAnd (l, r) = Dlr.callStaticT<Operators> "bitAnd" [l; r]
  static member bitAnd (l:IjsBox, r:IjsBox) =
    let l = TypeConverter.toNumber l
    let r = TypeConverter.toNumber r
    let l = TypeConverter.toInt32 l
    let r = TypeConverter.toInt32 r
    Utils.boxDouble (double (l &&& r))
    
  //----------------------------------------------------------------------------
  // |
  static member bitOr (l, r) = Dlr.callStaticT<Operators> "bitOr" [l; r]
  static member bitOr (l:IjsBox, r:IjsBox) =
    let l = TypeConverter.toNumber l
    let r = TypeConverter.toNumber r
    let l = TypeConverter.toInt32 l
    let r = TypeConverter.toInt32 r
    Utils.boxDouble (double (l ||| r))
    
  //----------------------------------------------------------------------------
  // ^
  static member bitXOr (l, r) = Dlr.callStaticT<Operators> "bitXOr" [l; r]
  static member bitXOr (l:IjsBox, r:IjsBox) =
    let l = TypeConverter.toNumber l
    let r = TypeConverter.toNumber r
    let l = TypeConverter.toInt32 l
    let r = TypeConverter.toInt32 r
    Utils.boxDouble (double (l ^^^ r))
    
  //----------------------------------------------------------------------------
  // <<
  static member bitLhs (l, r) = Dlr.callStaticT<Operators> "bitLhs" [l; r]
  static member bitLhs (l:IjsBox, r:IjsBox) =
    let l = TypeConverter.toNumber l
    let r = TypeConverter.toNumber r
    let l = TypeConverter.toInt32 l
    let r = TypeConverter.toUInt32 r &&& 0x1Fu
    Utils.boxDouble (double (l <<< int r))
    
  //----------------------------------------------------------------------------
  // >>
  static member bitRhs (l, r) = Dlr.callStaticT<Operators> "bitRhs" [l; r]
  static member bitRhs (l:IjsBox, r:IjsBox) =
    let l = TypeConverter.toNumber l
    let r = TypeConverter.toNumber r
    let l = TypeConverter.toInt32 l
    let r = TypeConverter.toUInt32 r &&& 0x1Fu
    Utils.boxDouble (double (l >>> int r))
    
  //----------------------------------------------------------------------------
  // >>>
  static member bitURhs (l, r) = Dlr.callStaticT<Operators> "bitURhs" [l; r]
  static member bitURhs (l:IjsBox, r:IjsBox) =
    let l = TypeConverter.toNumber l
    let r = TypeConverter.toNumber r
    let l = TypeConverter.toUInt32 l
    let r = TypeConverter.toUInt32 r &&& 0x1Fu
    Utils.boxDouble (double (l >>> int r))
    
  //----------------------------------------------------------------------------
  // &&
  static member and' (l, r) = Dlr.callStaticT<Operators> "and'" [l; r]
  static member and' (l:IjsBox, r:IjsBox) =
    if not (TypeConverter.toBoolean l) then l else r
    
  //----------------------------------------------------------------------------
  // ||
  static member or' (l, r) = Dlr.callStaticT<Operators> "or'" [l; r]
  static member or' (l:IjsBox, r:IjsBox) =
    if TypeConverter.toBoolean l then l else r
      


//------------------------------------------------------------------------------
// PropertyClass API
//------------------------------------------------------------------------------
type PropertyClass =
        
  //----------------------------------------------------------------------------
  static member subClass (x:IronJS.PropertyMap, name) = 
    if x.isDynamic then 
      let index = 
        if x.FreeIndexes.Count > 0 then x.FreeIndexes.Pop()
        else x.NextIndex <- x.NextIndex + 1; x.NextIndex - 1

      x.PropertyMap.Add(name, index)
      x

    else
      let mutable subClass = null
      
      if not(x.SubClasses.TryGetValue(name, &subClass)) then
        let newMap = new MutableDict<string, int>(x.PropertyMap)
        newMap.Add(name, newMap.Count)
        subClass <- IronJS.PropertyMap(x.Env, newMap)
        x.SubClasses.Add(name, subClass)

      subClass

  //----------------------------------------------------------------------------
  static member subClass (x:IronJS.PropertyMap, names:string seq) =
    Seq.fold (fun c (n:string) -> PropertyClass.subClass(c, n)) x names
        
  //----------------------------------------------------------------------------
  static member makeDynamic (x:IronJS.PropertyMap) =
    if x.isDynamic then x
    else
      let pc = new IronJS.PropertyMap(null)
      pc.Id <- -1L
      pc.NextIndex <- x.NextIndex
      pc.FreeIndexes <- new MutableStack<int>()
      pc.PropertyMap <- new MutableDict<string, int>(x.PropertyMap)
      pc
        
  //----------------------------------------------------------------------------
  static member delete (x:IronJS.PropertyMap, name) =
    let pc = if not x.isDynamic then PropertyClass.makeDynamic x else x
    let mutable index = 0

    if pc.PropertyMap.TryGetValue(name, &index) then 
      pc.FreeIndexes.Push index
      pc.PropertyMap.Remove name |> ignore

    pc
      
  //----------------------------------------------------------------------------
  static member getIndex (x:IronJS.PropertyMap, name) =
    x.PropertyMap.[name]
    
//------------------------------------------------------------------------------
// Function API
//------------------------------------------------------------------------------
type Function() =

  static let getPrototype(f:IjsFunc) =
    let prototype = (f :> IjsObj).Methods.GetProperty.Invoke(f, "prototype")
    match prototype.Type with
    | TypeCodes.Function
    | TypeCodes.Object -> prototype.Object
    | _ -> f.Env.Object_prototype

  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  // GENERATED FUNCTION METHODS
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,IjsBox>>(f)
    c.Invoke(f,t)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,IjsBox>>(f)
    c.Invoke(f,t,a0)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0,a1:'a1) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,IjsBox>>(f)
    c.Invoke(f,t,a0,a1)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0,a1:'a1,a2:'a2) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,IjsBox>>(f)
    c.Invoke(f,t,a0,a1,a2)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0,a1:'a1,a2:'a2,a3:'a3) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,IjsBox>>(f)
    c.Invoke(f,t,a0,a1,a2,a3)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,IjsBox>>(f)
    c.Invoke(f,t,a0,a1,a2,a3,a4)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4,a5:'a5) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,'a5,IjsBox>>(f)
    c.Invoke(f,t,a0,a1,a2,a3,a4,a5)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4,a5:'a5,a6:'a6) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,'a5,'a6,IjsBox>>(f)
    c.Invoke(f,t,a0,a1,a2,a3,a4,a5,a6)
  
  //----------------------------------------------------------------------------
  static member call (f:IjsFunc,t,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4,a5:'a5,a6:'a6,a7:'a7) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,'a5,'a6,'a7,IjsBox>>(f)
    c.Invoke(f,t,a0,a1,a2,a3,a4,a5,a6,a7)

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0,a1:'a1) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0,a1)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0,a1) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0,a1:'a1,a2:'a2) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0,a1,a2)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0,a1,a2) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0,a1:'a1,a2:'a2,a3:'a3) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0,a1,a2,a3)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0,a1,a2,a3) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0,a1,a2,a3,a4)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0,a1,a2,a3,a4) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4,a5:'a5) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,'a5,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0,a1,a2,a3,a4,a5)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0,a1,a2,a3,a4,a5) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4,a5:'a5,a6:'a6) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,'a5,'a6,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0,a1,a2,a3,a4,a5,a6)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0,a1,a2,a3,a4,a5,a6) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"

  //----------------------------------------------------------------------------
  static member construct (f:IjsFunc,t:IjsObj,a0:'a0,a1:'a1,a2:'a2,a3:'a3,a4:'a4,a5:'a5,a6:'a6,a7:'a7) =
    let c = f.Compiler
    let c = c.compileAs<Func<IjsFunc,IjsObj,'a0,'a1,'a2,'a3,'a4,'a5,'a6,'a7,IjsBox>>(f)

    match f.ConstructorMode with
    | ConstructorModes.Host -> c.Invoke(f,null,a0,a1,a2,a3,a4,a5,a6,a7)
    | ConstructorModes.User -> 
      let o = Environment.createObject f.Env
      o.Prototype <- getPrototype f
      c.Invoke(f,o,a0,a1,a2,a3,a4,a5,a6,a7) |> ignore
      Utils.boxObject o

    | _ -> Errors.runtime "Can't call [[Construct]] on non-constructor"


  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  // GENERATED FUNCTION METHODS
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------
  //----------------------------------------------------------------------------

//------------------------------------------------------------------------------
// DispatchTarget
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// HostFunction API
//------------------------------------------------------------------------------
module HostFunction =

  open Extensions

  [<ReferenceEquality>]
  type internal DispatchTarget<'a when 'a :> Delegate> = {
    Delegate : HostType
    Function : IjsHostFunc<'a>
    Invoke: Dlr.Expr -> Dlr.Expr seq -> Dlr.Expr
  }

  //----------------------------------------------------------------------------
  let internal marshalArgs (passedArgs:Dlr.ExprParam array) (env:Dlr.Expr) i t =
    if i < passedArgs.Length 
      then TypeConverter.convertTo env passedArgs.[i] t
      else Dlr.default' t
      
  //----------------------------------------------------------------------------
  let internal marshalBoxParams 
    (f:IjsHostFunc<_>) (passed:Dlr.ExprParam array) (marshalled:Dlr.Expr seq) =
    passed
    |> Seq.skip f.ArgTypes.Length
    |> Seq.map Expr.boxValue
    |> fun x -> Seq.append marshalled [Dlr.newArrayItemsT<IjsBox> x]
    
  //----------------------------------------------------------------------------
  let internal marshalObjectParams 
    (f:IjsHostFunc<_>) (passed:Dlr.ExprParam array) (marshalled:Dlr.Expr seq) =
    passed
    |> Seq.skip f.ArgTypes.Length
    |> Seq.map TypeConverter.toHostObject
    |> fun x -> Seq.append marshalled [Dlr.newArrayItemsT<HostObject> x]
    
  //----------------------------------------------------------------------------
  let internal createParam i t = Dlr.param (sprintf "a%i" i) t
  
  //----------------------------------------------------------------------------
  let internal compileDispatcher (target:DispatchTarget<'a>) = 
    let f = target.Function

    let argTypes = FSKit.Reflection.getDelegateArgTypes target.Delegate
    let args = argTypes |> Array.mapi createParam
    let passedArgs = args |> Seq.skip f.MarshalMode |> Array.ofSeq

    let func = args.[0] :> Dlr.Expr
    let env = Dlr.field func "Env"

    let marshalled = f.ArgTypes |> Seq.mapi (marshalArgs passedArgs env)
    let marshalled = 
      let paramsExist = f.ArgTypes.Length < passedArgs.Length 

      match f.ParamsMode with
      | ParamsModes.BoxParams when paramsExist -> 
        marshalBoxParams f passedArgs marshalled

      | ParamsModes.ObjectParams when paramsExist -> 
        marshalObjectParams f passedArgs marshalled

      | _ -> marshalled

    let invoke = target.Invoke func marshalled
    let body = 
      if Utils.isBox f.ReturnType then invoke
      elif Utils.isVoid f.ReturnType then Expr.voidAsUndefined invoke
      else
        Dlr.blockTmpT<Box> (fun tmp ->
          [
            (Expr.setBoxTypeOf tmp invoke)
            (Expr.setBoxValue tmp invoke)
            (tmp :> Dlr.Expr)
          ] |> Seq.ofList
        )
            
    let lambda = Dlr.lambda target.Delegate args body
    Debug.printExpr lambda
    lambda.Compile()

  //----------------------------------------------------------------------------
  let internal generateInvoke<'a when 'a :> Delegate> f args =
    let casted = Dlr.castT<IjsHostFunc<'a>> f
    Dlr.invoke (Dlr.field casted "Delegate") args
  
  //----------------------------------------------------------------------------
  let compile<'a when 'a :> Delegate> (x:IjsFunc) (delegate':System.Type) =
    compileDispatcher {
      Delegate = delegate'
      Function = x :?> IjsHostFunc<'a>
      Invoke = generateInvoke<'a>
    }
    
  //----------------------------------------------------------------------------
  let create (env:IjsEnv) (delegate':'a) =
    let h = IjsHostFunc<'a>(env, delegate')
    let f = h :> IjsFunc
    let o = f :> IjsObj

    o.Methods <- env.Object_methods
    o.Methods.PutValProperty.Invoke(f, "length", double h.jsArgsLength)
    Environment.addCompiler env f compile<'a>

    f

//------------------------------------------------------------------------------
module Object =

  //----------------------------------------------------------------------------
  let defaultvalue (o:IjsObj) (hint:byte) =
    let hint = 
      if hint = DefaultValue.None 
        then DefaultValue.Number 
        else hint

    let valueOf = o.Methods.GetProperty.Invoke(o, "valueOf")
    let toString = o.Methods.GetProperty.Invoke(o, "toString")

    match hint with
    | DefaultValue.Number ->
      match valueOf.Type with
      | TypeCodes.Function ->
        let mutable v = Function.call(valueOf.Func, o)
        if Utils.isPrimitive v then v
        else
          match toString.Type with
          | TypeCodes.Function ->
            let mutable v = Function.call(toString.Func, o)
            if Utils.isPrimitive v then v else Errors.runtime "[[TypeError]]"
          | _ -> Errors.runtime "[[TypeError]]"
      | _ -> Errors.runtime "[[TypeError]]"

    | DefaultValue.String ->
      match toString.Type with
      | TypeCodes.Function ->
        let mutable v = Function.call(toString.Func, o)
        if Utils.isPrimitive v then v
        else 
          match toString.Type with
          | TypeCodes.Function ->
            let mutable v = Function.call(valueOf.Func, o)
            if Utils.isPrimitive v then v else Errors.runtime "[[TypeError]]"
          | _ -> Errors.runtime "[[TypeError]]"
      | _ -> Errors.runtime "[[TypeError]]"

    | _ -> Errors.runtime "Invalid hint"

  let defaultValue' = Default defaultvalue
    
  //----------------------------------------------------------------------------
  module Property = 
  
    //--------------------------------------------------------------------------
    let requiredStorage (o:IjsObj) =
      o.PropertyMap.PropertyMap.Count
      
    //--------------------------------------------------------------------------
    let isFull (o:IjsObj) =
      requiredStorage o >= o.PropertyDescriptors.Length
      
    //--------------------------------------------------------------------------
    let getIndex (o:IjsObj) (name:string) =
      o.PropertyMap.PropertyMap.TryGetValue name
      
    //--------------------------------------------------------------------------
    let setMap (o:IjsObj) pc =
      o.PropertyMap <- pc

    //--------------------------------------------------------------------------
    let makeDynamic (o:IjsObj) =
      if o.PropertyMapId >= 0L then
        o.PropertyMap <- PropertyClass.makeDynamic o.PropertyMap
      
    //--------------------------------------------------------------------------
    let expandStorage (o:IjsObj) =
      let values = o.PropertyDescriptors
      let required = requiredStorage o
      let newValues = Array.zeroCreate (required * 2)

      if values.Length > 0 then 
        Array.Copy(values, newValues, values.Length)
      
      o.PropertyDescriptors <- newValues
      
    //--------------------------------------------------------------------------
    let ensureIndex (o:IjsObj) (name:string) =
      match getIndex o name with
      | true, index -> index
      | _ -> 
        o.PropertyMap <- PropertyClass.subClass(o.PropertyMap, name)
        if isFull o then expandStorage o
        o.PropertyMap.PropertyMap.[name]
        
    //--------------------------------------------------------------------------
    let find (o:IjsObj) name =
      let rec find o name =
        if Utils.Utils.isNull o then (null, -1)
        else
          match getIndex o name with
          | true, index ->  o, index
          | _ -> find o.Prototype name

      find o name
      
    //--------------------------------------------------------------------------
    #if DEBUG
    let putBox (o:IjsObj) (name:IjsStr) (val':IjsBox) =
    #else
    let inline putBox (o:IjsObj) (name:IjsStr) (val':IjsBox) =
    #endif
      let index = ensureIndex o name
      o.PropertyDescriptors.[index].Box <- val'
      o.PropertyDescriptors.[index].HasValue <- true

    //--------------------------------------------------------------------------
    #if DEBUG
    let putRef (o:IjsObj) (name:IjsStr) (val':HostObject) (tc:TypeCode) =
    #else
    let inline putRef (o:IjsObj) (name:IjsStr) (val':HostObject) (tc:TypeCode) =
    #endif
      let index = ensureIndex o name
      o.PropertyDescriptors.[index].Box.Clr <- val'
      o.PropertyDescriptors.[index].Box.Type <- tc
      
    //--------------------------------------------------------------------------
    #if DEBUG
    let putVal (o:IjsObj) (name:IjsStr) (val':IjsNum) =
    #else
    let inline putVal (o:IjsObj) (name:IjsStr) (val':IjsNum) =
    #endif
      let index = ensureIndex o name
      o.PropertyDescriptors.[index].Box.Double <- val'
      o.PropertyDescriptors.[index].HasValue <- true

    //--------------------------------------------------------------------------
    #if DEBUG
    let get (o:IjsObj) (name:IjsStr) =
    #else
    let inline get (o:IjsObj) (name:IjsStr) =
    #endif
      match find o name with
      | _, -1 -> Utils.boxedUndefined
      | pair -> (fst pair).PropertyDescriptors.[snd pair].Box

    //--------------------------------------------------------------------------
    let has (o:IjsObj) (name:IjsStr) =
      find o name |> snd > -1

    //--------------------------------------------------------------------------
    let delete (o:IjsObj) (name:IjsStr) =
      match getIndex o name with
      | true, index -> 
        setMap o (PropertyClass.delete(o.PropertyMap, name))

        let attrs = o.PropertyDescriptors.[index].Attributes
        let canDelete = Utils.Descriptor.isDeletable attrs

        if canDelete then
          o.PropertyDescriptors.[index].HasValue <- false
          o.PropertyDescriptors.[index].Box.Clr <- null
          o.PropertyDescriptors.[index].Box.Double <- 0.0

        canDelete

      | _ -> true
      
    //--------------------------------------------------------------------------
    module Delegates =
      let putBox = PutBoxProperty putBox
      let putVal = PutValProperty putVal
      let putRef = PutRefProperty putRef
      let get = GetProperty get
      let has = HasProperty has
      let delete = DeleteProperty delete
    
  //----------------------------------------------------------------------------
  module Index =
  
    open Extensions
    open Utils.Patterns
  
    //--------------------------------------------------------------------------
    let initSparse (o:IjsObj) =
      o.IndexSparse <- new MutableSorted<uint32, Box>()

      for i = 0 to int (o.IndexLength-1u) do
        if Utils.Descriptor.hasValue o.IndexDense.[i] then
          o.IndexSparse.Add(uint32 i, o.IndexDense.[i].Box)

      o.IndexDense <- null
      
    //--------------------------------------------------------------------------
    let expandStorage (o:IjsObj) i =
      if o.IndexSparse = null || o.IndexDense.Length <= i then
        let size = if i >= 1073741823 then 2147483647 else ((i+1) * 2)
        let values = o.IndexDense
        let newValues = Array.zeroCreate size

        if values <> null && values.Length > 0 then
          Array.Copy(values, newValues, values.Length)

        o.IndexDense <- newValues
        
    //--------------------------------------------------------------------------
    let updateLength (o:IjsObj) (i:uint32) =
      if i > o.IndexLength then
        o.IndexLength <- i+1u
        if o.Class = Classes.Array then
          Property.putVal o "length" (double i)

    //--------------------------------------------------------------------------
    let find (o:IjsObj) (i:uint32) =
      let rec find o (i:uint32) =
        if Utils.Utils.isNull o then (null, 0u, false)
        else 
          if Utils.Object.isDense o then
            let ii = int i
            if ii < o.IndexDense.Length then
              if Utils.Descriptor.hasValue o.IndexDense.[ii] 
                then (o, i, true)
                else find o.Prototype i
            else find o.Prototype i
          else
            if o.IndexSparse.ContainsKey i 
              then (o, i, false)
              else find o.Prototype i

      find o i
      
    //--------------------------------------------------------------------------
    let hasIndex (o:IjsObj) (i:uint32) =
      if Utils.Object.isDense o then
        let ii = int i
        if ii < o.IndexDense.Length 
          then Utils.Descriptor.hasValue o.IndexDense.[ii]
          else false
      else
        o.IndexSparse.ContainsKey i

    //--------------------------------------------------------------------------
    #if DEBUG
    let putBox (o:IjsObj) (i:uint32) (v:IjsBox) =
    #else
    let inline putBox (o:IjsObj) (i:uint32) (v:IjsBox) =
    #endif
      if i > Index.Max then initSparse o
      if Utils.Object.isDense o then
        if i > 255u && i/2u > o.IndexLength then
          initSparse o
          o.IndexSparse.[i] <- v

        else
          let i = int i
          if i >= o.IndexDense.Length then expandStorage o i
          o.IndexDense.[i].Box <- v
          o.IndexDense.[i].HasValue <- true

      else
        o.IndexSparse.[i] <- v

      updateLength o i


    //--------------------------------------------------------------------------
    #if DEBUG
    let putVal (o:IjsObj) (i:uint32) (v:IjsNum) =
    #else
    let inline putVal (o:IjsObj) (i:uint32) (v:IjsNum) =
    #endif
      if i > Index.Max then initSparse o
      if Utils.Object.isDense o then
        if i > 255u && i/2u > o.IndexLength then
          initSparse o
          o.IndexSparse.[i] <- Utils.boxVal v

        else
          let i = int i
          if i >= o.IndexDense.Length then expandStorage o i
          o.IndexDense.[i].Box.Double <- v
          o.IndexDense.[i].HasValue <- true

      else
        o.IndexSparse.[i] <- Utils.boxVal v

      updateLength o i

    //--------------------------------------------------------------------------
    #if DEBUG
    let putRef (o:IjsObj) (i:uint32) (v:HostObject) (tc:TypeCode) =
    #else
    let inline putRef (o:IjsObj) (i:uint32) (v:HostObject) (tc:TypeCode) =
    #endif
      if i > Index.Max then initSparse o
      if Utils.Object.isDense o then
        if i > 255u && i/2u > o.IndexLength then
          initSparse o
          o.IndexSparse.[i] <- Utils.boxRef v tc

        else
          let i = int i
          if i >= o.IndexDense.Length then expandStorage o i
          o.IndexDense.[i].Box.Clr <- v
          o.IndexDense.[i].Box.Type <- tc

      else
        o.IndexSparse.[i] <- Utils.boxRef v tc

      updateLength o i

    //--------------------------------------------------------------------------
    #if DEBUG
    let get (o:IjsObj) (i:uint32) =
    #else
    let inline get (o:IjsObj) (i:uint32) =
    #endif
      match find o i with
      | null, _, _ -> Utils.boxedUndefined
      | o, index, isDense ->
        if isDense 
          then o.IndexDense.[int index].Box
          else o.IndexSparse.[index]
          
    //--------------------------------------------------------------------------
    let has (o:IjsObj) (i:uint32) =
      match find o i with
      | null, _, _ -> false
      | _ -> true

    //--------------------------------------------------------------------------
    let delete (o:IjsObj) (i:uint32) =
      match find o i with
      | null, _, _ -> true
      | o2, index, isDense ->
        if Utils.refEquals o o2 then
          if isDense then 
            o.IndexDense.[int i].Box <- Box()
            o.IndexDense.[int i].HasValue <- false
          else 
            o.IndexSparse.Remove i |> ignore

          true
        else false
        
    //--------------------------------------------------------------------------
    module Delegates =
      let putBox = PutBoxIndex putBox
      let putVal = PutValIndex putVal
      let putRef = PutRefIndex putRef
      let get = GetIndex get
      let has = HasIndex has
      let delete = DeleteIndex delete
    
    //--------------------------------------------------------------------------
    type Converters =
    
      //------------------------------------------------------------------------
      static member put (o:IjsObj, index:IjsBox, value:IjsBox) =
        match index with
        | NumberAndIndex i 
        | StringAndIndex i -> o.put(i, value)
        | Tagged tc -> o.put(TypeConverter.toString index, value)
        | _ -> failwith "Que?"
      
      static member put (o:IjsObj, index:IjsBool, value:IjsBox) =
        o.put(TypeConverter.toString index, value)
      
      static member put (o:IjsObj, index:IjsNum, value:IjsBox) =
        match index with
        | NumberIndex i -> o.put(i, value)
        | _ -> o.put(TypeConverter.toString index, value)
        
      static member put (o:IjsObj, index:HostObject, value:IjsBox) =
        match TypeConverter.toString index with
        | StringIndex i -> o.put(i, value)
        | index -> o.put(index, value)

      static member put (o:IjsObj, index:Undefined, value:IjsBox) =
        o.put("undefined", value)
      
      static member put (o:IjsObj, index:IjsStr, value:IjsBox) =
        match index with
        | StringIndex i -> o.put(i, value)
        | _ -> o.put(TypeConverter.toString index, value)

      static member put (o:IjsObj, index:IjsObj, value:IjsBox) =
        match TypeConverter.toString index with
        | StringIndex i -> o.put(i, value)
        | index -> o.put(index, value)
        
      //------------------------------------------------------------------------
      static member put (o:IjsObj, index:IjsBox, value:IjsVal) =
        match index with
        | NumberAndIndex i
        | StringAndIndex i -> o.put(i, value)
        | Tagged tc -> o.put(TypeConverter.toString index, value)
        | _ -> failwith "Que?"
      
      static member put (o:IjsObj, index:IjsBool, value:IjsVal) =
        o.put(TypeConverter.toString index, value)
      
      static member put (o:IjsObj, index:IjsNum, value:IjsVal) =
        match index with
        | NumberIndex i -> o.put(i, value)
        | _ -> o.put(TypeConverter.toString index, value)
        
      static member put (o:IjsObj, index:HostObject, value:IjsVal) =
        match TypeConverter.toString index with
        | StringIndex i -> o.put(i, value)
        | index -> o.put(index, value)

      static member put (o:IjsObj, index:Undefined, value:IjsVal) =
        o.put("undefined", value)
      
      static member put (o:IjsObj, index:IjsStr, value:IjsVal) =
        match index with
        | StringIndex i -> o.put(i, value)
        | _ -> o.put(TypeConverter.toString index, value)

      static member put (o:IjsObj, index:IjsObj, value:IjsVal) =
        match TypeConverter.toString index with
        | StringIndex i -> o.put(i, value)
        | index -> o.put(index, value)
        
      //------------------------------------------------------------------------
      static member put (o:IjsObj, index:IjsBox, value:IjsRef, tc:TypeCode) =
        match index with
        | NumberAndIndex i
        | StringAndIndex i -> o.put(i, value, tc)
        | Tagged tc -> o.put(TypeConverter.toString index, value)
        | _ -> failwith "Que?"
      
      static member put (o:IjsObj, index:IjsBool, value:IjsRef, tc:TypeCode) =
        o.put(TypeConverter.toString index, value, tc)
      
      static member put (o:IjsObj, index:IjsNum, value:IjsRef, tc:TypeCode) =
        match index with
        | NumberIndex i -> o.put(i, value)
        | _ -> o.put(TypeConverter.toString index, value, tc)
        
      static member put (o:IjsObj, index:HostObject, value:IjsRef, tc:TypeCode) =
        match TypeConverter.toString index with
        | StringIndex i -> o.put(i, value, tc)
        | index -> o.put(index, value, tc)

      static member put (o:IjsObj, index:Undefined, value:IjsRef, tc:TypeCode) =
        o.put("undefined", value, tc)
      
      static member put (o:IjsObj, index:IjsStr, value:IjsRef, tc:TypeCode) =
        match index with
        | StringIndex i -> o.put(i, value, tc)
        | _ -> o.put(TypeConverter.toString index, value, tc)

      static member put (o:IjsObj, index:IjsObj, value:IjsRef, tc:TypeCode) =
        match TypeConverter.toString index with
        | StringIndex i -> o.put(i, value, tc)
        | index -> o.put(index, value, tc)

      //------------------------------------------------------------------------
      static member get (o:IjsObj, index:IjsBox) =
        match index with
        | NumberAndIndex i
        | StringAndIndex i -> o.get i
        | Tagged tc -> o.get(TypeConverter.toString index)
        | _ -> failwith "Que?"
      
      static member get (o:IjsObj, index:IjsBool) =
        o.get(TypeConverter.toString index)
      
      static member get (o:IjsObj, index:IjsNum) =
        match index with
        | NumberIndex i -> o.get i
        | _ -> o.get(TypeConverter.toString index)
        
      static member get (o:IjsObj, index:HostObject) =
        match TypeConverter.toString index with
        | StringIndex i -> o.get i
        | index -> o.get(TypeConverter.toString index)

      static member get (o:IjsObj, index:Undefined) =
        o.get("undefined")
      
      static member get (o:IjsObj, index:IjsStr) =
        match index with
        | StringIndex i -> o.get i
        | _ -> o.get index

      static member get (o:IjsObj, index:IjsObj) =
        match TypeConverter.toString index with
        | StringIndex i -> o.get i
        | index -> o.get index

      //------------------------------------------------------------------------
      static member has (o:IjsObj, index:IjsBox) =
        match index with
        | NumberAndIndex i
        | StringAndIndex i -> o.has i
        | Tagged tc -> o.has(TypeConverter.toString index)
        | _ -> failwith "Que?"
      
      static member has (o:IjsObj, index:IjsBool) =
        o.has(TypeConverter.toString index)
      
      static member has (o:IjsObj, index:IjsNum) =
        match index with
        | NumberIndex i -> o.has i
        | _ -> o.has(TypeConverter.toString index)
        
      static member has (o:IjsObj, index:HostObject) =
        match TypeConverter.toString index with
        | StringIndex i -> o.has i
        | index -> o.has(TypeConverter.toString index)

      static member has (o:IjsObj, index:Undefined) =
        o.has("undefined")
      
      static member has (o:IjsObj, index:IjsStr) =
        match index with
        | StringIndex i -> o.has i
        | _ -> o.has index

      static member has (o:IjsObj, index:IjsObj) =
        match TypeConverter.toString index with
        | StringIndex i -> o.has i
        | index -> o.has index

module Arguments =

  module Index =

    //--------------------------------------------------------------------------
    let putBox (o:IjsObj) (i:uint32) (v:IjsBox) =
      let a = o :?> Arguments
      let ii = int i

      if a.LinkIntact && ii < a.LinkMap.Length then
        match a.LinkMap.[ii] with
        | ArgumentsLinkArray.Locals, index -> a.Locals.[index] <- v
        | ArgumentsLinkArray.ClosedOver, index -> a.ClosedOver.[index] <- v
        | _ -> failwith "Que?"

      Object.Index.putBox o i v
  
    //--------------------------------------------------------------------------
    let putVal (o:IjsObj) (i:uint32) (v:IjsNum) =
      let a = o :?> Arguments
      let ii = int i

      if a.LinkIntact && ii < a.LinkMap.Length then
        match a.LinkMap.[ii] with
        | ArgumentsLinkArray.Locals, index -> a.Locals.[index].Double <- v
        | ArgumentsLinkArray.ClosedOver, index -> 
          a.ClosedOver.[index].Double <- v
        | _ -> failwith "Que?"

      Object.Index.putVal o i v

    //--------------------------------------------------------------------------
    let putRef (o:IjsObj) (i:uint32) (v:IjsRef) (tag:TypeTag) =
      let a = o :?> Arguments
      let ii = int i

      if a.LinkIntact && ii < a.LinkMap.Length then
        match a.LinkMap.[ii] with
        | ArgumentsLinkArray.Locals, index -> 
          a.Locals.[index].Clr <- v
          a.Locals.[index].Tag <- tag

        | ArgumentsLinkArray.ClosedOver, index -> 
          a.ClosedOver.[index].Clr <- v
          a.ClosedOver.[index].Tag <- tag

        | _ -> failwith "Que?"

      Object.Index.putRef o i v tag
    
    //--------------------------------------------------------------------------
    let get (o:IjsObj) (i:uint32) =
      let a = o :?> Arguments
      let ii = int i

      if a.LinkIntact && ii < a.LinkMap.Length then
        match a.LinkMap.[ii] with
        | ArgumentsLinkArray.Locals, index -> a.Locals.[index]
        | ArgumentsLinkArray.ClosedOver, index -> a.ClosedOver.[index]
        | _ -> failwith "Que?"

      else
        Object.Index.get o i
        
    //--------------------------------------------------------------------------
    let has (o:IjsObj) (i:uint32) =
      let a = o :?> Arguments
      let ii = int i

      if a.LinkIntact && ii < a.LinkMap.Length 
        then true
        else Object.Index.has o i
        
    //--------------------------------------------------------------------------
    let delete (o:IjsObj) (i:uint32) =
      let a = o :?> Arguments
      let ii = int i

      if a.LinkIntact && ii < a.LinkMap.Length then
        a.copyLinkedValues()
        a.LinkIntact <- false
        a.Locals <- null
        a.ClosedOver <- null

      Object.Index.delete o i
        
    //--------------------------------------------------------------------------
    module Delegates =
      let putBox = PutBoxIndex putBox
      let putVal = PutValIndex putVal
      let putRef = PutRefIndex putRef
      let get = GetIndex get
      let has = HasIndex has
      let delete = DeleteIndex delete