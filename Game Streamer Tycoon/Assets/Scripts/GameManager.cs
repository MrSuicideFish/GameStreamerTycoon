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
    string StreamerName;
    int Followers, Viewers;
    bool bGamePaused = false, bGameStarted = false;

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
            if ( string.IsNullOrEmpty( StreamerName ) )
            {
                //Can't Continue
                return;
            }

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
        //Test message
        //ThrowGameMessage( "Testing", "This is test dialog." );

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

        //Show welcome screen

        //Add two followers

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

            Text winTitle = CurrentMessageWindow.transform.GetChild( 0 ).GetComponent<Text>( );
            Text winDialog = CurrentMessageWindow.transform.GetChild( 1 ).GetComponent<Text>( );

            winTitle.text = GameMessageList[ 0 ].Key;
            winDialog.text = GameMessageList[ 0 ].Value;
        }

    }
}