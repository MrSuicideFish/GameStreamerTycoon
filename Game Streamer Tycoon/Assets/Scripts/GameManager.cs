using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GameState { MENU, PAUSE, GAME, MESSAGE }
    public GameState CurrentGameState;

    public GameObject StartGameScreen, InstructionScreen,
        PauseScreen, GameHUD, GameInfoHUD;

    string StreamerName;
    int Followers, Viewers;
    bool bGamePaused = false;

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

        //Set game state
        CurrentGameState = GameState.MENU;
    }

    void Update( )
    {
        //Can we pause?
        if ( GameHUD.activeInHierarchy || PauseScreen.activeInHierarchy )
        {
            if ( Input.GetKeyDown( KeyCode.Escape ) )
                PauseGame( );
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

        //Ask for player name
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
            StreamerName = newName;
        }
    }

    public void ShowGameMessage( string title, string dialog )
    {

    }
}
