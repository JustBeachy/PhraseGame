using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

[Serializable]
public class PhraseList : MonoBehaviour
{
    List<string> phrases = new List<string>();
    public static bool StartGame = true;
    public Text wordToGuess;
    string phraseOfTheDay;
    public string hiddenWord;
    public GameObject[] buttonList;
    public bool suddenDeath = false;
    public Text notificationText;
    string path;
    public string date;
    public List<string> buttonSaves;
    public bool win = false;
    public bool lose = false;

    public static PhraseList PL = new PhraseList();
    // Start is called before the first frame update
    void Start()
    {
        date = DateTime.Today.ToString("M/d/yyyy");
        path = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveFile.json";
        LoadListOfPhrases();
        PickWord();
        Load();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LetterClicked(char c, GameObject buttonClicked)
    {
        bool isInWord = false;
        int counter = 0;
        char[] charArray = hiddenWord.ToCharArray();
        foreach (char ch in phraseOfTheDay)
        {
            if (ch == c)
            {
                isInWord = true;
                charArray[counter] = c;
            }

            counter++;
        }
        hiddenWord = new string(charArray);
        wordToGuess.text = hiddenWord;


        if (hiddenWord == phraseOfTheDay)
        {
            Win();
        }


        if (!anyLeftToLock()) //if no buttons left
        {
            StartSuddenDeath();
        }
        else
        {
            if (!isInWord)//if letter not in word
            {
                if (!suddenDeath)
                {
                    LockRandom();
                    LockRandom();

                    if (!anyLeftToLock())
                        StartSuddenDeath();
                }
                else
                {
                    Lose();
                }
            }
        }
        Save();
    }

    void Win()
    {
        win = true;
        StartGame = false;
        notificationText.color = Color.green;
        notificationText.text = "Great Job! \nPlay again tomorrow!";
        GetComponent<Text>().color = Color.green;
    }


    void Lose()
    {
        lose = true;
        StartGame = false;
        notificationText.color = Color.red;
        notificationText.text = "Nice try. \nPlay again tomorrow!";
        hiddenWord = phraseOfTheDay;
        wordToGuess.text = phraseOfTheDay;
        GetComponent<Text>().color = Color.red;
        //lose game
    }

    public void StartSuddenDeath()
    {
        
        if (GameObject.FindGameObjectsWithTag("Lock").Length > 0)
        {
            notificationText.color = Color.yellow;
            notificationText.text = "Letters unlocked. \nA mistake will end the game.";
            suddenDeath = true;
            Save();
            foreach (GameObject g in buttonList)
            {
                if (g.GetComponent<LetterButton>().locked)
                    g.GetComponent<LetterButton>().UnlockLetter();
            }
        }
    }

    void LockRandom()
    {
        if (anyLeftToLock())
        {
            int rand = UnityEngine.Random.Range(0, buttonList.Length);
            if (!buttonList[rand].GetComponent<LetterButton>().locked && buttonList[rand].GetComponent<Button>().interactable)
            {
                buttonList[rand].GetComponent<LetterButton>().LockLetter();
            }
            else
            {
                LockRandom();
            }
        }
        else
        {
            StartSuddenDeath();
        }

    }

    bool anyLeftToLock()
    {
        bool anyUnlocked = false;
        foreach (GameObject g in buttonList)
        {
            if (!g.GetComponent<LetterButton>().locked && g.GetComponent<Button>().interactable)
            {
                anyUnlocked = true;

            }
        }
        return anyUnlocked;
    }

    void PickWord()
    {
        var startDate = new DateTime(2022, 4,2 , 0, 0, 0);

        hiddenWord = "";

        //int random = UnityEngine.Random.Range(0, phrases.Count);
        phraseOfTheDay = phrases[((DateTime.Now - startDate).Days) % phrases.Count];


        phraseOfTheDay = phraseOfTheDay.Replace(",", "");
        phraseOfTheDay = phraseOfTheDay.Replace("-", "");
        phraseOfTheDay = phraseOfTheDay.Replace("(", "");
        phraseOfTheDay = phraseOfTheDay.Replace(")", "");
        phraseOfTheDay = phraseOfTheDay.Replace("'", "");
        phraseOfTheDay = phraseOfTheDay.Replace(";", "");
        phraseOfTheDay = phraseOfTheDay.Replace("?", "");

        phraseOfTheDay = phraseOfTheDay.ToUpper();


        foreach (char c in phraseOfTheDay)
        {
            if (c != ' ')
                hiddenWord += '_';
            else
                hiddenWord += ' ';
        }

        wordToGuess.text = hiddenWord;

    }

    void Save()
    {
        PhraseList saveFile = gameObject.GetComponent<PhraseList>();
        saveFile.hiddenWord = hiddenWord;
        saveFile.date = date;

        foreach (GameObject b in buttonList)
        {
            if (!b.GetComponent<Button>().interactable)
                buttonSaves.Add("I"); //hold crate distances in save file
            else if (b.GetComponent<LetterButton>().locked)
                buttonSaves.Add("L");
            else
                buttonSaves.Add("G");
        }

        string json = JsonUtility.ToJson(saveFile);
        //print(json);
        StreamWriter sw = new StreamWriter(path);
        //File.WriteAllText(@path, json);
        sw.Write(json);
        sw.Close();
        buttonSaves.Clear();
        buttonSaves.TrimExcess();
    }

    void Load()
    {
        try
        {
            //if (File.Exists(@path))
            // {
            StreamReader sr = new StreamReader(path);
            string loadedString = sr.ReadToEnd();
            sr.Close();
                //string loadedString = File.ReadAllText(@path);
                
                //loadedString = EncryptDecrypt(loadedString, 1337); //comment out for testing -encryption
                JsonUtility.FromJsonOverwrite(loadedString, PL);
                if (PL.date == date)//check if same day
                {
                   
                    hiddenWord = PL.hiddenWord;
                    wordToGuess.text = PL.hiddenWord;

                    for (int i = 0; i < PL.buttonSaves.Count; i++)
                    {
                        if (PL.buttonSaves[i] == "I")
                            buttonList[i].GetComponent<Button>().interactable = false;
                        else if (PL.buttonSaves[i] == "L")
                            buttonList[i].GetComponent<LetterButton>().LockLetter();

                    }

                    if (PL.suddenDeath)
                        StartSuddenDeath();

                    if (PL.win)
                        Win();


                    if (PL.lose)
                        Lose();
              //  }


            }
        }
        catch (Exception e)
        {
            print("failed to load");
        }
    }
 

    void LoadListOfPhrases()
    {
        phrases.Add("Time flies");
        phrases.Add("The grass is always greener (on the other side)");
        phrases.Add("April showers bring forth May flowers");
        phrases.Add("Faith will move mountains");
        phrases.Add("Money talks");
        phrases.Add("Never cast a clout until May be out");
        phrases.Add("All is grist that comes to the mill");
        phrases.Add("The exception which proves the rule");
        phrases.Add("To be worn out is to be renewed");
        phrases.Add("In the midst of life, we are in death");
        phrases.Add("To learn a language is to have one more window from which to look at the world");
        phrases.Add("Put your money where your mouth is");
        phrases.Add("It is best to be on the safe side");
        phrases.Add("It will come back and haunt you");
        phrases.Add("It takes a whole village to raise a child");
        phrases.Add("There is no shame in not knowing; the shame lies in not finding out");
        phrases.Add("An ounce of prevention is worth a pound of cure");
        phrases.Add("Out of the frying pan and into the fire");
        phrases.Add("It ain't over until it's over");
        phrases.Add("All that glitters is not gold");
        phrases.Add("Money does not grow on trees");
        phrases.Add("Tis better to have loved and lost than never to have loved at all");
        phrases.Add("A stitch in time saves nine");
        phrases.Add("All things come to those who wait");
        phrases.Add("Do not throw the baby out with the bathwater");
        phrases.Add("Least said, soonest mended");
        phrases.Add("East, west, home is best,");
        phrases.Add("Ask my companion if I be a thief");
        phrases.Add("Beware of Greeks bearing gifts");
        phrases.Add("You either die a hero or live long enough to see yourself become the villain");
        phrases.Add("Hard cases make bad law");
        phrases.Add("Good things come to those who wait");
        phrases.Add("He who loves the world as his body may be entrusted with the empire");
        phrases.Add("Needs must when the devil drives");
        phrases.Add("Mighty oaks from little acorns grow");
        phrases.Add("A leopard cannot change its spots");
        phrases.Add("Fretting cares make grey hair");
        phrases.Add("Whom the Gods love die young");
        phrases.Add("The streets are paved with gold");
        phrases.Add("Good talk saves the food");
        phrases.Add("Seek and ye shall find");
        phrases.Add("Respect is not given, it is earned");
        phrases.Add("If you're growing in age, then you're nearing to the graveyard");
        phrases.Add("No pain, no gain");
        phrases.Add("The boy is father to the man");
        phrases.Add("There is no accounting for tastes");
        phrases.Add("The best things in life are free");
        phrases.Add("Fish always rots from the head downwards");
        phrases.Add("Put your best foot forward");
        phrases.Add("Third time is a charm");
        phrases.Add("The husband is always the last to know");
        phrases.Add("Putting the cart before the horse");
        phrases.Add("Music has charms to soothe the savage breast");
        phrases.Add("Fair exchange is no robbery");
        phrases.Add("Do not judge a book by its cover");
        phrases.Add("Everyone has their price");
        phrases.Add("Rules were made to be broken");
        phrases.Add("The early bird catches the worm");
        phrases.Add("Children should be seen and not heard");
        phrases.Add("The female of the species is more deadly than the male");
        phrases.Add("If God had meant us to fly, he would have given us wings");
        phrases.Add("While there is life there is hope");
        phrases.Add("Cheese, wine, and friends must be old to be good");
        phrases.Add("More haste, less speed");
        phrases.Add("There is an exception to every rule");
        phrases.Add("Feed a cold and starve a fever");
        phrases.Add("Home is where the heart is");
        phrases.Add("The only disability in life is a bad attitude");
        phrases.Add("Opportunity does not knock until you build a door");
        phrases.Add("The worm will turn");
        phrases.Add("Parsley seed goes nine times to the Devil");
        phrases.Add("The light is on but nobody is home");
        phrases.Add("A new language is a new life");
        phrases.Add("You are what you eat");
        phrases.Add("Half a loaf is better than no bread");
        phrases.Add("There's nowt so queer as folk");
        phrases.Add("Do not change horses in midstream");
        phrases.Add("Failing to plan is planning to fail");
        phrases.Add("Do not put too many irons in the fire");
        phrases.Add("It is better to have loved and lost than never to have loved at all");
        phrases.Add("Do not teach your Grandmother to suck eggs");
        phrases.Add("One half of the world does not know how the other half lives");
        phrases.Add("What goes up must come down");
        phrases.Add("Better late than never");
        phrases.Add("Better to light one candle than to curse the darkness");
        phrases.Add("Practice makes perfect");
        phrases.Add("There is one born every minute");
        phrases.Add("It goes without saying");
        phrases.Add("I'm going to have to give you the pink slip");
        phrases.Add("Money is not everything");
        phrases.Add("If you must dance with the Devil, you might as well know his favorite song");
        phrases.Add("Learn a language, and you will avoid a war");
        phrases.Add("Better the Devil you know than the Devil you do not");
        phrases.Add("It is what it is");
        phrases.Add("You cannot get blood out of a stone");
        phrases.Add("Loose lips sink ships");
        phrases.Add("Hunger never knows the taste, sleep never knows the comfort");
        phrases.Add("Turn your face toward the sun and the shadows fall behind you");
        phrases.Add("If ifs and ands were pots and pans, there would be no work for tinkers");
        phrases.Add("Do not count your chickens before they are hatched");
        phrases.Add("You cannot have your cake and eat it too");
        phrases.Add("There is no such thing as a free lunch");
        phrases.Add("Pearls of wisdom");
        phrases.Add("Do as you would be done by");
        phrases.Add("Smooth move, Ex-lax");
        phrases.Add("Might is right");
        phrases.Add("Discretion is the better part of valour");
        phrases.Add("Time goes by slowly when your are living intensely");
        phrases.Add("The best laid schemes of mice and men often go awry");
        phrases.Add("Hindsight is always twenty twenty");
        phrases.Add("Speak as you find");
        phrases.Add("Born with a silver spoon in one's mouth");
        phrases.Add("Money makes many things, but also makes devil dance");
        phrases.Add("The last drop makes the cup run over");
        phrases.Add("A bad workman blames his tools");
        phrases.Add("Silence is golden");
        phrases.Add("If wishes were horses, beggars would ride");
        phrases.Add("Let not the sun go down on your wrath");
        phrases.Add("The more the merrier");
        phrases.Add("Virtue is its own reward");
        phrases.Add("Speak of the devil and he shall appear");
        phrases.Add("With great power comes great responsibility");
        phrases.Add("An apple a day keeps the doctor away");
        phrases.Add("Never too old to learn");
        phrases.Add("Two heads are better than one");
        phrases.Add("Into every life a little rain must fall");
        phrases.Add("First come, first served");
        phrases.Add("Absence makes the heart grow fonder ");
        phrases.Add("The leopard does not change his spots");
        phrases.Add("He who can, does; he who cannot, teaches");
        phrases.Add("It ain’t what you don't know that gets you into trouble It's what you know for sure that just ain’t so");
        phrases.Add("Do not put all your eggs in one basket");
        phrases.Add("Time and tide wait for no man");
        phrases.Add("If you lie down with dogs, you will get up with fleas");
        phrases.Add("You cannot burn a candle at both ends");
        phrases.Add("Money earned by deceit, goes by deceit");
        phrases.Add("Eat, drink and be merry, for tomorrow we die");
        phrases.Add("The enemy of my enemy is my friend");
        phrases.Add("Fine feathers make fine birds");
        phrases.Add("The way to a man's heart is through his stomach");
        phrases.Add("Honesty is the best policy");
        phrases.Add("Handsome is as handsome does");
        phrases.Add("To travel hopefully is a better thing than to arrive");
        phrases.Add("Two can play at that game");
        phrases.Add("Live for today, for tomorrow never comes");
        phrases.Add("Like father, like son");
        phrases.Add("Do not meet troubles half-way");
        phrases.Add("It takes all sorts to make a world");
        phrases.Add("Not all those who wander are lost");
        phrases.Add("If you steal from one author, it is plagiarism; if you steal from many, it is research");
        phrases.Add("It is an ill wind (that blows no one any good)");
        phrases.Add("Ignorance is bliss");
        phrases.Add("Do not lock the stable door after the horse has bolted");
        phrases.Add("If you cannot be good, be careful");
        phrases.Add("A bad excuse is better than none");
        phrases.Add("Late lunch makes day go faster");
        phrases.Add("Ugly is as ugly does");
        phrases.Add("Talk of Angels, and hear the flutter of their wings");
        phrases.Add("One man's meat is another man's poison");
        phrases.Add("Many a mickle makes a muckle");
        phrases.Add("Do unto others as you would have them do unto you");
        phrases.Add("Easy come, easy go");
        phrases.Add("If you cannot stand the heat, get out of the kitchen");
        phrases.Add("The labourer is worthy of his hire");
        phrases.Add("You scratch my back and I will scratch yours");
        phrases.Add("There but for the grace of God go I");
        phrases.Add("To each his own");
        phrases.Add("If you've got it, flaunt it");
        phrases.Add("Know which side (one's) bread is buttered (on)");
        phrases.Add("Fish and guests smell after three days");
        phrases.Add("There is no place like home");
        phrases.Add("Keep your chickens separate");
        phrases.Add("A man's home is his castle");
        phrases.Add("It is the squeaky wheel that gets the grease");
        phrases.Add("Talk of the Devil, and he is bound to appear");
        phrases.Add("Judge not, that ye be not judgedgod is lord");
        phrases.Add("Business before pleasure");
        phrases.Add("The end justifies the means");
        phrases.Add("Cross the stream where it is shallowest");
        phrases.Add("Even a worm will turn");
        phrases.Add("Patience is a virtue");
        phrases.Add("Every stick has two ends");
        phrases.Add("Hope for the best, and prepare for the worst");
        phrases.Add("Ask no questions and hear no lies");
        phrases.Add("Let your hair down");
        phrases.Add("Little pitchers have big ears");
        phrases.Add("A chain is only as strong as its weakest link");
        phrases.Add("(You cannot) teach an old dog new tricks");
        phrases.Add("Truth is more valuable if it takes you a few years to find it");
        phrases.Add("No news is good news");
        phrases.Add("One kind word can warm three winter months");
        phrases.Add("Tomorrow never comes");
        phrases.Add("A fool and his money are soon parted");
        phrases.Add("One man's trash is another man's treasure");
        phrases.Add("You must have rocks in your head");
        phrases.Add("Pride comes before a fall");
        phrases.Add("No man can serve two masters");
        phrases.Add("You cannot run with the hare and hunt with the hounds");
        phrases.Add("Open confession is good for the soul");
        phrases.Add("Variety is the spice of life");
        phrases.Add("People who live in glass houses should not throw stones");
        phrases.Add("Those who know many languages live as many lives as the languages they know");
        phrases.Add("Curiosity killed the cat");
        phrases.Add("Give a man a fish and you feed him for a day Teach a man to fish and you feed him for a lifetime");
        phrases.Add("The longest day must have an end");
        phrases.Add("You catch more flies with honey than with vinegar");
        phrases.Add("It is better to be smarter than you appear than to appear smarter than you are");
        phrases.Add("Other times other manners");
        phrases.Add("Little strokes fell great oaks");
        phrases.Add("The pot calling the kettle black");
        phrases.Add("Do not cast your pearls before swine");
        phrases.Add("Tell the truth and shame the Devil");
        phrases.Add("Never tell tales out of school");
        phrases.Add("Fight fire with fire");
        phrases.Add("There are none so blind as those who will not see");
        phrases.Add("There are always more fish in the sea");
        phrases.Add("Too little, too late");
        phrases.Add("If a job is worth doing, it is worth doing well");
        phrases.Add("Revenge is sweet");
        phrases.Add("Oil and water do not mix");
        phrases.Add("It takes two to tango");
        phrases.Add("The only way to find a friend is to be one");
        phrases.Add("History repeats itself");
        phrases.Add("What goes around, comes around");
        phrases.Add("Do not let the grass grow beneath one's feet");
        phrases.Add("Shiny are the distant hills");
        phrases.Add("There's no need to wear a hair shirt");
        phrases.Add("When you have seen one, you have seen them all");
        phrases.Add("Moderation in all things");
        phrases.Add("Devil take the hindmost");
        phrases.Add("You cannot make an omelette without breaking eggs");
        phrases.Add("Ask a silly question and you will get a silly answer");
        phrases.Add("Too much of a good thing");
        phrases.Add("The Devil finds work for idle hands to do");
        phrases.Add("What the eye does not see (the heart does not grieve over)");
        phrases.Add("Do not burn your bridges behind you");
        phrases.Add("One swallow does not make a summer");
        phrases.Add("You pay your money and you take your choice");
        phrases.Add("One who believes in Sword, dies by the Sword");
        phrases.Add("Birds of a feather flock together");
        phrases.Add("Two is company, but three is a crowd,");
        phrases.Add("Any publicity is good publicity");
        phrases.Add("Be yourself");
        phrases.Add("Better to have loved and lost than never to have loved at all");
        phrases.Add("Kill the chicken to scare the monkey");
        phrases.Add("There is many a good tune played on an old fiddle");
        phrases.Add("He who sups with the Devil should have a long spoon");
        phrases.Add("It is the empty can that makes the most noise");
        phrases.Add("If you cannot live longer, live deeper");
        phrases.Add("Set a thief to catch a thief");
        phrases.Add("Never judge a book by its cover");
        phrases.Add("Penny, Penny Makes many");
        phrases.Add("Dead men tell no tales");
        phrases.Add("When the cat is away, the mice will play");
        phrases.Add("Idle hands are the devil's playthings");
        phrases.Add("The cobbler always wears the worst shoes");
        phrases.Add("Men get spoiled by staying, Women get spoiled by wandering");
        phrases.Add("A rising tide lifts all boats");
        phrases.Add("Do not cut off your nose to spite your face");
        phrases.Add("Where there is life there is hope");
        phrases.Add("Still waters run deep");
        phrases.Add("Sticks and stones may break my bones, but words will never hurt me");
        phrases.Add("Early marriage, earlier pregnant");
        phrases.Add("The innocent seldom find an uncomfortable pillow");
        phrases.Add("Man proposes, heaven disposes");
        phrases.Add("Flattery will get you nowhere");
        phrases.Add("Imitation is the sincerest form of flattery");
        phrases.Add("Big fish eat little fish");
        phrases.Add("An eye for an eye makes the whole world blind");
        phrases.Add("Never let the truth get in the way of a good story ");
        phrases.Add("What the eye does not see, the heart does not grieve over");
        phrases.Add("Do not bite the hand that feeds you");
        phrases.Add("Youth is wasted on the young");
        phrases.Add("Nothing succeeds like success,");
        phrases.Add("Nothing is certain but death and taxes");
        phrases.Add("Who will bell the cat?");
        phrases.Add("Keep your chin up");
        phrases.Add("It is better to light a candle than curse the darkness");
        phrases.Add("Money makes the world go around");
        phrases.Add("You cannot push a rope");
        phrases.Add("If you play with fire, you will get burned");
        phrases.Add("The shoemaker's son always goes barefoot");
        phrases.Add("Let bygones be bygones");
        phrases.Add("Well done is better than well said");
        phrases.Add("All is well that ends well");
        phrases.Add("Denial is not a river in Egypt");
        phrases.Add("Little things please little minds");
        phrases.Add("One year's seeding makes seven years weeding");
        phrases.Add("The work praises the man");
        phrases.Add("(March comes) in like a lion, (and goes) out like a lamb");
        phrases.Add("Do not make a mountain out of a mole hill");
        phrases.Add("It is the early bird that gets the worm");
        phrases.Add("You cannot win them all");
        phrases.Add("All good things come to him who waits");
        phrases.Add("Everybody wants to go to heaven but nobody wants to die");
        phrases.Add("Attack is the best form of defense");
        phrases.Add("Christmas comes but once a year");
        phrases.Add("Every man for himself and the Devil take the hindmost");
        phrases.Add("The bigger they are, the harder they fall");
        phrases.Add("When in Rome, (do as the Romans do)");
        phrases.Add("Beauty is in the eye of the beholder");
        phrases.Add("United we stand, divided we fall");
        phrases.Add("Tell me who your friends are, and I'll tell you who you are");
        phrases.Add("It is not enough to learn how to ride, you must also learn how to fall");
        phrases.Add("Blood is thicker than water");
        phrases.Add("Barking dogs seldom bite");
        phrases.Add("Haste makes waste");
        phrases.Add("(The) truth will out");
        phrases.Add("Free is for me");
        phrases.Add("One hand washes the other");
        phrases.Add("Horses for courses");
        phrases.Add("Do not upset the apple-cart");
        phrases.Add("You can have too much of a good thing");
        phrases.Add("The customer is always right");
        phrases.Add("Eat breakfast like a king, lunch like a prince and dinner like a pauper");
        phrases.Add("You've made your bed and you must lie in it");
        phrases.Add("What you lose on the swings you gain on the roundabouts");
        phrases.Add("Great minds think alike");
        phrases.Add("Make hay while the sun shines");
        phrases.Add("A little learning is a dangerous thing");
        phrases.Add("Do not put the cart before the horse");
        phrases.Add("Do not sympathize with those who can not empathize");
        phrases.Add("Laugh and the world laughs with you, weep and you weep alone");
        phrases.Add("You are never too old to learn");
        phrases.Add("The pen is mightier than the sword");
        phrases.Add("No man is an island");
        phrases.Add("Worrying never did anyone any good");
        phrases.Add("March comes in like a lion and goes out like a lamb");
        phrases.Add("Those who do not learn from history are doomed to repeat it");
        phrases.Add("A friend in need is a friend indeed");
        phrases.Add("Time is a great healer");
        phrases.Add("Do not look a gift horse in the mouth");
        phrases.Add("Speak softly and carry a big stick");
        phrases.Add("Old soldiers never die, (they just fade away)");
        phrases.Add("Nature abhors a vacuum");
        phrases.Add("The left hand doesn't know what the right hand is doing");
        phrases.Add("All things must pass");
        phrases.Add("Cowards may die many times before their death");
        phrases.Add("Absolute power corrupts absolutely");
        phrases.Add("It is on");
        phrases.Add("The die is cast");
        phrases.Add("Two birds with one stone");
        phrases.Add("Marry in haste, repent at leisure");
        phrases.Add("Jack of all trades, master of none");
        phrases.Add("Hard work never did anyone any harm");
        phrases.Add("Fools rush in where angels fear to tread");
        phrases.Add("Possession is nine tenths of the law");
        phrases.Add("It is the last straw that breaks the camel's back");
        phrases.Add("Whether you think you can, or you think you can't, you're right");
        phrases.Add("Let the dead bury the dead");
        phrases.Add("Preaching to the choir");
        phrases.Add("A watched man never plays");
        phrases.Add("Spare the rod and spoil the child");
        phrases.Add("Keep your friends close and your enemies closer");
        phrases.Add("Opportunity never knocks twice at any man's door");
        phrases.Add("Familiarity breeds contempt");
        phrases.Add("You've got to separate the wheat from the chaff");
        phrases.Add("Better wear out than rust out");
        phrases.Add("There is more than one way to skin a cat");
        phrases.Add("To the victor go the spoils");
        phrases.Add("Where there is muck there is brass");
        phrases.Add("(Only) time will tell");
        phrases.Add("Live to fight another day");
        phrases.Add("Power corrupts; absolute power corrupts absolutely");
        phrases.Add("Practice what you preach");
        phrases.Add("A ship in a harbour is safe, but that's not what a ship is for");
        phrases.Add("Milking the bull");
        phrases.Add("If it ain't broke, don't fix it");
        phrases.Add("Doubt is the beginning, not the end, of wisdom");
        phrases.Add("Whatever floats your boat");
        phrases.Add("A rolling stone gathers no moss");
        phrases.Add("What is sauce for the goose is sauce for the gander");
        phrases.Add("Count your blessings");
        phrases.Add("Strike while the iron is hot");
        phrases.Add("Life begins at forty");
        phrases.Add("He who lives by the sword, dies by the sword");
        phrases.Add("Never say never");
        phrases.Add("Why keep a dog and bark yourself?");
        phrases.Add("Too many cooks spoil the broth");
        phrases.Add("First things first");
        phrases.Add("What does not kill me makes me stronger");
        phrases.Add("Clothes make the man");
        phrases.Add("Wonders will never cease");
        phrases.Add("Kill two birds with one stone");
        phrases.Add("There is many a slip 'twixt cup and lip");
        phrases.Add("They that sow the wind shall reap the whirlwind");
        phrases.Add("It takes one to know one");
        phrases.Add("It needs a hundred lies to cover a single lie");
        phrases.Add("Do unto others as you would have them do unto you");
        phrases.Add("Careless talk costs lives");
        phrases.Add("If you cannot beat them, join them");
        phrases.Add("Beauty is only skin deep");
        phrases.Add("The apple does not fall far from the tree");
        phrases.Add("A picture is worth a thousand words");
        phrases.Add("See a penny and pick it up, all the day you will have good luck");
        phrases.Add("The good die young");
        phrases.Add("The darkest hour is just before the dawn");
        phrases.Add("Beggars cannot be choosers");
        phrases.Add("No guts, no glory");
        phrases.Add("Look before you leap");
        phrases.Add("The hand that rocks the cradle rules the world");
        phrases.Add("He who knows does not speak He who speaks does not know");
        phrases.Add("Let well alone");
        phrases.Add("It is a small world");
        phrases.Add("Genius is an infinite capacity for taking pains");
        phrases.Add("It is easy to be wise after the event");
        phrases.Add("Take care of the pennies, and the pounds will take care of themselves");
        phrases.Add("Never say die");
        phrases.Add("In for a penny, in for a pound");
        phrases.Add("Red sky at night shepherds delight; red sky in the morning, shepherds warning");
        phrases.Add("Knowledge is power, guard it well");
        phrases.Add("Every cloud has a silver lining");
        phrases.Add("It is all grist to the mill");
        phrases.Add("Memory is the treasure of the mind");
        phrases.Add("It is like juggling sand");
        phrases.Add("Do not cross the bridge till you come to it");
        phrases.Add("Out of sight, out of mind");
        phrases.Add("Do not cry over spilled milk");
        phrases.Add("You cannot make a silk purse from a sow's ear");
        phrases.Add("There ain't no such thing as a free lunch");
        phrases.Add("Empty vessels make the most noise");
        phrases.Add("Better safe than sorry");
        phrases.Add("As you sow so shall you reap");
        phrases.Add("If you have never seen the bottom of the tree, you cannot know how tall it stands");
        phrases.Add("It is never too late");
        phrases.Add("He who laughs last laughs longest");
        phrases.Add("Do not keep a dog and bark yourself");
        phrases.Add("Do not try to walk before you can crawl");
        phrases.Add("Well begun is half done");
        phrases.Add("Good fences make good neighbours");
        phrases.Add("Penny wise and pound foolish");
        phrases.Add("Only fools and horses work");
        phrases.Add("Do not rock the boat");
        phrases.Add("Live and let live");
        phrases.Add("Nothing ventured, nothing gained");
        phrases.Add("Lightning never strikes twice in the same place");
        phrases.Add("You can lead a horse to water, but you cannot make it drink");
        phrases.Add("Necessity is the mother of invention");
        phrases.Add("Many a true word is spoken in jest");
        phrases.Add("Cheats never prosper");
        phrases.Add("If you pay peanuts, you get monkeys");
        phrases.Add("Once the poison, twice the charm");
        phrases.Add("Kill the goose that lays the golden egg");
        phrases.Add("Love will find a way");
        phrases.Add("Let the buyer beware");
        phrases.Add("Fake it till you make it");
        phrases.Add("Love of money is the root of all evil");
        phrases.Add("It will be the same a hundred years hence");
        phrases.Add("Any port in a storm");
        phrases.Add("Misery loves company");
        phrases.Add("A journey of a thousand miles begins with a single step");
        phrases.Add("What is learnt in the cradle lasts to the tombs");
        phrases.Add("In the kingdom of the blind, the one eyed man is king");
        phrases.Add("Brevity is the soul of wit");
        phrases.Add("Fine words butter no parsnips");
        phrases.Add("Cut your coat according to your cloth");
        phrases.Add("Use it or lose it");
        phrases.Add("Shrouds have no pockets");
        phrases.Add("Cleanliness is next to godliness");
        phrases.Add("All you need is love");
        phrases.Add("Never put off until tomorrow what you can do today");
        phrases.Add("Life is what you make it");
        phrases.Add("Nine tailors make a man,");
        phrases.Add("Coffee and love taste best when hot");
        phrases.Add("Never give a sucker an even break");
        phrases.Add("One law for the rich and another for the poor");
        phrases.Add("One might as well throw water into the sea as to do a kindness to rogues");
        phrases.Add("Physician, heal thyself");
        phrases.Add("Manners maketh man");
        phrases.Add("Let the cat out of the bag");
        phrases.Add("There is no time like the present");
        phrases.Add("Up a creek without a paddle");
        phrases.Add("Success has many fathers, while failure is an orphan");
        phrases.Add("Right or wrong, my country");
        phrases.Add("Never speak ill of the dead");
        phrases.Add("Once bitten, twice shy");
        phrases.Add("When it rains it pours");
        phrases.Add("Tomorrow is another day");
        phrases.Add("When the going gets tough, the tough get going");
        phrases.Add("Love makes the world go around");
        phrases.Add("He who hesitates is lost");
        phrases.Add("A friend to everyone is a friend to no one");
        phrases.Add("The squeaky wheel gets the grease");
        phrases.Add("A watched kettle never boils");
        phrases.Add("Softly, softly, catchee monkey");
        phrases.Add("If it were not for hope the heart would break");
        phrases.Add("An army marches on its stomach");
        phrases.Add("Do not put off until tomorrow what you can do today");
        phrases.Add("Give him an inch and he will take a mile");
        phrases.Add("As a tree bends, so shall it grow");
        phrases.Add("Slow but sure");
        phrases.Add("A mill cannot grind with the water that is past");
        phrases.Add("Comparisons are odious");
        phrases.Add("Every little bit helps");
        phrases.Add("Slow and steady wins the race");
        phrases.Add("It is no use crying over spilt milk");
        phrases.Add("When life gives you lemons, make lemonade");
        phrases.Add("The road to Hell is paved with good intentions");
        phrases.Add("Let the punishment fit the crime");
        phrases.Add("Life is not all beer and skittles");
        phrases.Add("Seeing is believing");
        phrases.Add("One good turn deserves another");
        phrases.Add("The Devil looks after his own");
        phrases.Add("If at first you do not succeed, try, try again");
        phrases.Add("Give credit where credit is due");
        phrases.Add("Marriages are made in heaven");
        phrases.Add("There is no smoke without fire");
        phrases.Add("God helps those who help themselves");
        phrases.Add("False friends are worse than open enemies");
        phrases.Add("Might makes right");
        phrases.Add("Time is money");
        phrases.Add("Procrastination is the thief of time");
        phrases.Add("The proof of the pudding is in the eating");
        phrases.Add("Many hands make light work");
        phrases.Add("Bad news travels fast");
        phrases.Add("Islands depend on reeds, just as reeds depend on islands");
        phrases.Add("Even from a foe a man may learn wisdom");
        phrases.Add("Cold hands, warm heart");
        phrases.Add("Give the devil his due");
        phrases.Add("Do not put new wine into old bottles");
        phrases.Add("Do not throw pearls to swine");
        phrases.Add("Those who sleep with dogs will rise with fleas");
        phrases.Add("Uneasy lies the head that wears a crown");
        phrases.Add("Keep your powder dry");
        phrases.Add("All the world loves a lover");
        phrases.Add("Truth is stranger than fiction");
        phrases.Add("Fortune favours the brave");
        phrases.Add("Caesar's wife must be above suspicion");
        phrases.Add("It is better to travel hopefully than to arrive");
        phrases.Add("Mud sticks");
        phrases.Add("Fall seven times, stand up eight");
        phrases.Add("You cannot make bricks without straw");
        phrases.Add("East is east, and west is west and never the twain shall meet");
        phrases.Add("He who pays the piper calls the tune");
        phrases.Add("All is fair in love and war");
        phrases.Add("If anything can go wrong, it will");
        phrases.Add("Crime does not pay");
        phrases.Add("Better to reign in hell than serve in heaven");
        phrases.Add("Rome was not built in one day");
        phrases.Add("A dog is a man's best friend");
        phrases.Add("You cannot always get what you want");
        phrases.Add("Work expands so as to fill the time available");
        phrases.Add("No one can make you feel inferior without your consent");
        phrases.Add("There is no fool like an old fool");
        phrases.Add("Give a man rope enough and he will hang himself");
        phrases.Add("Easier said than done");
        phrases.Add("All work and no play makes Jack a dull boy");
        phrases.Add("No names, no pack-drill");
        phrases.Add("There is honour among thieves");
        phrases.Add("Out of the mouths of babes (and sucklings)");
        phrases.Add("A bird in the hand is worth two in the bush");
        phrases.Add("Laughter is the best medicine");
        phrases.Add("A penny saved is a penny earned");
        phrases.Add("The moon is made of green cheese");
        phrases.Add("Walk softly but carry a big stick");
        phrases.Add("There are two sides to every question");
        phrases.Add("Many a little makes a mickle");
        phrases.Add("Never look a gift horse in the mouth");
        phrases.Add("The old wooden spoon beats me down");
        phrases.Add("Two wrongs (do not) make a right");
        phrases.Add("Revenge is a dish best served cold");
        phrases.Add("If the shoe fits, wear it");
        phrases.Add("There is safety in numbers");
        phrases.Add("Actions speak louder than words");
        phrases.Add("You'll never get if you never go");
        phrases.Add("Walls have ears");
        phrases.Add("Do not let the bastards grind you down");
        phrases.Add("Man does not live by bread alone");
        phrases.Add("Every Jack has his Jill");
        phrases.Add("The age of miracles is past");
        phrases.Add("Every tide has its ebb");
        phrases.Add("Behind every great man, there is a great woman");
        phrases.Add("All is for the best in the best of all possible worlds");
        phrases.Add("The course of true love never did run smooth");
        phrases.Add("If you give a mouse a cookie, he'll always ask for a glass of milk");
        phrases.Add("Talk is cheap");
        phrases.Add("Prevention is better than cure");
        phrases.Add("Life is too short not to do something that matters");
        phrases.Add("Money demands care, you abuse it and it disappears");
        phrases.Add("No friends but the mountains");
        phrases.Add("Hope springs eternal");
        phrases.Add("Those who live in glass houses should not throw stones");
        phrases.Add("The more things change, the more they stay the same");
        phrases.Add("Never give advice unless asked");
        phrases.Add("Everything comes to those who wait");
        phrases.Add("A bad penny always turns up");
        phrases.Add("It is better to give than to receive");
        phrases.Add("Waste not, want not");
        phrases.Add("Charity begins at home");
        phrases.Add("To err is human, to forgive divine");
        phrases.Add("Another day, another dollar");
        phrases.Add("Stupid is as stupid does");
        phrases.Add("The comeback is greater than the setback");
        phrases.Add("Every dog has his day");
        phrases.Add("Give a dog a bad name and hang him");
        phrases.Add("It's Greek to me");
        phrases.Add("Walnuts and pears you plant for your heirs");
        phrases.Add("Enough is as good as a feast");
        phrases.Add("If the mountain will not come to Mohammed, then Mohammed must go to the mountain");
        phrases.Add("The child is the father of the man");
        phrases.Add("All roads lead to Rome");
        phrases.Add("First impressions are the most lasting");
        phrases.Add("Never let the sun go down on your anger");
        phrases.Add("Men are blind in their own cause");
        phrases.Add("Early to bed and early to rise makes a man healthy, wealthy and wise");
        phrases.Add("The best defence is a good offence");
        phrases.Add("From the sublime to the ridiculous is only a step");
        phrases.Add("Criss-cross, applesauce ");
        phrases.Add("Every man has his price");
        phrases.Add("Laugh before breakfast, cry before supper");
        phrases.Add("Boys will be boys");
        phrases.Add("Knock on wood");
        phrases.Add("Do not spend it all in one place");
        phrases.Add("He who makes a beast out of himself gets rid of the pain of being a man");
        phrases.Add("It takes a thief to catch a thief");
        phrases.Add("No rest for the wicked");
        phrases.Add("If you want a thing done well, do it yourself");
        phrases.Add("Make love not war");
        phrases.Add("A cat may look at a king");
        phrases.Add("There are none so deaf as those who will not hear");
        phrases.Add("A language is a dialect with an army and navy");
        phrases.Add("Forewarned is forearmed");
        phrases.Add("The bread never falls but on its buttered side");
        phrases.Add("Love is blind");
        phrases.Add("Where there is a will there is a way");
        phrases.Add("There is no such thing as bad publicity");
        phrases.Add("Every picture tells a story");
        phrases.Add("Do as I say, not as I do");
        phrases.Add("Less is more");
        phrases.Add("Some are more equal than others");
        phrases.Add("If it were a snake, it would have bit you");
        phrases.Add("A miss is as good as a mile");
        phrases.Add("All hands on deck");
        phrases.Add("What cannot be cured must be endured");
        phrases.Add("Better to remain silent and be thought a fool than to speak and remove all doubt");
        phrases.Add("See no evil, hear no evil, speak no evil");
        phrases.Add("It never rains but it pours");
        phrases.Add("All good things must come to an end");
        phrases.Add("Let sleeping dogs lie");
        phrases.Add("Finders keepers losers weepers");
        phrases.Add("The longest journey starts with a single step");
        phrases.Add("Do not wash your dirty linen in public");
    }
}
