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

    int MoneyEarned = 0;

    void Start( )
    {
        GameNameTitle.text = GameManager.Instance.CurrentGame.Title;
        FollowersGained.text = "Followers Gained: " + ( GameManager.Instance.Followers - GameManager.Instance.StartFollowers ).ToString( );
        TotalWatchers.text = "Total Watchers: " + GameManager.Instance.BestViewers.ToString( );
        TotalDonations.text = "Total Donations: $" + ( GameManager.Instance.Money - GameManager.Instance.StartCash ).ToString( );

        //Determine comments
        
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