using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PostLiveWindow : MonoBehaviour
{
    public Text GameNameTitle, 
        FollowersGained,
        TotalWatchers,
        TotalDonations,
        CommentA, CommentB, CommentC;

    public Button BtnContinue, BtnPrevious;

    public GameObject PanelStats, PanelComments;

    Game GameToResolve;

#region names
    string[] Usernames = 
    {
        "gindalton",
        "mustyslightly",
        "factlichen",
        "centaurushabit",
        "dimwittedrighteous",
        "harrasskylark",
        "monstercalm",
        "nitrogenscratch",
        "rustlespider",
        "discesker",
        "unbecomingqueasy",
        "fragilelashed",
        "describebeach",
        "wrongsturdy",
        "fratlack",
        "embedmischief",
        "pinsmemory",
        "covetgasbag",
        "patsydislike",
        "flocksepsis",
        "moldycrash",
        "aloofnervosa",
        "geologystriker",
        "belovedpoised",
        "mousellineremnant",
        "detailsforever",
        "responddame",
        "inspectbudget",
        "huhtinkle",
        "speakingskiver",
        "simpsonsdelay",
        "glandsadmiration",
        "rhymebum",
        "obesechangtse",
        "peppercervical",
        "cranniestrust",
        "turdiformchamois",
        "waalsclimax",
        "resistantsailing",
        "cankerdoughnut",
        "matrontortellini",
        "fabulousathlete",
        "blockpowdered",
        "swayunicyclist",
        "variantbloody",
        "shooterglitches",
        "unnaturalmousse",
        "quitlug",
        "creamsovary",
        "fishifiedcenozoic",
        "mapostulate",
        "friendlyprompting",
        "skimmedwaltz",
        "outlookcrashing",
        "potterydizziness",
        "plancasell",
        "hittingtransfer",
        "gearscuts",
        "meniscusplacebo",
        "tissuegudgeon",
        "jacketflame",
        "instinctresources",
        "flopjukebox",
        "handygreyhound",
        "cookingbaking",
        "figuremutually",
        "selfiessatisfy",
        "intriguemarble",
        "weatherdrunken",
        "gastropublargo",
        "oddstwigs",
        "fusionglitch",
        "apparentdo",
        "baileytwelve",
        "dumplingskit",
        "steamlolly",
        "occiputcolic",
        "bloodyredwing",
        "ulnaexacting",
        "splitrelativity",
        "yearlingreporter",
        "chestnutsnice",
        "muonvalhrona",
        "myeloidstale",
        "zonkedvouch",
        "intoglamorous",
        "muttontiger",
        "selectcherish",
        "puzzlingshard",
        "concepttottering",
        "mediocreass",
        "langedgoody",
        "orneryspelt",
        "pancettamustering",
        "sleepingmoorhen",
        "tipnod",
        "ovenbirdfoggy",
        "cracklingchorizo",
        "meatworking",
        "cockamamieinfantile",
        "acceleratorvirgo",
        "functionalsplash",
        "nutritiouschef",
        "oilspolygon",
        "gnarledopener",
        "ununoctiumrestraint",
        "exclusionadmin",
        "peacockshrill",
        "diligentspores",
        "smilingcheddars",
        "stunningunmuzzled",
        "taxidrivercoronet",
        "graniticpotassium",
        "carpacciolidar",
        "cabjealous",
        "welcomevalley",
        "scholarsenses",
        "angularscoopula",
        "isomercrushing",
        "siegeyoke",
        "jellyfishweb",
        "idiotpulling",
        "boilingcosmogony",
        "talkingparchment",
        "widerwhimbrel",
        "lotteryphenomenon",
        "thoriummatrices",
        "generousregulus",
        "typewriterworm",
        "henryannoy",
        "pribblinghelvetica",
        "shipsspilling",
        "lugcreatures",
        "carefulhandle",
        "ticketsefferent",
        "infinitycuillin",
        "rabidoption",
        "hygienistmedical",
        "anvilseamstress",
        "saneunlawful",
        "nissanzeolite",
        "puncturedzap",
        "foldingperky",
        "redstartlustful",
        "externalzebra",
        "bromineace",
        "smashingwheatear",
        "soundinanarchist",
        "lhotseplank",
        "kyanitefulfilled",
        "lunationdevilish",
        "quackcockalorum",
        "weighrecover",
        "purrreferred",
        "hullabaloopropose",
        "rosaceawisp",
        "pickledgrit",
        "backpackbodmin",
        "dollopcongolese",
        "bassmagenta",
        "moonsash",
        "weeklynodules",
        "dinnerposing",
        "grillatomic",
        "dimpledpungent",
        "hoosegowslurp",
        "metersiteinside",
        "scholarlyscreeching",
        "unhealthysupplier",
        "saucybugbear",
        "sturdybountiful",
        "rackedrake",
        "furnacezygomatic",
        "abrasivefollowing",
        "varcharbilled",
        "offbeatbrave",
        "activexwiggly",
        "anyasparagus",
        "punishmentguineafowl",
        "glitchescrampons",
        "boyishrefractor",
        "ignorantfeigned",
        "parallelmodular",
        "stainedrelax",
        "epeesinus",
        "oxidantagent",
        "pistontomatoe",
        "equipmentrepulsive",
        "bikelattice",
        "flummoxhatching",
        "spatulalozenge",
        "gatewaywaxwing",
        "quadrateyrar",
        "gaiafornax",
        "vacationslang",
        "importantwrong",
        "chertprotect",
        "jerksyncline",
        "fluxwhinchat",
        "outertwinning",
    };
#endregion

    string[] NegSocial = 
    {
        "Speak up! We can't hear you over the game.",
        "This game is actually pretty cool, but the streamer makes it boring.",
        "This streamer is sooo annoying."
    };

    string[] PosSocial = 
    {
        "Hahaha! Great impressions!",
        "Terrible game, but " + GameManager.Instance.StreamerName + " makes it hilarious!",
        "I would love to see " + GameManager.Instance.StreamerName + " drop in on DewDeeGuy's channel!"
    };

    string[] NegSkill = 
    {
        "You're bad at this.",
        "Would be better if they were actually good at it.",
        "Man, this is painful."
    };

    string[] PosSkill = 
    {
        "Them skills!",
        GameManager.Instance.StreamerName + " 4 MLG LOLZ",
        "Turn off the hax bro"
    };

    string[] PosCommunity =
    {
        "Definitely buying this game!",
        "Great game!!",
        "Just got this game on Vapor, it's amazing!",
        "Will trade BattleToads for this game, anyone?",
        "Gotta get this game"
    };

    string[] NegCommunity =
    {
        "Awful game",
        "Waste of time",
        "Why am I watching this?",
        "People actually buy this?"
    };

    string[] CommentList;

    int MoneyEarned = 0;

    void Start( )
    {
        GameToResolve = GameManager.Instance.CurrentGame;
        GameNameTitle.text = GameManager.Instance.CurrentGame.Title;
        FollowersGained.text = "Followers Gained: " + ( GameManager.Instance.Followers - GameManager.Instance.StartFollowers ).ToString( );
        TotalWatchers.text = "Total Watchers: " + GameManager.Instance.BestViewers.ToString( );
        TotalDonations.text = "Total Donations: $" + ( GameManager.Instance.Money - GameManager.Instance.StartCash ).ToString( );

        //Determine comments
        CommentList = new string[ GameManager.Instance.BestViewers > 3 ? 3 : GameManager.Instance.BestViewers ];

        CommentA.text += GameToResolve.CommunityRating > 40 ? PosCommunity[ Random.Range( 0, 2 ) ] : NegSkill[ Random.Range( 0, 2 ) ] + '"' + "-" + Usernames[Random.Range(0, Usernames.Length - 1)];
        CommentB.text += GameToResolve.CriticRating > 60 ? PosSocial[ Random.Range( 0, 2 ) ] : NegCommunity[ Random.Range( 0, 2 ) ] + '"' + "-" + Usernames[Random.Range(0, Usernames.Length - 1)];
        CommentC.text += GameToResolve.StreamerRating> 30 ? PosCommunity[ Random.Range( 0, 2 ) ] : NegSocial[ Random.Range( 0, 2 ) ] + '"' + "-" + Usernames[Random.Range(0, Usernames.Length - 1)];

        //Give player cash
        float potentialEarnings = ( 2f * ( float )GameManager.Instance.Likes ) + ( 1.4f * ( float )GameManager.Instance.Viewers );
        potentialEarnings *= ( GameManager.Instance.Followers - GameManager.Instance.StartFollowers );
        GameManager.Instance.AddMoney( ( int )potentialEarnings );

        GoToStatistics( );
    }

    public void GoToStatistics( )
    {
        PanelStats.SetActive( true );
        PanelComments.SetActive( false );

        BtnContinue.transform.GetChild( 0 ).GetComponent<Text>( ).text = "Continue";
        BtnContinue.gameObject.SetActive( true );
        BtnPrevious.gameObject.SetActive( false );
    }

    public void GoToComments( )
    {
        if ( !PanelComments.activeInHierarchy )
        {
            PanelStats.SetActive( false );
            PanelComments.SetActive( true );

            BtnContinue.transform.GetChild( 0 ).GetComponent<Text>( ).text = "Close";
            BtnContinue.gameObject.SetActive( true );
            BtnPrevious.gameObject.SetActive( true );
        }
        else
        {
            GameObject.Destroy( gameObject );
        }
    }
}