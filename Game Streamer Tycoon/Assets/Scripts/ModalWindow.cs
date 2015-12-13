using UnityEngine;
using System.Collections;

public class ModalWindow : MonoBehaviour
{
    public void Close( )
    {
        GameManager.Instance.bShowingMessage = false;
        GameObject.Destroy( this.gameObject );
    }
}
