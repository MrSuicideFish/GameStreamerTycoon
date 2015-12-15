using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum ESRBRating
{
    Everyone,
    Teen,
    Mature,
    Adult
}

/// <summary>
/// Games
/// Rating system from 0 - 100
/// </summary>
public struct Game
{
    public string Title,
        Developer,
        Description;

    public float Price;

    public int CommunityRating,
        CriticRating,
        StreamerRating;

    public bool IsSponsored
    {
        get
        {
            return GameManager.Instance.Sponsors.Contains( Developer );
        }
    }

    public bool Owned
    {
        get
        {
            return GameManager.Instance.PlayerOwnedGames.Contains( this );
        }
    }

    public ESRBRating EsrbRating;

    public static Game CreateGame( )
    {
        Game newGame = GameService.GetRandomGame( );

        //Give game random rating
        int ratingIdx = Random.Range( 1, 4 );
        newGame.EsrbRating = ( ESRBRating )ratingIdx;

        newGame.CommunityRating = Random.Range( 1, 100 );
        newGame.CriticRating = Random.Range( 1, 100 );
        newGame.StreamerRating = Random.Range( 1, 100 );

        //Determine game's price
        var sectionA = ( float )( ( float )newGame.CommunityRating / 100f ) * 0.3f;
        var sectionB = ( float )( ( float )newGame.CriticRating / 100f ) * 2.0f;
        var sectionC = ( float )( ( float )newGame.StreamerRating / 100f ) * 0.6f;

        sectionA = Mathf.Round( ( sectionA ) * 100 ) / 100;
        sectionB = Mathf.Round( ( sectionB ) * 100 ) / 100;
        sectionC = Mathf.Round( ( sectionC ) * 100 ) / 100;

        newGame.Price = sectionA * sectionB * sectionC;

        //Limit games to $60 max and $5 min
        newGame.Price *= 900;
        newGame.Price = Mathf.Clamp( newGame.Price, 5.00f, 60.00f );
        newGame.Price = Mathf.Round( newGame.Price );

        return newGame;
    }
}

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton
    /// </summary>
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if ( instance == null )
            {
                instance = GameObject.FindGameObjectWithTag( "Canvas" ).GetComponent<GameManager>( );
            }

            return instance;
        }
    }

    /// <summary>
    /// Game State
    /// </summary>
    public enum GameState { MENU, PAUSE, GAME, MESSAGE }
    public GameState CurrentGameState;

    /// <summary>
    /// Game System Objects
    /// </summary>
    public GameObject StartGameScreen, InstructionScreen, Marketplace,
        PauseScreen, GameHUD, GameInfoHUD, BtnLive, BtnShop;

    public InputField WebsiteHeaderText;

    /// <summary>
    /// Game Messages
    /// </summary>
    public bool bShowingMessage = false;
    GameObject CurrentMessageWindow;
    //string - title, string - dialog
    List<KeyValuePair<string,string>> GameMessageList = new List<KeyValuePair<string, string>>( );

    /// <summary>
    /// Player Stats
    /// </summary>

    public enum STATISTIC_TYPE { SOCIAL, VERBAL, SKILL, HARDWARE };
    public int SkillPoints { get; private set; }
    public int SocialLevel,
                VerbalLevel,
                SkillLevel,
                HardwareLevel;

    //Career
    public string StreamerName { get; private set; }
    public float Money { get; private set; }
    public int Followers { get; private set; }
    public int BestViewers { get; private set; }
    public int Viewers { get; private set; }
    public int Likes { get; private set; }
    bool bGamePaused = false, bGameStarted = false;
    public bool IsLive { get; private set; }
    public string[] Sponsors = new string[ 0 ];

    public bool bRealtimePaused = false;

    public bool bPlayerHasFirstLike = false;
    public bool bPlayerHasFirstWatcher = false;

    public Game CurrentGame;
    public List<Game> PlayerOwnedGames;

    ///Session variables
    ///--
    public float StartCash { get; private set; }
    public float StartFollowers { get; private set; }
    List<Game> GameStreamHistory = new List<Game>( );

    /// <summary>
    /// Live Variables
    /// </summary>
    public float TotalUpTime { get; private set; }
    public float CurrentStreamTime { get; private set; }
    public float TotalStreamTime { get; private set; }

    int FollowersWatchingStream = 0, 
        WatchersLikingStream = 0;

    void Start( )
    {
        //Initiate data
        GameService.InitDocument( );

        //Show the start game screen
        StartGameScreen.SetActive( true ); // ON
        InstructionScreen.SetActive( false );
        Marketplace.SetActive( false );
        PauseScreen.SetActive( false );
        GameHUD.SetActive( false );
        GameInfoHUD.SetActive( false );

        BtnLive.SetActive( false );
        BtnShop.SetActive( false );

        //Set initial values
        SocialLevel = 0;
        VerbalLevel = 0;
        SkillLevel = 0;
        HardwareLevel = 0;

        Followers = 0;
        Viewers = 0;
        Money = 60.00f;
        StreamerName = "";
        TotalStreamTime = 16.0f;
        StartCash = 0;
        GameMessageList = new List<KeyValuePair<string, string>>( );
        PlayerOwnedGames = new List<Game>( );

        //Set game state
        CurrentGameState = GameState.MENU;
        bGameStarted = false;
    }

    float TimeForLastUpdate = 0.0f;
    bool bCanUpdateStreamStats = false;
    void Update( )
    {
        //Can we pause?
        if ( GameHUD.activeInHierarchy || PauseScreen.activeInHierarchy )
        {
            if ( Input.GetKeyDown( KeyCode.Escape ) )
                PauseGame( );
        }

        if ( CurrentGameState == GameState.GAME )
        {
            //Handle message screens
            //Is there a message currently displaying?
            if ( CurrentMessageWindow == null )
            {
                //If not, is there supposed to be?
                if ( GameMessageList.Count > 0 )
                {
                    bSwitchingGameMessages = true;
                    ShowNextGameMessage( );
                }
            }
            else
            {
                bSwitchingGameMessages = false;
            }

            if ( string.IsNullOrEmpty( StreamerName ) )
            {
                //Can't Continue
                return;
            }

            if ( IsLive && !CurrentGame.Equals( default( Game ) ) && CurrentGameState == GameState.GAME && !bRealtimePaused)
            {
                CurrentStreamTime += Time.fixedDeltaTime;

                if ( Mathf.Round( CurrentStreamTime % 1 ) == 0 && bCanUpdateStreamStats )
                {
                    bool bStreamAlmostOver = false;

                    //Get total reach of consumers
                    float fanbaseScale = Random.Range( 17, 35 ) + Followers + CurrentGame.CommunityRating;
                    
                    //grasp a percent of consumers
                    float potentialShare = 0.2f + ( fanbaseScale * ( ( SocialLevel + VerbalLevel + SkillLevel + HardwareLevel ) / 15 ) );
                    float finalShare = fanbaseScale * potentialShare;
                    finalShare = Mathf.Round( finalShare ) / 3;

                    //Begin losing viewers towards end of stream
                    if ( CurrentStreamTime / TotalStreamTime > 0.5f )
                    {
                        finalShare -= ( int )( ( CurrentStreamTime / TotalStreamTime ) * Random.Range( 5f, 10f ) ); //remove time folks
                        bStreamAlmostOver = true;
                    }
                    else
                    {
                        //Add timescale
                        finalShare += ( int )( ( CurrentStreamTime / TotalStreamTime ) * Random.Range( 3f, 7f ) ); //remove time folks
                    }

                    //Add duplicate penalty
                    finalShare -= GameStreamHistory.Count( x => x.Title == CurrentGame.Title ) * 5;
                    if ( Viewers + finalShare < 0 ) finalShare = 0; // limit

                    //Add watchers this update
                    AddWatchers( ( int )finalShare );

                    //Determine what perc of viewers this update like the stream
                    if ( !bStreamAlmostOver || (int)finalShare > 0 )
                    {
                        float retentionPerc = potentialShare + ( CurrentGame.CommunityRating / 100 );
                        int newLikes = Mathf.RoundToInt( finalShare * retentionPerc );

                        AddLikes( newLikes );
                    }

                    bCanUpdateStreamStats = false;
                }
                else if ( Mathf.Round( CurrentStreamTime % 1 ) != 0 )
                {
                    bCanUpdateStreamStats = true;
                }

                if ( CurrentStreamTime >= TotalStreamTime )
                {
                    //Determine new followers
                    float retentionPerc = 0.2f + ( ( SocialLevel + VerbalLevel + SkillLevel + HardwareLevel ) / 15 );
                    retentionPerc += ( Likes / ( BestViewers != 0 ? BestViewers : 1 ) );

                    float newFollowers = Likes * retentionPerc;
                    AddFollowers( Mathf.RoundToInt( newFollowers ) );

                    ToggleLive( );
                }
            }
        }
    }

    public void BeginStream( Game gameToStream )
    {
        StartCash = Money;
        StartFollowers = Followers;
        Likes = 0;
        CurrentStreamTime = 0;
        Viewers = 0;
        BestViewers = 0;
        TimeForLastUpdate = 0;
        CurrentGame = gameToStream;
    }

    public void EndStream( )
    {
        GameStreamHistory.Add( CurrentGame );

        StartCash = 0;
        CurrentStreamTime = 0;
        TimeForLastUpdate = 0;
    }

    /// <summary>
    /// Start game and go to game hud & screen
    /// </summary>
    public void StartGame( )
    {
        //show the game hud
        StartGameScreen.SetActive( false ); 
        InstructionScreen.SetActive( false );
        PauseScreen.SetActive( false );
        GameHUD.SetActive( true );// ON
        GameInfoHUD.SetActive( true );// ON

        CurrentGameState = GameState.GAME;

        StartCoroutine( StartGameCallback( ) );
    }

    IEnumerator StartGameCallback( )
    {
        ThrowGameMessage( "Welcome", "So you're new to the streaming world, huh? \n Well, you're going to have to learn the ropes as you go along. But first thing's first, what's your <color=#00ffffff><b><i>StreamerTag</i></b></color>?" );
        
        while ( CurrentMessageWindow != null ) yield return null;

        //Ask for player name
        GameObject playerNameWin = null;
        while ( StreamerName == "" )
        {
            if ( playerNameWin == null )
            {
                playerNameWin = ( GameObject )GameObject.Instantiate( ( GameObject )Resources.Load( "StreamTagWin" ), Vector3.zero, Quaternion.identity );
                playerNameWin.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform.GetChild( 0 ), false );
                playerNameWin.transform.GetChild( 0 ).GetComponent<InputField>( ).ActivateInputField( );
            }

            yield return null;
        }

        WebsiteHeaderText.text = WebsiteHeaderText.text + StreamerName;

        //Remove player name window
        GameObject.Destroy( playerNameWin );

        AddFollowers( 2 );

        while ( bShowingMessage ) yield return null;

        //Show welcome screen
        ThrowGameMessage( "New Followers", "Congratulations! You have your first followers! They're your parents but a follow is a follow in the world of streaming.\n As you gain followers, you will begin to notice a rise in opportunities coming your way." );

        while ( bShowingMessage ) yield return null;

        ThrowGameMessage( "Getting Games", "Games must be purchased on the <color=#ff00ffff>Vapor Marketplace</color> before you can stream. Click the  button at the bottom-right corner of the screen to view the latest games on the market." );
        Marketplace.GetComponent<MarketplaceWindow>( ).RefreshGames( );
        BtnShop.SetActive( true );

        while ( ( bShowingMessage || PlayerOwnedGames.Count == 0 ) || ( PlayerOwnedGames.Count > 0 && Marketplace.activeInHierarchy ) ) yield return null;

        ThrowGameMessage( "Going Live", "As a streamer, obviously you are required to <i>actually</i> stream. To do this, start/stop your stream by pressing the 'Go Live' button at the bottom of the screen." );
        BtnLive.SetActive( true );

        yield return null;
    }

    /// <summary>
    /// Show Game instructions
    /// </summary>
    public void ShowStartScreen( )
    {
        StartGameScreen.SetActive( true );
        InstructionScreen.SetActive( false );
        Marketplace.SetActive( false );
        PauseScreen.SetActive( false );
        GameHUD.SetActive( false );
        GameInfoHUD.SetActive( false );

        CurrentGameState = GameState.MENU;
    }

    /// <summary>
    /// Show Game instructions
    /// </summary>
    public void ShowInstructions( )
    {
        StartGameScreen.SetActive( false );
        InstructionScreen.SetActive( true );
        PauseScreen.SetActive( false );
        GameHUD.SetActive( false );
        GameInfoHUD.SetActive( false );
    }

    /// <summary>
    /// Pause / unpause the game
    /// </summary>
    public void PauseGame( )
    {
        bGamePaused = !bGamePaused;

        StartGameScreen.SetActive( false );
        InstructionScreen.SetActive( false );
        PauseScreen.SetActive( bGamePaused );
        GameHUD.SetActive( !bGamePaused );
        GameInfoHUD.SetActive( !bGamePaused );

        CurrentGameState = bGamePaused ? GameState.PAUSE : GameState.GAME;
    }

    public void GoToMarketplace( )
    {
        Marketplace.SetActive( true );
        GameInfoHUD.SetActive( false );
    }

    public void LeaveMarketplace( )
    {
        Marketplace.SetActive( false );
        GameHUD.SetActive( true );
        GameInfoHUD.SetActive( true );
    }

    public delegate void FollowersChanged( int num );
    public event FollowersChanged OnFollowersGained;
    public event FollowersChanged OnFollowersLost;
    public void AddFollowers( int numOfNewFollowers )
    {
        Followers += numOfNewFollowers;

        if ( Followers % ( 10 * ( 1 + ( SkillLevel + SocialLevel + VerbalLevel + HardwareLevel ) ) ) == 0 )
        {
            GivePlayerSkillPoint( );
        }

        if ( Followers > 150 && Sponsors.Length == 0 )
        {
            Sponsors = new string[ 1 ];
            Sponsors[ 0 ] = "GA Sports";

            ThrowGameMessage( "Sponsor", "Hi, " + GameManager.instance.StreamerName + "! We at GA Sports would love to sponsor you! Unforunately, this feature wasn't completed in time so we'll just say you're on our team." );
        }

        if ( OnFollowersGained != null )
        {
            OnFollowersGained.Invoke( numOfNewFollowers );
        }
    }

    public void RemoveFollowers( int numOfLostFollowers )
    {
        Followers -= numOfLostFollowers;

        if ( OnFollowersLost != null )
        {
            OnFollowersLost.Invoke( numOfLostFollowers );
        }
    }

    public delegate void ViewersChanged( int num );
    public event ViewersChanged OnViewersGained;
    public event ViewersChanged OnViewersLost;
    public void AddWatchers( int numOfNewWatchers )
    {
        Viewers += numOfNewWatchers;

        if ( Viewers > BestViewers )
        {
            BestViewers = Viewers;
        }

        if ( Viewers > 0 && !bPlayerHasFirstWatcher )
        {
            bRealtimePaused = true;
            ThrowGameMessage( "Viewers", "Noice! You've gained your first few viewers. As you raise your audience, you raise the likelihood that someone may actually want to follow you." );
            bPlayerHasFirstWatcher = true;
        }

        if ( OnViewersGained != null )
        {
            OnViewersGained.Invoke( numOfNewWatchers );
        }
    }

    public void RemoveWatchers( int numOfLostWatchers )
    {
        Viewers -= numOfLostWatchers;

        if ( OnViewersLost != null )
        {
            OnViewersLost.Invoke( numOfLostWatchers );
        }
    }

    public delegate void LikesChanged( int num );
    public event LikesChanged OnLikeGained;
    public event LikesChanged OnLikeLost;
    public void AddLikes( int numOfNewLikes )
    {
        Likes += numOfNewLikes;

        if ( OnLikeGained != null )
        {
            OnLikeGained.Invoke( numOfNewLikes );
        }
    }

    public void RemoveLikes( int numOfLostLikes )
    {
        Likes -= numOfLostLikes;

        if ( OnLikeLost != null )
        {
            OnLikeLost.Invoke( numOfLostLikes );
        }
    }

    public delegate void MoneyChanged( int amount );
    public event MoneyChanged OnMoneyEarned;
    public void AddMoney( int amount )
    {
        Money += amount;

        if ( OnMoneyEarned != null )
        {
            OnMoneyEarned.Invoke( amount );
        }
    }

    public void GivePlayerSkillPoint( )
    {
        bRealtimePaused = true;
        ThrowGameMessage( "Skill Point", "Congradulations! You've unlocked a skill point! Use skill points to upgrade your streamer stats and become more socially interesting. You can view your stats in the window on the right side of the sceen." );
        SkillPoints++;
    }

    public void ToggleLive( )
    {
        IsLive = !IsLive;

        if ( IsLive )
        {
            //show game list
            GameObject GameListWin = (GameObject)GameObject.Instantiate( Resources.Load( "GameListWindow" ), Vector3.zero, Quaternion.identity );
            GameListWin.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform, false );
        }
        else
        {
            //Show post live window
            GameObject PostLiveWin = ( GameObject )GameObject.Instantiate( Resources.Load( "PostLiveWindow" ), Vector3.zero, Quaternion.identity );
            PostLiveWin.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform, false );

            PostLiveWin.GetComponent<PostLiveWindow>( ).Initialize( );

            EndStream( );
        }

        BtnShop.GetComponent<Button>( ).interactable = !IsLive;
    }

    public void SetPlayerName( string newName )
    {
        if ( !string.IsNullOrEmpty( newName ) )
        {
            Instance.StreamerName = newName;
        }
    }

    public void ThrowGameMessage( string title, string dialog )
    {
        //Create new message
        KeyValuePair<string, string> newMessage = new KeyValuePair<string, string>( title, dialog );

        //Push message to top of stack
        GameMessageList.Add( newMessage );
    }

    bool bSwitchingGameMessages = false;
    public void ShowNextGameMessage( )
    {
        if ( CurrentMessageWindow == null )
        {
            //Create new window
            CurrentMessageWindow = ( GameObject )GameObject.Instantiate( ( GameObject )Resources.Load( "DialogWindow" ), Vector3.zero, Quaternion.identity );
            CurrentMessageWindow.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform, false );

            Text winTitle = CurrentMessageWindow.transform.GetChild( 0 ).GetComponent<Text>( );
            Text winDialog = CurrentMessageWindow.transform.GetChild( 1 ).GetComponent<Text>( );

            winTitle.text = GameMessageList[ 0 ].Key;
            winDialog.text = GameMessageList[ 0 ].Value;

            bShowingMessage = true;

            if ( GameMessageList.Count > 1 )
            {
                //Remove this message from the list
                GameMessageList.RemoveAt( 0 );

                //Shift list down
                for ( int i = 1; i < GameMessageList.Count - 1; i++ )
                {
                    GameMessageList[ i - 1 ] = GameMessageList[ i ];
                }
            }
            else
            {
                //Remove this message from the list
                GameMessageList.RemoveAt( 0 );
            }
        }
    }

    public static int RatingToWidth( int rating, int maxWidth )
    {
        float ratingPerc = ( float )rating / 100f;
        int val = ( int )( ( float )maxWidth * ratingPerc );
        return val;
    }

    public void CloseMessage( )
    {
        bShowingMessage = false;
    }

    public void PurchaseGame( Game newGame )
    {
        //If we don't already own this item
        if ( !newGame.Owned )
        {
            GameManager.Instance.PlayerOwnedGames.Add( newGame );
            Money -= newGame.Price;
        }
    }

    public void IncrementStat( STATISTIC_TYPE statToInc )
    {
        if ( SkillPoints > 0 )
        {
            switch ( statToInc )
            {
                case STATISTIC_TYPE.SOCIAL:
                    SocialLevel++;
                    break;
                case STATISTIC_TYPE.VERBAL:
                    VerbalLevel++;
                    break;
                case STATISTIC_TYPE.SKILL:
                    SkillLevel++;
                    break;
                case STATISTIC_TYPE.HARDWARE:
                    HardwareLevel++;
                    break;
            }

            SkillPoints--;
        }
    }
}