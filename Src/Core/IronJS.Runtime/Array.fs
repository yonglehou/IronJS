﻿namespace IronJS.Native

open System
open IronJS
open IronJS.Api.Extensions
open IronJS.DescriptorAttrs

//------------------------------------------------------------------------------
// 15.4
module Array =

  //----------------------------------------------------------------------------
  // 15.4.2
  let constructor' (f:IjsFunc) (_:IjsObj) (args:IjsBox array) : IjsObj =
    if args.Length = 1 then
      let number = Api.TypeConverter.toNumber args.[0]
      let size = Api.TypeConverter.toUInt32 number
      Api.Environment.createArray f.Env size

    else
      let size = args.Length |> uint32
      let array = Api.Environment.createArray f.Env size
      
      Array.iteri (fun i value -> 
        array.Methods.PutBoxIndex.Invoke(array, uint32 i, value)) args

      array
      
  //----------------------------------------------------------------------------
  let setupConstructor (env:IjsEnv) =
    let ctor = new Func<IjsFunc, IjsObj, IjsBox array, IjsObj>(constructor')
    let ctor = Api.HostFunction.create env ctor

    ctor.ConstructorMode <- ConstructorModes.Host
    ctor.put("prototype", env.Prototypes.Array, Immutable)

    env.Globals.put("Array", ctor)
    env.Constructors <- {env.Constructors with Array = ctor}
    
  //----------------------------------------------------------------------------
  let createPrototype (env:IjsEnv) objPrototype =
    let prototype = Api.Environment.createArray env 0u
    prototype.Prototype <- objPrototype
    prototype.Class <- Classes.Array
    prototype
    
  //----------------------------------------------------------------------------
  let setupPrototype (env:IjsEnv) =
    env.Prototypes.Array.put("constructor", env.Constructors.Array, DontEnum)

