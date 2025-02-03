using GrandChessTree.Toolkit;

Console.WriteLine("----The Grand Chess Tree Toolkit----");
var input = Console.ReadLine();
if (string.IsNullOrEmpty(input))
{
    Console.WriteLine("Invalid command.");
    return;
}

var commandParts = input.Split(':');

if (commandParts.Length == 0)
{
    Console.WriteLine("Invalid command.");
    return;
}

var command = commandParts[0];
if (command == "seed_positions")
{
    if (commandParts.Length != 2 || !int.TryParse(commandParts[1], out var depth))
    {
        Console.WriteLine("Invalid seed command format is 'seed_positions:<depth>'.");
        return;
    }
    await PositionSeeder.Seed(depth);
}
else if(command == "seed_account")
{
    await AccountSeeder.Seed();
}
else if (command == "seed_apikey")
{
    await ApiKeySeeder.Seed();
}
else if (command == "perft_full_reset")
{
    if (commandParts.Length != 2 || !int.TryParse(commandParts[1], out var depth))
    {
        Console.WriteLine("Invalid command format is 'perft_full_reset:<depth>'.");
        return;
    }
    await PerftClearer.FullReset(depth);
}
else if (command == "perft_release_incomplete")
{
    if (commandParts.Length != 2 || !int.TryParse(commandParts[1], out var depth))
    {
        Console.WriteLine("Invalid command format is 'perft_release_incomplete:<depth>'.");
        return;
    }
    await PerftClearer.ReleaseIncompleteTasks(depth);
}
else
{
    Console.WriteLine("unrecognized command.");
}
