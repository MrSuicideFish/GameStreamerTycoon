using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void GameSelect(int idx);

public class GamePanel : MonoBehaviour
{
    public Game ParentGame;
    public Text Title, Developer, Price;
    public Image Sponsored;
    public RectTransform CriticVal, CommunityVal, StreamerVal;
    public Outline PanelOutline;

    public event GameSelect OnGameSelected;

    public int PanelIdx = 0;
    public bool IsSelected = false;


    void FixedUpdate( )
    {
        if ( IsSelected )
            PanelOutline.effectColor = new Color( 0.47f, 0.97f, 1f );
        else
            PanelOutline.effectColor = new Color( 0.57f, 0.80f, 1f );
    }

    public void SetGame( Game newGame )
    {
        Title.text = newGame.Title;
        Developer.text = newGame.Developer;

        Sponsored.gameObject.SetActive( newGame.IsSponsored );

        ParentGame = newGame;

        //Set Ratings
        CommunityVal.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, GameManager.RatingToWidth( ParentGame.CommunityRating, 300 ) );
        CriticVal.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, GameManager.RatingToWidth( ParentGame.CriticRating, 300 ) );
        StreamerVal.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, GameManager.RatingToWidth( ParentGame.StreamerRating, 300 ) );
    }

    public void SelectGame( )
    {
        if ( OnGameSelected != null )
        {
            OnGameSelected.Invoke( PanelIdx );
        }
    }
}
