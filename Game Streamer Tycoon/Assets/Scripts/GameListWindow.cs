using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameListWindow : ModalWindow
{
    public GameObject ListPanel;
    public Button BtnSelect;

    GameObject SelectedPanel;
    GameObject[] GamePanels;

    void Start( )
    {
        //Load up player's games
        GamePanels = new GameObject[ GameManager.Instance.PlayerOwnedGames.Count ];

        //Set panel length
        ListPanel.GetComponent<RectTransform>( ).SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, ( GamePanels.Length * 200 ) + 40 );

        //Add the panels to the list
        for ( int i = 0; i < GamePanels.Length; i++ )
        {
            GamePanels[ i ] = ( GameObject )GameObject.Instantiate( Resources.Load( "GamePanel" ), Vector3.zero, Quaternion.identity );
            GamePanels[ i ].transform.SetParent( ListPanel.transform, false );

            RectTransform panelRect = GamePanels[ i ].GetComponent<RectTransform>( );

            //Position panel
            panelRect.anchoredPosition = new Vector2( -23.33f, -25 - ( 200 * i ) );

            GamePanel panel = GamePanels[ i ].GetComponent<GamePanel>( );
            panel.SetGame( GameManager.Instance.PlayerOwnedGames[ i ] );
            panel.PanelIdx = i;
            panel.OnGameSelected += OnGameSelected;
        }

        BtnSelect.gameObject.SetActive( false );
    }

    void FixedUpdate( )
    {
        BtnSelect.gameObject.SetActive( SelectedPanel != null );
    }

    void OnGameSelected( int idx )
    {
        if ( SelectedPanel != null )
        {
            SelectedPanel.GetComponent<GamePanel>( ).IsSelected = false;
            SelectedPanel = null;
        }

        SelectedPanel = GamePanels[ idx ];
        SelectedPanel.GetComponent<GamePanel>( ).IsSelected = true;
    }

    public void Select( )
    {
        GameManager.Instance.BeginStream(SelectedPanel.GetComponent<GamePanel>( ).ParentGame);
        GameObject.Destroy( gameObject );
    }
}