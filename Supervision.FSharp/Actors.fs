module Actors

open System
open Akka.Actor
open Akka.FSharp

type Command = 
    | CreateActor of string
    | ActorCommand of string * string

let workerActor (mailbox:Actor<_>) =
    let rec loop (commands : string list) =
        actor {
            Console.WriteLine("Actor {0} has commands history: [{1}]", mailbox.Self.Path, String.Join("|", commands));
            let! message = mailbox.Receive ()
            match message with
            | ActorCommand (actorId, cmd) -> 
                match cmd with
                | "null" -> raise <| ArgumentNullException()
                | cmd when cmd.StartsWith("-") -> raise<| ArgumentOutOfRangeException()
                | cmd when Char.IsPunctuation(cmd.[0]) -> raise <| ArgumentException()
                | _ -> return! loop (cmd :: commands)
            | _ -> ()

            return! loop (commands)
        }

    loop ([])

let supervisingActor (mailbox:Actor<_>) =
    let rec loop () =
        actor {
            let! message = mailbox.Receive ()
            match message with
            | CreateActor actorId -> 
                spawn mailbox.Context actorId <| workerActor |> ignore
            | ActorCommand (actorId, cmd) ->
                let actor = select actorId mailbox.Context
                actor <! ActorCommand (actorId, cmd)

            return! loop ()
        }

    loop()
