using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
    public GameObject StartGameScreen, InstructionScreen,
        PauseScreen, GameHUD, GameInfoHUD;

    /// <summary>
    /// Game Messages
    /// </summary>
    GameObject CurrentMessageWindow;
    //string - title, string - dialog
    List<KeyValuePair<string,string>> GameMessageList = new List<KeyValuePair<string, string>>( );

    /// <summary>
    /// Player Stats
    /// </summary>
    public string StreamerName { get; private set; }
    public int Followers { get; private set; }
    public int Viewers { get; private set; }
    bool bGamePaused = false, bGameStarted = false;
    public bool IsLive { get; private set; }
    public string[] Sponsors = new string[ 0 ];

    void Start( )
    {
        //Show the start game screen
        StartGameScreen.SetActive( true ); // ON
        InstructionScreen.SetActive( false );
        PauseScreen.SetActive( false );
        GameHUD.SetActive( false );
        GameInfoHUD.SetActive( false );

        //Set initial values
        Followers = 0;
        Viewers = 0;
        StreamerName = "";
        GameMessageList = new List<KeyValuePair<string, string>>( );

        //Set game state
        CurrentGameState = GameState.MENU;
        bGameStarted = false;
    }

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
                bSwitchingGameMessages = false;

            if ( string.IsNullOrEmpty( StreamerName ) )
            {
                //Can't Continue
                return;
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

        yield return new WaitForSeconds( 2 );
        AddFollowers( 2 );

        //Show welcome screen
        ThrowGameMessage( "New Followers", "Congratulations! You have your first followers! They're your parents but a follow is a follow in the world of streaming.\n As you gain followers, you will begin to notice a rise in opportunities coming your way." );
        ThrowGameMessage( "Going Live", "As a streamer, obviously you are required to <i>actually</i> stream. \n To do this, start/stop your stream by pressing the 'Go Live' button at the bottom of the screen." );

        yield return null;
    }

    /// <summary>
    /// Show Game instructions
    /// </summary>
    public void ShowStartScreen( )
    {
        StartGameScreen.SetActive( true );
        InstructionScreen.SetActive( false );
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

    public void ToggleLive( )
    {
        IsLive = !IsLive;

        if ( IsLive )
        {
            //show game list
            GameObject GameListWin = (GameObject)GameObject.Instantiate( Resources.Load( "GameListWindow" ), Vector3.zero, Quaternion.identity );
            GameListWin.transform.SetParent( GameObject.FindGameObjectWithTag( "Canvas" ).transform.GetChild( 0 ), false );
        }
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
        GameMessageList.Insert( 0, newMessage );
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
}