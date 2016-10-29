using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace Supervision.CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystem = ActorSystem.Create("MyActorSystem");
            var supervisingActor = actorSystem.ActorOf(Props.Create(() => new SupervisingActor()), "supervisor");

            while (true)
            {
                System.Threading.Thread.Sleep(200);
                Console.Write("Command: ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    break;

                var items = input.Split(' ');
                if (items.Length == 1)
                {
                    supervisingActor.Tell(new CreateActor(items.First()));
                }
                else if (items.Length == 2)
                {
                    supervisingActor.Tell(new ActorCommand(items.First(), items.Last()));
                }
                else
                {
                    Console.WriteLine("Invalid command");
                }
            }

            actorSystem.Terminate().Wait();
        }
    }
}
