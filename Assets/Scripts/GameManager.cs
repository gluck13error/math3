using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    [SerializeField] private Camera _main;
    [SerializeField] private Text textSource;

    private TilesControll _tilesControll;
  
    private Tween tweenSelected;
    private SwapElement SwapElement1;
    private SwapElement SwapElement2;

    private int source = 0;
    enum GameState
    {
        Not,
        Selecting,
        CheckSwap,
        AnimatingSwap,
        CheckDestroy,
        AnimationDestroy,
        ClearDestroy,
        CheckMove,
        AnimatingMove,
    }
    private GameState state;
    void Start()
    {
        _tilesControll = new TilesControll(_config);
        _tilesControll.InitTiles();
        _tilesControll.CheckAndSetSwap();
    }


    bool CheckSwap(SwapElement element1, SwapElement element2)
    {
        var sub = element1.position - element2.position;
        return (Mathf.Abs(sub.x) == 1 && sub.y == 0) || (Mathf.Abs(sub.y) == 1 && sub.x == 0);
    }
   
    void StopSelect()
    {
        if (tweenSelected != null)
            tweenSelected.Kill();
        if (SwapElement1 != null)
        {
            var color = SwapElement1.sr.color;
            color.a = 1;
            SwapElement1.sr.color = color;
        }
    }
    void Select(SwapElement element)
    {
        StopSelect();
        state = GameState.Selecting;
        tweenSelected = element.sr.DOFade(0, 0.3f).SetLoops(-1, LoopType.Yoyo);
        SwapElement1 = element;
    }
  
    void AddSourceLine(List<int>lines)
    {
        foreach (var count in lines)
        {
            if (count == 3)
                source += _config.Reward3;
            if (count == 4)
                source += _config.Reward4;
            if (count >= 5)
                source += _config.Reward5;
        }
        textSource.text = source.ToString();
    }
    void Update()
    {
        if (state == GameState.Not || state == GameState.Selecting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var pos = _main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log(Input.mousePosition);
                var hit = Physics2D.Raycast(_main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    var newselected = hit.collider.GetComponent<SwapElement>();
                    if (newselected != null)
                    {

                        if (state == GameState.Not)
                        {
                            Select(newselected);

                        }
                        else if (state == GameState.Selecting)
                        {
                            if (CheckSwap(SwapElement1, newselected))
                            {
                                state = GameState.AnimatingSwap;
                                SwapElement2 = newselected;
                                StopSelect();
                                _tilesControll.RunSwap(SwapElement1, SwapElement2).OnComplete(() => state = GameState.CheckSwap);
                            }
                            else
                            {
                                Select(newselected);
                            }
                        }
                    }
                }

            }
        }

        if (state == GameState.CheckSwap)
        {
            if ( _tilesControll.CheckDestroy(SwapElement1.position, SwapElement1.Type) || _tilesControll.CheckDestroy(SwapElement2.position, SwapElement2.Type))
            {
                state = GameState.CheckDestroy;
            }
            else
            {
                state = GameState.AnimatingSwap;
                _tilesControll.RunSwap(SwapElement1, SwapElement2).OnComplete(() => state = GameState.Not);
            }
        }

        if (state == GameState.CheckDestroy)
        {
         
            state = GameState.AnimationDestroy;
            var lines = _tilesControll.DestoryElement(new List<Vector2Int> {SwapElement1.position,SwapElement2.position });
            AddSourceLine(lines);
            _tilesControll.RunDestoryElement()
                .OnComplete(() => state = GameState.ClearDestroy);
           
        }
        if (state == GameState.ClearDestroy)
        {
            state = GameState.CheckMove;
            _tilesControll.ClearTiles();
        }
        if (state == GameState.CheckMove)
        {
            state = GameState.AnimatingMove;
            if (_tilesControll.CheckMove())
            {
                _tilesControll.RunMove().OnComplete(() => state = GameState.CheckMove);
            }
            else
            {
                var lines = _tilesControll.DestoryElement(null);
                if (lines.Count > 0)
                {
                    AddSourceLine(lines);
                    _tilesControll.RunDestoryElement()
                         .OnComplete(() => state = GameState.ClearDestroy);
                }
                else
                {
                    _tilesControll.CheckAndSetSwap();
                 
                    state = GameState.Not;
                }
            }
        }
    }
}
