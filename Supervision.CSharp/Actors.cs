using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace Supervision.CSharp
{
    class SupervisingActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is CreateActor)
            {
                var actorId = (message as CreateActor).ActorId;

                Context.ActorOf(Props.Create(() => new WorkerActor()), actorId);

                Console.WriteLine("Created child actor with id {0}", actorId);
            }
            else if (message is ActorCommand)
            {
                var actorId = (message as ActorCommand).ActorId;
                var command = (message as ActorCommand).Command;

                Context.ActorSelection(actorId).Tell(command);

                Console.WriteLine("Sent command {0} to actor {1}", command, actorId);
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                3,
                TimeSpan.FromSeconds(10),
                x =>
                {
                    Console.WriteLine("Invoking supervision strategy");
                    if (x is ArgumentNullException)
                    {
                        Console.WriteLine("Stopping actor");
                        return Directive.Stop;
                    }
                    if (x is ArgumentOutOfRangeException)
                    {
                        Console.WriteLine("Restarting actor");
                        return Directive.Restart;
                    }
                    if (x is ArgumentException)
                    {
                        Console.WriteLine("Resuming actor");
                        return Directive.Resume;
                    }
                    return Directive.Escalate;
                });
        }
    }

    class WorkerActor : ReceiveActor
    {
        private readonly List<string> _commands;

        public WorkerActor()
        {
            _commands = new List<string>();

            Empty();
        }

        private void Empty()
        {
            Receive<string>(cmd =>
            {
                Console.WriteLine("Actor {0} has state {1}", this.Self.Path, "Empty");
                HandleCommand(cmd);
                Become(NotEmpty);
            });
        }

        private void NotEmpty()
        {
            Receive<string>(cmd =>
            {
                Console.WriteLine("Actor {0} has state {1}", this.Self.Path, "NonEmpty");
                HandleCommand(cmd);
            });
        }

        private void HandleCommand(string cmd)
        {
            Console.WriteLine("Actor {0} received command {1}", this.Self.Path, cmd);

            if (cmd == "null")
                throw new ArgumentNullException();
            if (cmd.StartsWith("-"))
                throw new ArgumentOutOfRangeException();
            if (char.IsPunctuation(cmd[0]))
                throw new ArgumentException();

            _commands.Add(cmd);
            Console.WriteLine("Actor {0} has commands history: [{1}]", this.Self.Path, string.Join("|", _commands));
        }

        protected override void PreStart()
        {
            Console.WriteLine("PreStart");
            _commands.Clear();
            base.PreStart();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            Console.WriteLine("PreRestart");
            this.Self.Tell("RETRY-" + message);
            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            Console.WriteLine("PostRestart");
            base.PostRestart(reason);
        }

        protected override void PostStop()
        {
            Console.WriteLine("PostStop");
            base.PostStop();
        }
    }
}
