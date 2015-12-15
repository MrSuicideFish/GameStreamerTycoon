using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MarketplaceWindow : ModalWindow
{
    public GameObject ContentPanel;
    public Button BtnBuy;
    public Text PlayerCash;
    public int NumGamesToDisplay = 5;

    List<GameObject> GamePanels;
    GameObject SelectedPanel;

    public void Buy( )
    {
        if ( SelectedPanel != null )
        {
            GameManager.Instance.PurchaseGame( SelectedPanel.GetComponent<GamePanel>( ).ParentGame );
        }
    }

    void FixedUpdate( )
    {
        BtnBuy.interactable = SelectedPanel != null && !SelectedPanel.GetComponent<GamePanel>().ParentGame.Owned;
        PlayerCash.text = "$" + ( ( int )( GameManager.Instance.Money ) ).ToString( );

        if ( GamePanels != null )
        {
            foreach ( GameObject panelObj in GamePanels )
            {
                GamePanel panel = panelObj.GetComponent<GamePanel>( );
                panelObj.GetComponent<Button>( ).interactable = !panel.ParentGame.Owned && ( panel.ParentGame.Price <= GameManager.Instance.Money );
            }
        }
    }

    public void RefreshGames( )
    {
        if ( GamePanels != null )
        {
            for ( int i = 0; i < GamePanels.Count; i++ )
            {
                GameObject.Destroy( GamePanels[ i ] );
            }

            GamePanels = null;
        }

        //Set panel length
        ContentPanel.GetComponent<RectTransform>( ).SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, ( NumGamesToDisplay * 200 ) + 40 );

        //Refresh market games
        for ( int i = 0; i < NumGamesToDisplay; i++ )
        {
            Game newGame = Game.CreateGame( );

            //Create game panel in list
            GameObject newPanel = ( GameObject )GameObject.Instantiate( Resources.Load( "GamePanel" ), Vector3.zero, Quaternion.identity );
            newPanel.transform.SetParent( ContentPanel.transform, false );

            //Position panel
            RectTransform panelRect = newPanel.GetComponent<RectTransform>( );
            panelRect.anchoredPosition = new Vector2( -23.33f, -25 - ( 200 * i ) );

            GamePanel panel = newPanel.GetComponent<GamePanel>( );
            panel.Price.gameObject.SetActive( true ); //Show the price
            panel.Price.text = "$" + newGame.Price;

            panel.SetGame( newGame );
            panel.PanelIdx = i;
            panel.OnGameSelected += OnGameSelected;

            if ( GamePanels == null ) GamePanels = new List<GameObject>( );

            GamePanels.Add( newPanel );
        }
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
}
