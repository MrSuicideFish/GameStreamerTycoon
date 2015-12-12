using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameHUD : MonoBehaviour
{
    public Text AmountOfCash, NumOfFollowers, NumOfViewers;
    public Button LiveButton;

    void FixedUpdate( )
    {
        NumOfFollowers.text = GameManager.Instance.Followers.ToString( );
        NumOfViewers.text = GameManager.Instance.Viewers.ToString( );
    }

    public void GoLive( )
    {
        GameManager.Instance.ToggleLive( );
        LiveButton.interactable = !GameManager.Instance.IsLive;
    }
}
