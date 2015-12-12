using UnityEngine;
using System.Collections;

public class ModalWindow : MonoBehaviour
{
    public void Close( )
    {
        GameObject.Destroy( this.gameObject );
    }
}
