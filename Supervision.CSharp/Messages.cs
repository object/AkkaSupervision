using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supervision.CSharp
{
    class CreateActor
    {
        public CreateActor(string actorId)
        {
            this.ActorId = actorId;
        }

        public string ActorId { get; private set; }
    }

    class ActorCommand
    {
        public ActorCommand(string actorId, string command)
        {
            this.ActorId = actorId;
            this.Command = command;
        }

        public string ActorId { get; private set; }
        public string Command { get; private set; }
    }
}
