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
        Game newGame = new Game( );
        newGame.Title = "SomeTitle";
        newGame.Developer = "SomeDeveloper";
        newGame.Description = "Some Description Here";

        //Give game random rating
        int ratingIdx = Random.Range( 1, 4 );
        newGame.EsrbRating = ( ESRBRating )ratingIdx;

        //Give game random scores
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
    //Personal
    public int Skill { get; private set; }
    public int Charisma { get; private set; }
    public int Intuition { get; private set; }

    //Career
    public string StreamerName { get; private set; }
    public float Money { get; private set; }
    public int Followers { get; private set; }
    public int BestViewers { get; private set; }
    public int Viewers { get; private set; }
    bool bGamePaused = false, bGameStarted = false;
    public bool IsLive { get; private set; }
    public string[] Sponsors = new string[ 0 ];

    public Game CurrentGame;
    public List<Game> PlayerOwnedGames;

    ///Session variables
    ///--
    public float StartCash { get; private set; }
    public float StartFollowers { get; private set; }

    /// <summary>
    /// Live Variables
    /// </summary>
    public float TotalUpTime { get; private set; }
    public float CurrentStreamTime { get; private set; }
    public float TotalStreamTime { get; private set; }

    void Start( )
    {
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
        Skill = 0;
        Charisma = 0;
        Intuition = 0;

        Followers = 36;
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

            if ( IsLive && !CurrentGame.Equals( default( Game ) ) && CurrentGameState == GameState.GAME )
            {
                CurrentStreamTime += Time.fixedDeltaTime;

                //Usable ratings
                var communityValue = 0.1f * ( float )( ( float )CurrentGame.CommunityRating / 100f );
                var criticValue = 0.1f * ( float )( ( float )CurrentGame.CriticRating / 100f );
                var streamerValue = 0.1f * ( float )( ( float )CurrentGame.StreamerRating / 100f );

                //Determine update time
                var nextUpdateTime = TimeForLastUpdate + ( Random.Range( 4, 8 ) - ( ( ( Charisma + Intuition + Skill ) / 15 ) * Followers ) );
                if ( CurrentStreamTime > nextUpdateTime )
                {
                    //Update watchers
                    var gameWatcherScore = ( ( ( float )CurrentGame.CommunityRating / 100f ) * 0.7f ) + ( ( ( float )CurrentGame.CriticRating / 100f) * 0.5f );
                    var popularityPenalty = 1 - ( ( float )Followers / ( ( float )CurrentGame.CommunityRating + ( float )CurrentGame.CriticRating + ( float )CurrentGame.StreamerRating ) );
                    var demandBonus = ( ( ( float )CurrentGame.StreamerRating + ( float )CurrentGame.CommunityRating ) / 200f );
                    var popularityDemandOffset = demandBonus + popularityPenalty;
                    gameWatcherScore += popularityDemandOffset;

                    AddWatchers( 1 + ( int )gameWatcherScore );

                    //Update followers
                    //AddFollowers( 1 + ( int )( ( ( ( Charisma * 0.7f ) + ( Skill * 0.1f ) + ( Intuition * 0.3f ) * 0.5f ) ) + ( Sponsors.Length * 1.3f ) ) );

                    //Update donations
                    Money += ( 0.63f * Viewers ) + Sponsors.Length * ( Sponsors.Length * 5 );
                    Money = Mathf.Round( Money * 100 ) / 100;

                    if ( Viewers > BestViewers )
                    {
                        BestViewers = Viewers;
                    }

                    TimeForLastUpdate = CurrentStreamTime;
                }

                if ( CurrentStreamTime >= TotalStreamTime )
                {
                    ToggleLive( );
                }
            }
        }
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

        //Remove player window
        GameObject.Destroy( playerNameWin );

        AddFollowers( 2 );

        while ( bShowingMessage ) yield return null;

        //Show welcome screen
        ThrowGameMessage( "New Followers", "Congratulations! You have your first followers! They're your parents but a follow is a follow in the world of streaming.\n As you gain followers, you will begin to notice a rise in opportunities coming your way." );

        while ( bShowingMessage ) yield return null;

        ThrowGameMessage( "Getting Games", "Games must be purchased on the <color=#800080ff>Smoke Marketplace</color> before you can stream. Click the  button at the bottom-right corner of the screen to view the latest games on the market." );
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

    public void ToggleLive( )
    {
        IsLive = !IsLive;

        if ( IsLive )
        {

            //show game list
            GameObject GameListWin = (GameObject)GameObject.Instantiate( Resources.Load( "GameListWindow" ), Vector3.zero, Quaternion.identity );
            GameListWin.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform.GetChild( 0 ), false );
        }
        else
        {
            CurrentGame = default( Game );

            //Show post live window
            GameObject PostLiveWin = ( GameObject )GameObject.Instantiate( Resources.Load( "PostLiveWindow" ), Vector3.zero, Quaternion.identity );
            PostLiveWin.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform.GetChild( 0 ), false );
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
            CurrentMessageWindow.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform.GetChild( 0 ), false );

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

    public void BeginStream( Game gameToStream )
    {
        StartCash = Money;
        StartFollowers = Followers;
        CurrentStreamTime = 0;
        BestViewers = 0;
        TimeForLastUpdate = 0;
        CurrentGame = gameToStream;
    }
}