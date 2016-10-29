open System
open System.Linq
open Akka.Actor
open Akka.FSharp
open Actors

let rec run (supervisor : IActorRef) =

    System.Threading.Thread.Sleep(200)
    Console.Write("Command: ")

    let input = Console.ReadLine()
    if not (String.IsNullOrEmpty(input)) then
        let items = input.Split(' ')
        match items.Length with
        | 1 ->
            supervisor <! CreateActor (items.First())
        | 2 ->
            supervisor <! ActorCommand(items.First(), items.Last())
        | _ -> Console.WriteLine("Invalid command")

        run (supervisor)

let strategy () = 
    Strategy.OneForOne((fun ex ->
    Console.WriteLine("Invoking supervision strategy");
    match ex with 
    | :? ArgumentNullException -> 
        Console.WriteLine("Stopping actor");
        Directive.Stop
    | :? ArgumentOutOfRangeException -> 
        Console.WriteLine("Restarting actor");
        Directive.Restart
    | :? ArgumentException -> 
        Console.WriteLine("Resuming actor");
        Directive.Resume
    | _ -> Directive.Escalate), 3, TimeSpan.FromSeconds(10.))

[<EntryPoint>]
let main argv = 

    let system = System.create "system" <| Configuration.load ()
    let supervisor = spawnOpt system "runner" <| supervisingActor <| [ SpawnOption.SupervisorStrategy(strategy ()) ]

    run (supervisor)
    0
