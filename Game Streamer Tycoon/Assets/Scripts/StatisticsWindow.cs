using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StatisticsWindow : MonoBehaviour
{
    public GameObject SocialObj, VerbalObj, SkillObj, HardwareObj;

    void Start( )
    {

    }

    void FixedUpdate( )
    {
        //Show / hide increment buttons
        SocialObj.transform.GetChild( 0 ).gameObject.SetActive( GameManager.Instance.SkillPoints > 0 );
        VerbalObj.transform.GetChild( 0 ).gameObject.SetActive( GameManager.Instance.SkillPoints > 0 );
        SkillObj.transform.GetChild( 0 ).gameObject.SetActive( GameManager.Instance.SkillPoints > 0 );
        HardwareObj.transform.GetChild( 0 ).gameObject.SetActive( GameManager.Instance.SkillPoints > 0 );

    }

    public void AddSocialPoint( )
    {
        GameManager.Instance.IncrementStat( GameManager.STATISTIC_TYPE.SOCIAL );

        for ( int i = 1; i <= GameManager.Instance.SocialLevel; i++ )
        {
            SocialObj.transform.GetChild( i ).GetComponent<Image>().color = Color.white;
        }
    }

    public void AddVerbalPoint( )
    {
        GameManager.Instance.IncrementStat( GameManager.STATISTIC_TYPE.VERBAL );

        for ( int i = 1; i <= GameManager.Instance.VerbalLevel; i++ )
        {
            VerbalObj.transform.GetChild( i ).GetComponent<Image>( ).color = Color.white;
        }
    }

    public void AddSkillPoint( )
    {
        GameManager.Instance.IncrementStat( GameManager.STATISTIC_TYPE.SKILL );

        for ( int i = 1; i <= GameManager.Instance.SkillLevel; i++ )
        {
            SkillObj.transform.GetChild( i ).GetComponent<Image>( ).color = Color.white;
        }
    }

    public void AddHardwarePoint( )
    {
        GameManager.Instance.IncrementStat( GameManager.STATISTIC_TYPE.HARDWARE );

        for ( int i = 1; i <= GameManager.Instance.HardwareLevel; i++ )
        {
            HardwareObj.transform.GetChild( i ).GetComponent<Image>( ).color = Color.white;
        }
    }
}