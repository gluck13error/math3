using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class DeltaType
{
    public Vector2Int delta1;
    public Vector2Int delta2;
    public Vector2Int checkDelta;
}
[CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    public int Reward3 = 10;
    public int Reward4 = 15;
    public int Reward5 = 20;
    public int Rows = 6;
    public int Columns = 6;

    public SwapElement TypeNull;

    public SwapElement[] TypeColors;
    public List<DeltaType> patternSwap;
    

}


