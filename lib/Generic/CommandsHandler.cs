using GlmNet;

namespace Lib
{
    abstract class Command
    {
        public static string defaultTarget = "";
        public abstract string Name { get; protected set; }
        public abstract void Execute(string[] parsedCommand, RaytracingScene scene);
    }

    class HelpCommand : Command
    {
        public override string Name { get; protected set; } = "help";

        public override void Execute(string[] parsedCommand, RaytracingScene scene)
        {
            // Add descriptions for each commands
            Console.WriteLine("List of commands: ");
            foreach (Command command in CommandParser.commands.Values.OfType<Command>())
            {
                Console.WriteLine(command.Name);
            }

        }
    }

    class SelectCommand : Command
    {
        public override string Name { get; protected set; } = "select";

        public override void Execute(string[] parsedCommand, RaytracingScene scene)
        {
            string target = parsedCommand[0];

            if (scene.ExistsInScene(target))
            {
                defaultTarget = target;
                Console.WriteLine("Selected " + defaultTarget + " as default target");
            }
            else
            {
                Console.WriteLine("Specified target is not in scene!");
            }
        }
    }

    class DeselectCommand : Command
    {
        public override string Name { get; protected set; } = "deselect";

        public override void Execute(string[] parsedCommand, RaytracingScene scene)
        {
            defaultTarget = "";
            Console.WriteLine("Removed previous default target");
        }
    }

    class NewCommand : Command
    {
        public override string Name { get; protected set; } = "new";

        public override void Execute(string[] parsedCommand, RaytracingScene scene) 
        {
            /*
                new sphere (0 0 0)
                new model (path) /\(0 0 0)/\
            */

            string type = "";

            if (parsedCommand.Length < 1)
            {
                Console.WriteLine("Invalid arguments for \"new\" command. Expected string (string) float float float"); return;
            }

            type = parsedCommand[0];

            if (type.ToLower().Equals("sphere"))
            {
                if (parsedCommand.Length == 1)
                {
                    scene.AddObjectInScene(new Sphere(Common.GiveDefaultFreeName(scene, "Sphere")));
                }
                else if (parsedCommand.Length == 4)
                {
                    if (!float.TryParse(parsedCommand[parsedCommand.Length - 3], out float pos1) ||
                        !float.TryParse(parsedCommand[parsedCommand.Length - 2], out float pos2) ||
                        !float.TryParse(parsedCommand[parsedCommand.Length - 1], out float pos3))
                    {
                        Console.WriteLine("Invalid arguments for \"new\" command. Expected float float float"); return;
                    }

                    vec3 position = new vec3(pos1, pos2, pos3);
                    Sphere sphere = new Sphere(Common.GiveDefaultFreeName(scene, "Sphere"));
                    sphere.position = position;
                    scene.AddObjectInScene(sphere);
                }
                else if (parsedCommand.Length == 5)
                {
                    string name = parsedCommand[1];

                    if (!float.TryParse(parsedCommand[parsedCommand.Length - 3], out float pos1) ||
                        !float.TryParse(parsedCommand[parsedCommand.Length - 2], out float pos2) ||
                        !float.TryParse(parsedCommand[parsedCommand.Length - 1], out float pos3))
                    {
                        Console.WriteLine("Invalid arguments for \"new\" command. Expected float float float"); return;
                    }

                    vec3 position = new vec3(pos1, pos2, pos3);
                    Sphere sphere = new Sphere(Common.GiveDefaultFreeName(scene, name));
                    sphere.position = position;
                    scene.AddObjectInScene(sphere);
                }
                else
                {
                    Console.WriteLine("Invalid arguments for \"new\" command. Expected string (string) float float float"); return;
                }
            }
            else if (type.ToLower().Equals("model"))
            {
                Console.WriteLine("ci godo emoji");
            }
        }
    }

    class MoveCommand : Command
    {
        public override string Name { get; protected set; } = "move";

        public override void Execute(string[] parsedCommand, RaytracingScene scene)
        {
            // TEMP: ONLY SPHERES, ADD SUPPORT FOR MODELS LATER

            string target = "";

            if (parsedCommand.Length < 3)
            {
                Console.WriteLine("Did not specify enough argument for \"move\" command!"); return;
            }
            else if (parsedCommand.Length == 3)
            {
                if (!defaultTarget.Equals(""))
                {
                    target = defaultTarget;
                }
                else
                {
                    Console.WriteLine("An appropiate selected or specified target for \"move\" command was not found!"); return;
                }
            }
            else if (parsedCommand.Length == 4)
            {
                target = parsedCommand[0];
            }
            else if (parsedCommand.Length > 4)
            {
                Console.WriteLine("Too many arguments for command \"move\"!"); return;
            }

            if (!scene.ExistsInScene(target))
            {
                Console.WriteLine("Specified target is not in scene!"); return;
            }

            if (scene.GetObjectByName(target) is Sphere)
            {
                if (!float.TryParse(parsedCommand[parsedCommand.Length - 3], out float pos1) ||
                !float.TryParse(parsedCommand[parsedCommand.Length - 2], out float pos2) ||
                !float.TryParse(parsedCommand[parsedCommand.Length - 1], out float pos3))
                {
                    Console.WriteLine("Invalid arguments for \"move\" command. Expected float float float"); return;
                }

                Sphere sphere = (Sphere)scene.GetObjectByName(target);

                vec3 moveCoords = new vec3(pos1, pos2, pos3);
                sphere.position = moveCoords;
                Console.WriteLine("Moved " + target + " at position " + moveCoords);
            }
            else
            {
                Console.WriteLine("Support for this command on objects of type \"Model\" coming soon ;)");
            }
        }
    }

    class CommandParser
    {
        public static Dictionary<string, Command> commands;

        public CommandParser()
        {
            commands = new Dictionary<string, Command>();
            RegisterCommands();
        }

        private void RegisterCommands() {
            RegisterCommand(new HelpCommand());
            RegisterCommand(new SelectCommand());
            RegisterCommand(new DeselectCommand());
            RegisterCommand(new NewCommand());
            RegisterCommand(new MoveCommand());
        }

        private void RegisterCommand(Command command)
        {
            commands[command.Name] = command;
        }

        public void ParseAndExecute(string input, RaytracingScene scene)
        {
            string[] parts = input.Split(' ');
            if (parts.Length == 0) {
                Console.WriteLine("Invalid command."); return;
            }

            string commandName = parts[0];
            if (commands.TryGetValue(commandName, out Command command))
            {
                string[] arguments = parts.Skip(1).ToArray();
                command.Execute(arguments, scene);
            }
            else
            {
                Console.WriteLine("Unknown command: " + commandName);
            }
        }

    }
}
