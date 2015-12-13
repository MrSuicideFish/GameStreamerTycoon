using UnityEngine;
using System.Collections;

public class ModalWindow : MonoBehaviour
{
    public virtual void Close( )
    {
        GameManager.Instance.bShowingMessage = false;
        GameObject.Destroy( this.gameObject );
    }
}
