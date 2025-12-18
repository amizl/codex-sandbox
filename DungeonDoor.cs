using System;

class DungeonDoor
{
    static void Main()
    {
        const int initialLives = 3;
        int lives = initialLives;
        int roomsSurvived = 0;
        Random random = new Random();

        Console.WriteLine("Welcome to Dungeon Door!");
        Console.WriteLine($"You begin with {initialLives} lives.\n");

        while (lives > 0)
        {
            int safeDoor = random.Next(1, 4);
            Console.WriteLine("You see three doors ahead. One is safe.");
            Console.WriteLine("Choose a door (1-3):");

            int choice = ReadDoorChoice();

            if (choice == safeDoor)
            {
                roomsSurvived++;
                Console.WriteLine("The door creaks open... safe! You move to the next room.\n");
            }
            else
            {
                lives--;
                Console.WriteLine($"Trapped! You lose a life. Lives remaining: {lives}.\n");
            }
        }

        Console.WriteLine("Game over!");
        Console.WriteLine($"Rooms survived: {roomsSurvived}");
    }

    private static int ReadDoorChoice()
    {
        while (true)
        {
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= 3)
            {
                return choice;
            }

            Console.WriteLine("Please enter 1, 2, or 3:");
        }
    }
}
