using UnityEngine;
using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GameService : MonoBehaviour
{
    static XmlDocument GameListDocument;
    static string DataPath = Application.dataPath + "/Data";

    public static void InitDocument( )
    {
        GameListDocument = new XmlDocument( );
        GameListDocument.Load( DataPath + "/Games.xml" );
    }

    public static Game GetRandomGame( )
    {
        Game newGame = new Game( );

        //Find random game node
        XmlNode gameListNode = GameListDocument.SelectSingleNode( "Games" );
        int gameCount = gameListNode.ChildNodes.Count;

        XmlNode gameNode = gameListNode.ChildNodes[ Random.Range( 0, gameCount ) ];

        newGame.Title = gameNode.ChildNodes[ 0 ].InnerText; //Title
        newGame.Developer = gameNode.ChildNodes[ 1 ].InnerText; //Developer

        return newGame;
    }
}
