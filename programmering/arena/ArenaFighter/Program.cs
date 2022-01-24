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

    [DataContract]
    public class Character
    {
        [DataMember]
        public string name; //name!

        [DataMember]
        public int health; //Hälsa!

        [DataMember]
        public int strength; //strength!

        [DataMember]
        public int sword; //Svärd!

        [DataMember]
        public int speed; //speed!

        [DataMember]
        public int breastplate; //Bröstplåt!

        [DataMember]
        public int greave; //greave!

        [DataMember]
        public int money; //money

        [DataMember]
        public int type; //type! 0 = player, 1 = motståndare

        [DataMember]
        public bool dead; //Är playern död?

        public Character(string name, int health, int strength, bool dead, int sword, int speed, int breastplate, int greave, int money, int type)
        {
            this.name = name;
            this.health = health;
            this.strength = strength;
            this.dead = dead;
            this.sword = sword;
            this.speed = speed;
            this.breastplate = breastplate;
            this.greave = greave;
            this.money = money;
            this.type = type;
        }
        public int TakeDamage(int dmg)
        {
            int temp = this.health - dmg;
            return temp;
        }
        public bool CheckIfDead()
        {
            if (this.health <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int RollTheDice()
        {
            //Rulla Tärningen
            Random slump = new Random();
            return slump.Next(1, 7); //Mellan 1 och 6.
        }
        public void Attack(Character offer, Game theGame)
        {
            offer.health = offer.TakeDamage(this.strength);
            Console.WriteLine(String.Format("{0} stabbed {2}. {2} took {1} damage.", this.name, this.strength, offer.name));
            theGame.LogThis(String.Format("{2} was stabbed {0} ({1} damage).", this.name, this.strength, offer.name));

            if (offer.health <= 0)
            {
                offer.dead = true;
                if (this.type == 0 && this.name != offer.name)
                {
                    //Give the player their reward
                    this.money += offer.money;
                    theGame.minmoney += 20;
                    theGame.maxmoney += 20;

                    //The Player gets a congratulations
                    Console.WriteLine(String.Format("Congratulations, you killed {0}", offer.name));
                    theGame.LogThis(String.Format("The player killed {0}, which had {1} money on them. The player's money is now {2}.", offer.name, offer.money, this.money));
                    Console.WriteLine(String.Format("You found {0} money on {1}. You now have {2} money.", offer.money, offer.name, this.money));

                    //Health is increased for the enemy
                    theGame.minhealth += 10;
                    theGame.maxhealth += 10;
                }
            }
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
        public int costsword;

        [DataMember]
        public int costbreastplate;

        [DataMember]
        public int costgreave;

        [DataMember]
        public List<string> log = new List<string>();

        public Game(int minhealth, int maxhealth, int minstrength, int maxstrength, int roundsDone, string text, int minmoney, int maxmoney, int costsword, int costbreastplate, int costgreave)
        {
            this.minhealth = minhealth;
            this.maxhealth = maxhealth;
            this.minstrength = minstrength;
            this.maxstrength = maxstrength;
            this.roundsDone = roundsDone;
            this.log.Add(text);
            this.minmoney = minmoney;
            this.maxmoney = maxmoney;
            this.costgreave = costgreave;
            this.costbreastplate = costbreastplate;
            this.costsword = costsword;
        }

        public void LogThis(string text)
        {
            //If the text is not null, then add it to the log
            if (text != null)
            {
                this.log.Add(text);
            }
        }
    }
    class ArenaFighter
    {
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
            return serializableObject;
        }

        static Character CharacterCreation(Game theGame)
        {
            //TODO: Add more names, the player might be too good!
            string[] name = new string[] { "Sofia Björn", "Clidna Annemarie", "Timo Thorne", "Merrill Latasha", "Jarka Iona", "Liis Rafaela", "Floella Halinka", "Elva Lamya", "Britta Conchobhar", "Alana Ellie", "Magni Tonya" };
            Random rand = new Random();

            //Health
            int shealth = rand.Next(theGame.minhealth, theGame.maxhealth);

            //Give the enemy a name
            int rname = rand.Next(1, name.Length);

            //Create the enemy
            Character enemy = new Character(name[rname], shealth, 0, false, 1, rand.Next(1, 51), 0, 0, rand.Next(theGame.minmoney, theGame.maxmoney), 1); 

            //Modify the strength
            enemy.strength = enemy.RollTheDice();

            //Save the enemy
            SaveViaDataContractSerialization(enemy, "enemy.xml");   //Save Motståndaren

            //Give back the enemy to the game
            return enemy;
        }

        static void Round(Character player, Character enemy, Game theGame)
        {
            //Clear the screen
            Console.Clear();

            //Log the Round
            theGame.LogThis(String.Format("Round {0}", theGame.roundsDone));

            //Temporary variables
            int playerMaxhealth = player.health;
            int mMaxhealth = enemy.health;
            bool endTheGame = false;

            //Write who the player is fighting
            Console.WriteLine(String.Format("{0} versus {1}. You have a health of {2} and a strength of {3}, the enemy has a health of {4}.", player.name, enemy.name, player.health, player.strength, enemy.health));
            if (player.speed > enemy.speed)
            {
                Console.WriteLine("You are faster than your enemy");
            }
            else
            {
                Console.WriteLine("You are slower than your enemy");
            }
            Console.WriteLine("Click enter to continue");
            Console.ReadLine();

            //Begin the while loop, but exit if the player is dead, the enemy is dead or the game has ended.
            while (!player.dead && !enemy.dead && !endTheGame)
            {
                Console.Clear();
                Console.WriteLine(String.Format("Your health: {0}/{1}", player.health, playerMaxhealth));
                Console.WriteLine(String.Format("{0}'s health: {1}/{2}", enemy.name, enemy.health, mMaxhealth));

                //If the player is faster than the enemy or the speed is the same, then the player goes first.
                if (player.speed>enemy.speed || player.speed==enemy.speed) { 
                    Console.WriteLine("1) Attack");
                    Console.WriteLine("2) Retire");
                    Console.WriteLine("3) Save and exit");
                    Console.Write("\r\nWhat do you want to do? ");
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
                            endTheGame = true;
                            break;
                        case "3":
                            //The player saves and exits.
                            Console.WriteLine(String.Format("It seems as if {0} wants to take a pause. So far, {0} has survived {1} rounds.", player.name, theGame.roundsDone));
                            theGame.LogThis(String.Format("The Game was exited at round {0} by the player.", theGame.roundsDone));
                            endTheGame = true;
                            SaveGame(theGame, player, enemy);
                            break;
                        default:
                            //The enemy takes damage
                            player.Attack(enemy, theGame);
                            break;
                    }
                    if (!endTheGame)
                    {
                        if (enemy.health > 0)
                        {
                            enemy.Attack(player, theGame);
                        }
                    }
                }
                //If the enemy is faster than the player, then the enemy goes first
                else
                {
                    if (!endTheGame)
                    {
                        if (enemy.health > 0)
                        {
                            enemy.Attack(player, theGame); //Attack the player
                        }
                    }
                    Console.WriteLine("1) Attack");
                    Console.WriteLine("2) Retire");
                    Console.WriteLine("3) Save and exit");
                    Console.Write("\r\nWhat do you want to do? ");
                    switch (Console.ReadLine())
                    {
                        case "1":
                            //The enemy takes damage
                            player.Attack(enemy, theGame);
                            break;
                        case "2":
                            //The player gives up
                            player.Attack(player, theGame);
                            player.dead = true;
                            endTheGame = true;
                            break;
                        case "3":
                            //The player saves and exits.
                            Console.WriteLine(String.Format("It seems as if {0} wants to take a pause. So far, {0} has survived {1} rounds.", player.name, theGame.roundsDone));
                            theGame.LogThis(String.Format("The Game was exited at round {0} by the player.", theGame.roundsDone));
                            endTheGame = true;
                            SaveGame(theGame, player, enemy);
                            break;
                        default:
                            //The enemy takes damage
                            player.Attack(enemy, theGame);
                            break;
                    }
                }
                Console.WriteLine("Click enter to continue...");
                Console.ReadLine();
            }
            if (!player.dead && enemy.dead && !endTheGame)
            {
                //The player's health is restored
                player.health = playerMaxhealth;

                //Log that the round has finished
                theGame.LogThis(String.Format("Round {0} done", theGame.roundsDone));
                theGame.roundsDone += 1;
                
                //New enemey
                enemy = CharacterCreation(theGame);

                //Randomize the strength of the player
                player.strength = player.RollTheDice();

                //Save the Game
                SaveGame(theGame, player, enemy);

                //Start the shop
                Console.WriteLine("You see a nice little shop");
                bool playerShop = true;
                int shopId = 0;
                while (playerShop)
                {
                    if (shopId == 0)
                    {
                        Console.WriteLine("1) Enter the shop");
                        Console.WriteLine("2) Go into battle");
                        Console.WriteLine("3) Save and exit");
                        Console.Write("\r\nWhat do you want to do? ");
                        switch (Console.ReadLine())
                        {
                            case "1":
                                shopId = 1;
                                break;
                            case "2":
                                playerShop = false;
                                break;
                            case "3":
                                playerShop = false;
                                endTheGame = true;
                                SaveGame(theGame, player, enemy);
                                break;
                            default:
                                playerShop = false;
                                break;
                        }
                    }
                    else if (shopId == 1)
                    {
                        Console.WriteLine(String.Format("You have {0} money", player.money));
                        Console.WriteLine(String.Format("1) Uppgrade the sword (costs {0})", theGame.costsword));
                        if (player.breastplate == 0) {
                            Console.WriteLine(String.Format("2) Buy a breast plate (costs {0})", theGame.costbreastplate));
                        }
                        else { 
                            Console.WriteLine(String.Format("2) Upgrade your breast plate (costs {0})", theGame.costbreastplate));
                        }
                        if (player.greave == 0)
                        {
                            Console.WriteLine(String.Format("3) Buy greaves (costs {0})", theGame.costgreave));
                        }
                        else
                        {
                            Console.WriteLine(String.Format("3) Upgrade your greaves (costs {0})", theGame.costgreave));
                        }
                        Console.WriteLine("4) Exit the shop");
                        Console.Write("\r\nWhat do you want to do? ");
                        switch (Console.ReadLine())
                        {
                            case "1":
                                if (player.money >= theGame.costsword) {
                                    player.sword += 1;
                                    player.money -= theGame.costsword;
                                    theGame.costsword *= 2;
                                    Console.WriteLine(String.Format("You've upgrade your sword to level {0}!\nNext time you want to upgrade your sword, it will cost {1}.", player.sword, theGame.costsword));
                                    theGame.LogThis(String.Format("The player now has a level {0} sword and {1} money", player.breastplate, player.money));
                                    SaveGame(theGame, player, enemy);
                                }
                                else
                                {
                                    Console.WriteLine(String.Format("Sorry, it costs {0} to upgrade your sword. You only have {1} and you need {2} more to be able to upgrade this sword.", theGame.costsword, player.money, theGame.costsword-player.money));
                                }
                                break;
                            case "2":
                                if (player.money >= theGame.costbreastplate)
                                {
                                    player.breastplate += 1;
                                    player.money -= theGame.costbreastplate;
                                    theGame.costbreastplate *= 2;
                                    if (player.breastplate != 1) { 
                                        Console.WriteLine(String.Format("You have upgrade your breast plate to level {0}!\nNext time you want to upgrade your breast plate, it will cost {1}.", player.breastplate, theGame.costbreastplate));
                                    }
                                    else
                                    {
                                        Console.WriteLine(String.Format("You've bought a breast plate!\nNext time you want to upgrade your breast plate, ít will cost {0}.", theGame.costbreastplate));
                                    }
                                    theGame.LogThis(String.Format("The player now has a level {0} breast plate and {1} money", player.breastplate, player.money));
                                    SaveGame(theGame, player, enemy);
                                }
                                else
                                {
                                    if (player.breastplate != 0) { 
                                        Console.WriteLine(String.Format("Sorry, it costs{0} to upgrade your breast plate. You have {1} and you need {2} more to be able to upgrade your breast plate.", theGame.costbreastplate, player.money, theGame.costbreastplate - player.money));
                                    }
                                    else
                                    {
                                        Console.WriteLine(String.Format("Sorry, it costs{0} to buy a breast plate. You have {1} and you need {2} more to be able to buy this breast plate.", theGame.costbreastplate, player.money, theGame.costbreastplate - player.money));
                                    }
                                }
                                break;
                            case "3":
                                if (player.money >= theGame.costgreave)
                                {
                                    player.greave += 1;
                                    player.money -= theGame.costgreave;
                                    theGame.costgreave *= 2;
                                    if (player.greave != 1)
                                    {
                                        Console.WriteLine(String.Format("You have upgrade your greaves to level {0}!\nNext time you want to upgrade your greaves, it will cost {1}.", player.greave, theGame.costgreave));
                                    }
                                    else
                                    {
                                        Console.WriteLine(String.Format("You have bought greaves!\nNext time you want to upgrade your greaves, it will cost {1}.", theGame.costgreave));
                                    }
                                    theGame.LogThis(String.Format("The player now has level {0} greaves och {1} money", player.greave, player.money));
                                    SaveGame(theGame, player, enemy);
                                }
                                else
                                {
                                    if (player.greave != 0)
                                    {
                                        Console.WriteLine(String.Format("Sorry, it costs{0} to upgrade your greaves. You have {1} and you need {2} more to be able to upgrade your greaves.", player.greave, theGame.costgreave));
                                    }
                                    else
                                    {
                                        Console.WriteLine(String.Format("Sorry, it costs{0} to buy the greaves. You have {1} and you need {2} more to be able to buy these greaves.", theGame.costgreave));
                                    }
                                }
                                break;
                            case "4":
                                shopId = 0;
                                break;
                            default:
                                shopId = 0;
                                break;
                        }
                    }
                }

                //New Round
                Round(player, enemy, theGame);
            }
            if (player.dead && !enemy.dead)
            {
                Console.WriteLine(String.Format("This seems the end for {0}, who bravely fought {1} rounds.", player.name, theGame.roundsDone - 1));
                theGame.LogThis(String.Format("The player died at round {0}. The Game is now quitting", theGame.roundsDone - 1));
                File.Delete("player.xml");
                File.Delete("enemy.xml");
                File.Delete("theGame.xml");
            }
        }

        static void SaveGame(Game theGame, Character player, Character enemy)
        {
            theGame.LogThis("The Game is now Saved");
            SaveViaDataContractSerialization(theGame, "theGame.xml");   //Save theGame
            SaveViaDataContractSerialization(player, "player.xml");   //Save playern
            SaveViaDataContractSerialization(enemy, "enemy.xml");   //Save playern
            Console.WriteLine("The Game is now Saved!");
        }
        static Game LoadGame()
        {
            Console.WriteLine("Loading the Game");
            return LoadViaDataContractSerialization<Game>("theGame.xml"); //Ladda in theGame.
        }

        static void Main()
        {
            //Variables
            Character player;
            Game theGame;
            Character enemy;

            //Does the game even exist?
            if (!File.Exists("theGame.xml"))
            {
                // Create the Game
                theGame = new Game(30, 71, 1, 11, 1, "The Beginning of the Game", 10, 101, 100, 125, 150);

                // Important variables
                Random rand = new Random();
                int shealth = rand.Next(30, 101); //Health, between 30 och 100.

                //Ask for the player's name
                Console.WriteLine("What is the name of your player?");

                //Create the player
                player = new Character(Console.ReadLine(), shealth, 0, false, 1, rand.Next(1, 51), 0, 0, 100, 0); //Create the player

                //Roll the dice
                Console.WriteLine("You throw a dice");
                player.strength = player.RollTheDice();

                //Save the player and load it
                SaveViaDataContractSerialization(player, "player.xml");   //Save the player
                player = null; //the player is "removed"
                player = LoadViaDataContractSerialization<Character>("player.xml"); //Load the player
                
                //Create the enemy
                enemy = CharacterCreation(theGame);

                //Save the game and its characters
                SaveGame(theGame, player, enemy);
            }
            else
            {
                //The game does exist, load it!
                theGame = LoadGame();
                theGame.LogThis("Loading the Game");

                //Load the player
                player = LoadViaDataContractSerialization<Character>("player.xml");
                if (player.dead == null)
                {
                    player.dead = false;
                }
                SaveViaDataContractSerialization(player, "player.xml");   //Save the player
                player = null; //the player is "removed"
                player = LoadViaDataContractSerialization<Character>("player.xml"); //Load the player

                //Load the enemy
                enemy = LoadViaDataContractSerialization<Character>("enemy.xml");
                if (enemy.dead == null)
                {
                    enemy.dead = false;
                }
                SaveViaDataContractSerialization(enemy, "enemy.xml"); //Save the enemy
                enemy = null; //the enemy is "removed"
                enemy = LoadViaDataContractSerialization<Character>("enemy.xml"); //Load the enemy
            }
            //Start the round
            Round(player, enemy, theGame);
        }
    }
}