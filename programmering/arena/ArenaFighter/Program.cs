using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Arena
{
    public enum CharacterType
    {
        Player,
        Enemy,
        Merchant
    }

    [DataContract]
    public class StorePrices
    {
        [DataMember]
        public int costsword;

        [DataMember]
        public int costbreastplate;

        [DataMember]
        public int costgreave;
    }

    [DataContract]
    public class ArenaVariety
    {
        [DataMember]
        public int healthBalance;

        [DataMember]
        public int strengthBalance;

        [DataMember]
        public int speedBalance;
    }

    [DataContract]
    public class Character
    {
        [DataMember]
        public string name; //Name of the Character

        [DataMember]
        public int health; //Health of the Character

        [DataMember]
        public int strength; //Strength of the Character

        [DataMember]
        public int sword; //Level of Sword

        [DataMember]
        public int speed; //Speed of the Character

        [DataMember]
        public int breastplate; //Level of Breast Plate

        [DataMember]
        public int greave; //Level of Greave

        [DataMember]
        public int money; //Amount of Money

        [DataMember]
        public CharacterType type; //Character Type

        [DataMember]
        public bool dead; //Is the player dead?

        public Character(string _name, int _health, int _strength, bool _dead, int _sword, int _speed, int _breastplate, int _greave, int _money, CharacterType _type)
        {
            name = _name;
            health = _health;
            strength = _strength;
            dead = _dead;
            sword = _sword;
            speed = _speed;
            breastplate = _breastplate;
            greave = _greave;
            money = _money;
            type = _type;
        }
        public int RollTheDice()
        {
            //Rulla Tärningen
            Random slump = new Random();
            return slump.Next(1, 7); //Mellan 1 och 6.
        }
        public void Attack(Character victim, Game theGame)
        {
            victim.AddHealth(-this.strength, theGame);
            Console.WriteLine(String.Format("{0} stabbed {2}. {2} took {1} damage.", name, strength, victim.name));
            theGame.LogThis(String.Format("{2} was stabbed {0} ({1} damage).", name, strength, victim.name));

            if (victim.health <= 0)
            {
                victim.dead = true;
                if (type == CharacterType.Player && name != victim.name)
                {
                    //Give the player their reward
                    money += victim.money;
                    theGame.minmoney += 20;
                    theGame.maxmoney += 20;

                    //The Player gets a congratulations
                    Console.WriteLine(String.Format("Congratulations, you killed {0}", victim.name));
                    theGame.LogThis(String.Format("The player killed {0}, which had {1} money on them. The player's money is now {2}.", victim.name, victim.money, money));
                    Console.WriteLine(String.Format("You found {0} money on {1}. You now have {2} money.", victim.money, victim.name, money));

                    //Health is increased for the enemy
                    theGame.minhealth += 10;
                    theGame.maxhealth += 10;
                }
                if (type == CharacterType.Enemy && victim.type == CharacterType.Player && victim.dead == true)
                {
                    theGame.end = true;
                }
            }
        }

        public void AddStrength(int _strength, Game theGame)
        {
            strength += _strength;
            if (strength <= 0)
            {
                strength = 1;
            }
            theGame.LogThis(String.Format("{0}'s strength is now {1}", name, strength));
        }

        public void SetStrength(int _strength, Game theGame)
        {
            strength = _strength;
            if (strength <= 0)
            {
                strength = 1;
            }
            theGame.LogThis(String.Format("{0}'s strength is now {1}", name, strength));
        }

        public void AddHealth(int _health, Game theGame)
        {
            health += _health;
            theGame.LogThis(String.Format("{0}'s health is now {1}", name, health));
        }

        public void SetHealth(int _health, Game theGame)
        {
            health = _health;
            theGame.LogThis(String.Format("{0}'s health is now {1}", name, health));
        }
    }
    [DataContract]
    public class Game
    {
        [DataMember]
        public int minhealth;

        [DataMember]
        public int maxhealth;

        [DataMember]
        public int minmoney;

        [DataMember]
        public int maxmoney;

        [DataMember]
        public int minstrength;

        [DataMember]
        public int maxstrength;

        [DataMember]
        public int roundsDone;

        [DataMember]
        public StorePrices storePrices;

        [DataMember]
        public ArenaVariety arenaVariety;

        [DataMember]
        public bool end;

        [DataMember]
        public List<string> log = new();

        public Game(int _minhealth, int _maxhealth, int _minstrength, int _maxstrength, int _roundsDone, string _text,
                    int _minmoney, int _maxmoney, int _costsword, int _costbreastplate, int _costgreave, bool _end, int _healthBalance, int _strengthBalance, int _speedBalance)
        {
            minhealth = _minhealth;
            maxhealth = _maxhealth;
            minstrength = _minstrength;
            maxstrength = _maxstrength;
            roundsDone = _roundsDone;
            minmoney = _minmoney;
            maxmoney = _maxmoney;
            end = _end;
            storePrices = new();
            storePrices.costgreave = _costgreave;
            storePrices.costbreastplate = _costbreastplate;
            storePrices.costsword = _costsword;
            arenaVariety = new();
            arenaVariety.healthBalance = _healthBalance;
            arenaVariety.speedBalance = _speedBalance;
            arenaVariety.strengthBalance = _strengthBalance;
            log.Add(_text);
        }

        public void LogThis(string text)
        {
            //If the text is not null, then add it to the log
            if (text != null)
            {
                log.Add(text);
            }
        }

        public void randomizeArena(int arena = 0)
        {
            //0 = random
            if (arena > 0)
            {
                switch(arena)
                {
                    case 1:
                        //Jungle Arena
                        Console.WriteLine("Now entering... Jungle Arena");
                        arenaVariety.healthBalance = 0;
                        arenaVariety.speedBalance = -5;
                        arenaVariety.strengthBalance = 2;
                        Console.WriteLine(String.Format("Your health will be changed by {0}, your speed by {1} and your strength by {2}", arenaVariety.healthBalance, arenaVariety.speedBalance, arenaVariety.strengthBalance));
                        break;
                    case 2:
                        //Desert Arena
                        Console.WriteLine("Now entering... Desert Arena");
                        arenaVariety.healthBalance = -10;
                        arenaVariety.speedBalance = -10;
                        arenaVariety.strengthBalance = -1;
                        Console.WriteLine(String.Format("Your health will be changed by {0}, your speed by {1} and your strength by {2}", arenaVariety.healthBalance, arenaVariety.speedBalance, arenaVariety.strengthBalance));
                        break;
                    case 3:
                        //Wetlands
                        Console.WriteLine("Now entering... Wetlands Arena");
                        arenaVariety.healthBalance = 0;
                        arenaVariety.speedBalance = -15;
                        arenaVariety.strengthBalance = 0;
                        Console.WriteLine(String.Format("Your health will be changed by {0}, your speed by {1} and your strength by {2}", arenaVariety.healthBalance, arenaVariety.speedBalance, arenaVariety.strengthBalance));
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Random rand = new Random();
                randomizeArena(rand.Next(1,4));

            }
        }
    }
    class ArenaFighter
    {
        public const int startingMinHealth = 30;
        public const int startingMaxHealth = 71;
        public const string whatgonnado = "\r\nWhat do you want to do? ";
        public const string shopuText = "You've upgrade your {0} to level {1}!\nNext time you want to upgrade your {0}, it will cost {2}.";
        public const string shopbText = "You've bought {0}!\nNext time you want to upgrade your {0}, it will cost {1}.";
        public const string shopuSorry = "Sorry, it costs {1} to upgrade your {0}. You only have {2} and you need {3} more to be able to upgrade this {0}.";
        public const string shopbSorry = "Sorry, it costs {1} to buy {0}. You only have {2} and you need {3} more to be able to buy this {0}.";

        static void SaveViaDataContractSerialization<T>(T serializableObject, string filepath)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
            };
            var writer = XmlWriter.Create(filepath, settings);
            serializer.WriteObject(writer, serializableObject);
            writer.Close();
        }


        static T LoadViaDataContractSerialization<T>(string filepath)
        {
            var fileStream = new FileStream(filepath, FileMode.Open);
            var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
            var serializer = new DataContractSerializer(typeof(T));
            T serializableObject = (T)serializer.ReadObject(reader, true);
            reader.Close();
            fileStream.Close();
            if (serializableObject != null)
            {
                return serializableObject;
            }
            throw new NullReferenceException("serializableObject was null");
        }

        static Character CharacterCreation(Game theGame)
        {
            Random rand = new();

            string randomCharacterName = GetName();

            //The health is between Min Health and Max Health, starting is between 30 and 70
            int startingHealth = rand.Next(theGame.minhealth, theGame.maxhealth);


            //Create the Enemy
            Character enemy = new(randomCharacterName, startingHealth, 0, false, 1, rand.Next(1, 51), 0, 0, rand.Next(theGame.minmoney, theGame.maxmoney), CharacterType.Enemy);

            //Roll the Dice for the Enemy
            enemy.SetStrength(enemy.RollTheDice(), theGame);

            //Save the enemy
            SaveViaDataContractSerialization(enemy, "enemy.xml");

            //Return the enemy
            return enemy;
        }

        private static string GetName()
        {
            Random rand = new();

            string[] names = new string[] {
                "Sofia",
                "Clidna",
                "Timo",
                "Merrill",
                "Jarka",
                "Feodor",
                "Ivan",
                "Elva",
                "Britta",
                "Alana",
                "Tonya",
                "Damascius",
                "Raeda",
                "Domus",
                "Panem"
            };
            string[] title = new string[] {
                "the Adopted",
                "the Tempered",
                "the Liberator",
                "the Quietest",
                "the Dark",
                "the Blessed",
                "the Terrible",
                "the Elven King",
                "the Rough",
                "the Powerful",
                "the Great",
                "the Fierce",
                "the Avenger",
                "the Conqueror",
                "the Beloved"
            };
            return names[rand.Next(0, names.Length)] + " " + title[rand.Next(0,title.Length)];
        }

        static void Round(Character player, Character enemy, Game theGame)
        {
            //Clear the screen
            Console.Clear();

            //Log the Round
            theGame.LogThis(String.Format("Round {0}", theGame.roundsDone));

            //Temporary variables
            int playerMaxhealth = player.health;
            int playerStrength = player.strength;
            int mMaxhealth = enemy.health;

            //Make sure the player's strength is accurate
            player.AddStrength(player.sword*2, theGame);
            player.health += (player.greave*2);
            player.health += (player.breastplate*2);
            theGame.LogThis(String.Format("{0}'s health is now {1}", player.name, player.health));

            //Arena Variety
            player.AddStrength(theGame.arenaVariety.strengthBalance, theGame);
            enemy.AddStrength(theGame.arenaVariety.strengthBalance, theGame);
            player.AddHealth(theGame.arenaVariety.healthBalance, theGame);
            enemy.AddHealth(theGame.arenaVariety.healthBalance, theGame);


            //Write who the player is fighting
            Console.WriteLine(String.Format("{0} versus {1}. You have a health of {2} and a strength of {3}, the enemy has a health of {4}.", player.name, enemy.name, player.health, player.strength, enemy.health));
            if (player.speed >= enemy.speed)
            {
                Console.WriteLine("You are faster than your enemy");
                PlayerTurn(player, theGame, enemy);
                EnemyTurn(player, theGame, enemy);
            }
            else
            {
                Console.WriteLine("You are slower than your enemy");
                EnemyTurn(player, theGame, enemy);
                PlayerTurn(player, theGame, enemy);
                Console.WriteLine("Click enter to continue...");
                Console.ReadLine();
            }

            //Begin the while loop, but exit if the player is dead, the enemy is dead or the game has ended.
            while (!player.dead && !enemy.dead && !theGame.end)
            {
                Console.Clear();
                Console.WriteLine(String.Format("Your health: {0}/{1}", player.health, playerMaxhealth));
                Console.WriteLine(String.Format("{0}'s health: {1}/{2}", enemy.name, enemy.health, mMaxhealth));

                //If the player is faster than the enemy or the speed is the same, then the player goes first.
                if (player.speed >= enemy.speed)
                {
                    PlayerTurn(player, theGame, enemy);
                    EnemyTurn(player, theGame, enemy);
                }
                //If the enemy is faster than the player, then the enemy goes first
                else
                {
                    EnemyTurn(player, theGame, enemy);
                    PlayerTurn(player, theGame, enemy);
                }
                Console.WriteLine("Click enter to continue...");
                Console.ReadLine();
            }

            if (!player.dead && enemy.dead && !theGame.end)
            {
                //Reset the max health of the player
                player.SetHealth(playerMaxhealth, theGame);

                //Log that the round is finished
                theGame.LogThis(String.Format("Round {0} done", theGame.roundsDone));
                theGame.roundsDone += 1;

                //A new enemy is added
                enemy = CharacterCreation(theGame);

                //Randomize the Strength of the Player
                player.SetStrength(player.RollTheDice(), theGame);

                //Save the Game
                SaveGame(theGame, player, enemy);

                //Start the shop
                ShopSubroutine(theGame, player, enemy);

                //New round
                theGame.randomizeArena();
                Round(player, enemy, theGame);
            }
        }

        private static void ShopSubroutine(Game theGame, Character player, Character enemy)
        {
            bool playerShop = false;
            Console.WriteLine("You see a nice little shop");
            Console.WriteLine("1) Enter the shop");
            Console.WriteLine("2) Go into battle");
            Console.WriteLine("3) Save and exit");
            Console.Write(whatgonnado);

            switch (Console.ReadLine())
            {
                case "1":
                    playerShop = true;
                    break;
                case "2":
                    break;
                case "3":
                    theGame.end = true;
                    SaveGame(theGame, player, enemy);
                    break;
                default:
                    break;
            }

            //Stay in the shop while the player shops.
            while (playerShop)
            {
                Console.WriteLine(string.Format("You have {0} money", player.money));
                Console.WriteLine(string.Format("1) To upgrade the sword (costs {0})", theGame.storePrices.costsword));
                if (player.breastplate == 0)
                {
                    Console.WriteLine(string.Format("2) Buy a breast plate (costs {0})", theGame.storePrices.costbreastplate));
                }
                else
                {
                    Console.WriteLine(string.Format("2) Upgrade your breast plate (costs {0})", theGame.storePrices.costbreastplate));
                }
                if (player.greave == 0)
                {
                    Console.WriteLine(string.Format("3) Buy greaves (costs {0})", theGame.storePrices.costgreave));
                }
                else
                {
                    Console.WriteLine(string.Format("3) Upgrade your greaves (costs {0})",
                                                    theGame.storePrices.costgreave));
                }
                Console.WriteLine("4) Exit the shop");
                Console.Write(whatgonnado);
                switch (Console.ReadLine())
                {
                    case "1":
                        if (player.money >= theGame.storePrices.costsword)
                        {
                            player.sword += 1;
                            player.money -= theGame.storePrices.costsword;
                            theGame.storePrices.costsword *= 2;
                            Console.WriteLine(string.Format(shopuText,"sword",player.sword, theGame.storePrices.costsword));
                            theGame.LogThis(string.Format("The player now has a level {0} sword and {1} money",
                                                          player.sword, player.money));
                            SaveGame(theGame, player, enemy);
                        }
                        else
                        {
                            Console.WriteLine(string.Format(shopuSorry, "sword", theGame.storePrices.costsword, player.money, theGame.storePrices.costsword - player.money));
                        }
                        break;
                    case "2":
                        if (player.money >= theGame.storePrices.costbreastplate)
                        {
                            player.breastplate += 1;
                            player.money -= theGame.storePrices.costbreastplate;
                            theGame.storePrices.costbreastplate *= 2;
                            if (player.breastplate >= 0)
                            {
                                Console.WriteLine(string.Format(shopuText, "breast plate", player.breastplate, theGame.storePrices.costbreastplate));
                            }
                            else
                            {
                                Console.WriteLine(string.Format(shopbText, "breast plate", theGame.storePrices.costbreastplate));
                            }
                            theGame.LogThis(string.Format("The player now has a level {0} breast plate and {1} money",
                                                              player.breastplate, player.money));
                            SaveGame(theGame, player, enemy);
                        }
                        else
                        {
                            if (player.breastplate != 0)
                            {
                                Console.WriteLine(string.Format(shopuSorry, "breast plate", theGame.storePrices.costbreastplate, player.money, theGame.storePrices.costbreastplate - player.money));
                            }
                            else
                            {
                                Console.WriteLine(string.Format(shopbSorry, "breast plate", theGame.storePrices.costbreastplate, player.money, theGame.storePrices.costbreastplate - player.money));
                            }
                        }
                        break;
                    case "3":
                        if (player.money >= theGame.storePrices.costgreave)
                        {
                            player.greave += 1;
                            player.money -= theGame.storePrices.costgreave;
                            theGame.storePrices.costgreave *= 2;
                            if (player.greave >= 0)
                            {
                                Console.WriteLine(string.Format(shopuText, "greaves", player.greave, theGame.storePrices.costgreave));
                            }
                            else
                            {
                                Console.WriteLine(string.Format(shopbText, "greaves", theGame.storePrices.costgreave));
                            }
                            theGame.LogThis(string.Format("The player now has level {0} greaves and {1} money", player.greave, player.money));
                            SaveGame(theGame, player, enemy);
                        }
                        else
                        {
                            if (player.greave != 0)
                            {
                                Console.WriteLine(string.Format(shopuSorry, "greaves", theGame.storePrices.costgreave, player.money, theGame.storePrices.costgreave - player.money));
                            }
                            else
                            {
                                Console.WriteLine(string.Format(shopbSorry, "greaves", theGame.storePrices.costgreave, player.money, theGame.storePrices.costgreave - player.money));
                            }
                        }
                        break;
                    default:
                        playerShop = false;
                        break;
                }
            }
        }

        private static void EnemyTurn(Character player, Game theGame, Character enemy)
        {
            if (!theGame.end && enemy.health > 0)
            {
                enemy.Attack(player, theGame);
            }
        }

        private static void PlayerTurn(Character player, Game theGame, Character enemy)
        {
            Console.WriteLine("1) Attack");
            Console.WriteLine("2) Retire");
            Console.WriteLine("3) Save and exit");
            Console.Write(whatgonnado);
            switch (Console.ReadLine())
            {
                case "1":
                    //The enemy takes damage
                    player.Attack(enemy, theGame);
                    break;
                case "2":
                    //The player retires
                    player.Attack(player, theGame);
                    player.dead = true;
                    theGame.end = true;
                    break;
                case "3":
                    //The player saves and exits.
                    Console.WriteLine(String.Format("It seems as if {0} wants to take a pause. So far, {0} has survived {1} rounds.", player.name, theGame.roundsDone));
                    theGame.LogThis(String.Format("The Game was exited at round {0} by the player.", theGame.roundsDone));
                    theGame.end = true;
                    SaveGame(theGame, player, enemy);
                    break;
                default:
                    //The enemy takes damage
                    player.Attack(enemy, theGame);
                    break;
            }
        }

        static void SaveGame(Game theGame, Character player, Character enemy)
        {
            theGame.LogThis("Attempting to Save Files");
            SaveViaDataContractSerialization(theGame, "theGame.xml");   //Save the Game
            theGame.LogThis("Game saved successfully");
            SaveViaDataContractSerialization(player, "player.xml");   //Save the player
            theGame.LogThis("Player saved successfully");
            SaveViaDataContractSerialization(enemy, "enemy.xml");   //Save enemy
            theGame.LogThis("Enemy saved successfully");
            theGame.LogThis("All files saved");
            Console.WriteLine("The Game is now Saved!");
        }
        static Game LoadGame()
        {
            Console.WriteLine("Loading the Game");
            return LoadViaDataContractSerialization<Game>("theGame.xml"); //Load the Game
        }

        static void Main()
        {
            //Variabler
            Character? player;
            Game theGame;
            Character enemy;
            Random rand = new();

            //Does the game even exist?
            if (!File.Exists("theGame.xml"))
            {
                //Create the Game
                theGame = new Game(30, 71, 1, 11, 1, "The Beginning of the Game", 10, 101, 100, 125, 150, false, 0, 0, 0);

                //Important variables
                int startingHealth = rand.Next(30, 101); //Health, between 30 och 100.

                //Ask the player
                Console.WriteLine("What is the name of your player?");

                var name = InputValidText(Console.ReadLine());

                //Create the player
                player = new Character(name, startingHealth, 0, false, 1, rand.Next(1, 51), 0, 0, 100, 0); //Create the player

                //Rulla tärningen
                Console.WriteLine("You throw a dice");
                player.AddStrength(player.RollTheDice(), theGame);

                //Save the player och ladda in
                SaveViaDataContractSerialization(player, "player.xml");   //Save the player
                player = LoadViaDataContractSerialization<Character>("player.xml"); //Ladda in player.

                //Create Motståndaren
                enemy = CharacterCreation(theGame);

                //Save the Game and its characters
                SaveGame(theGame, player, enemy);
                Console.Clear();
                theGame.randomizeArena();
                Console.WriteLine("Click enter to continue...");
                Console.ReadLine();
            }
            else
            {
                //The game does exist, load it.
                theGame = LoadGame();
                theGame.LogThis("Loading the Game");

                //Load in the player
                player = LoadViaDataContractSerialization<Character>("player.xml"); 
                player.dead = false;

                //Load in the enemy
                enemy = LoadViaDataContractSerialization<Character>("enemy.xml");
                enemy.dead = false;
            }
            //Start the Round
            Round(player, enemy, theGame);
            if (theGame.end)
            {
                if (player.dead)
                {
                    Console.WriteLine(String.Format("This seems the end for {0}, who bravely fought {1} rounds.", player.name, theGame.roundsDone - 1));
                    theGame.LogThis(String.Format("The player died at round {0}. The Game is now quitting", theGame.roundsDone - 1));
                    File.Delete("player.xml");
                    File.Delete("enemy.xml");
                    File.Delete("theGame.xml");
                }
            }
        }

        private static string InputValidText(string? v)
        {
            string? input = v;
            bool valid = false;
            while (!valid)
            {
                if (input != null)
                {
                    return input;
                }
                Console.WriteLine("Please enter some text.");
            }
            return "Foo"; //This should not happen.
        }
    }
}