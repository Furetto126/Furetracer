using GlmNet;

namespace Lib
{
    abstract class Command
    {
        public static int defaultTarget = -1;
        public abstract string Name { get; protected set; }
        public abstract void Execute(string[] parsedCommand, Shader shader);
    }

    class SelectCommand : Command
    {
        public override string Name { get; protected set; } = "select";

        public override void Execute(string[] parsedCommand, Shader shader)
        {
            int target = -1;

            if (!int.TryParse(parsedCommand[0], out target))
            {
                Console.WriteLine("Invalid arguments for \"select\" command. Expected int"); return;
            }
            else if (target < 0 || target >= shader.GetSpheresList().Count)
            {
                Console.WriteLine("Did not find a matching object index!"); return;
            }

            defaultTarget = target;
            Console.WriteLine("Selected " + defaultTarget + " as default target");
        }
    }

    class DeselectCommand : Command
    {
        public override string Name { get; protected set; } = "deselect";

        public override void Execute(string[] parsedCommand, Shader shader)
        {
            defaultTarget = -1;
            Console.WriteLine("Removed previous default target");
        }
    }

    class MoveCommand : Command
    {
        public override string Name { get; protected set; } = "move";

        public override void Execute(string[] arguments, Shader shader)  // move (69) 0 0 0
        {
            int target = -1;

            if (arguments.Length < 3)
            {
                Console.WriteLine("Did not specify enough argument for \"move\" command!"); return;
            }
            else if (arguments.Length == 3)
            {
                if (defaultTarget != -1)
                {
                    target = defaultTarget;
                }
                else
                {
                    Console.WriteLine("An appropiate selected or specified target for \"move\" command was not found!"); return;
                }
            }
            else if (arguments.Length == 4) {
                if (!int.TryParse(arguments[0], out target))
                {
                    Console.WriteLine("Did not specify a valid argument value!"); return;
                }
            }

            if (target < 0 || target >= shader.GetSpheresList().Count) {
                Console.WriteLine("Did not find a matching object index!"); return;
            }
            
            if (!float.TryParse(arguments[arguments.Length - 3], out float pos1) ||
                !float.TryParse(arguments[arguments.Length - 2], out float pos2) || 
                !float.TryParse(arguments[arguments.Length - 1], out float pos3))
            {
                Console.WriteLine("Invalid arguments for \"move\" command. Expected float float float"); return;
            }

            vec3 moveCoords = new vec3(pos1, pos2, pos3);
            shader.GetSpheresList()[target].position = moveCoords;
            Console.WriteLine("Moved " + target + " at position " + moveCoords);
        }
    }

    class CommandParser
    {
        private Dictionary<string, Command> commands;

        public CommandParser()
        {
            commands = new Dictionary<string, Command>();
            RegisterCommands();
        }

        private void RegisterCommands() {
            RegisterCommand(new SelectCommand());
            RegisterCommand(new DeselectCommand());
            RegisterCommand(new MoveCommand());
        }

        private void RegisterCommand(Command command)
        {
            commands[command.Name] = command;
        }

        public void ParseAndExecute(string input, Shader shader)
        {
            string[] parts = input.Split(' ');
            if (parts.Length == 0) {
                Console.WriteLine("Invalid command."); return;
            }

            string commandName = parts[0];
            if (commands.TryGetValue(commandName, out Command command))
            {
                string[] arguments = parts.Skip(1).ToArray();
                command.Execute(arguments, shader);
            }
            else
            {
                Console.WriteLine("Unknown command: " + commandName);
            }
        }

    }
}
