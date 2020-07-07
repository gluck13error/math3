using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class TilesControll
{
    private GameConfig _config;

    private SwapElement[,] tiles;
    private HashSet<Vector2Int> checkTiles = new HashSet<Vector2Int>();
    public TilesControll(GameConfig config)
    {
        _config = config;
    }
    SwapElement GetRandomElement()
    {
        var random = Random.Range(0, _config.TypeColors.Length);
        return GameObject.Instantiate<SwapElement>(_config.TypeColors[random]);
    }

    public void InitTiles()
    {
        tiles = new SwapElement[_config.Rows, _config.Columns];

        int count = 3;
        while (count > 0)
        {
            var randomx = Random.Range(0, _config.Rows);
            var randomy = Random.Range(0, _config.Columns);
            if (tiles[randomx, randomy] == null)
            {
                tiles[randomx, randomy] = GameObject.Instantiate<SwapElement>(_config.TypeNull);
                count--;
            }
        }

        for (int row = 0; row < _config.Rows; row++)
        {
            for (int column = 0; column < _config.Columns; column++)
                if (tiles[column, row] == null)
                {

                    SwapElement newElement = null;
                    bool Checking = true;
                    while (Checking)
                    {
                        Checking = false;
                        if (newElement != null)
                        {
                            GameObject.Destroy(newElement.gameObject);
                        }
                        newElement = GetRandomElement();
                        if (row >= 2 && tiles[column, row - 1].Type == newElement.Type && tiles[column, row - 2].Type == newElement.Type)
                        {
                            Checking = true;
                        }
                        if (column >= 2 && tiles[column - 1, row].Type == newElement.Type && tiles[column - 2, row].Type == newElement.Type)
                        {
                            Checking = true;
                        }
                    }
                    tiles[column, row] = newElement;
                }
        }
        for (int row = 0; row < _config.Rows; row++)
        {
            for (int column = 0; column < _config.Columns; column++)
            {
                tiles[column, row].transform.localPosition = new Vector3(column, row);
                tiles[column, row].position = new Vector2Int(column, row);
            }
        }
    }



    public Tween RunSwap(SwapElement element1, SwapElement element2)
    {
        //StopSelect();

        var pos = element1.position;
        element1.position = element2.position;
        element2.position = pos;
        tiles[element1.position.x, element1.position.y] = element1;
        tiles[element2.position.x, element2.position.y] = element2;
        var sequence = DOTween.Sequence();
        sequence.Join(element1.transform.DOLocalMove(element2.transform.localPosition, 0.3f));
        sequence.Join(element2.transform.DOLocalMove(element1.transform.localPosition, 0.3f));
        return sequence;
    }

    struct SizeLine
    {
        public int start;
        public int end;
    }
    SizeLine CheckLineX(Vector2Int Pos, string Type)
    {
        SizeLine result = new SizeLine { start = Pos.x, end = Pos.x };

        for (int x = 1; x < 5; x++)
        {
            if (Pos.x + x < _config.Columns && tiles[Pos.x + x, Pos.y].Type == Type)
                result.end++;
            else break;
        }
        for (int x = 1; x < 5; x++)
        {
            if (Pos.x - x >= 0 && tiles[Pos.x - x, Pos.y].Type == Type)
                result.start--;
            else break;
        }

        return result;
    }
    SizeLine CheckLineY(Vector2Int Pos, string Type)
    {
        SizeLine result = new SizeLine { start = Pos.y, end = Pos.y };

        for (int y = 1; y < 5; y++)
        {
            if (Pos.y + y < _config.Rows && tiles[Pos.x, Pos.y + y].Type == Type)
                result.end++;
            else break;
        }
        for (int y = 1; y < 5; y++)
        {
            if (Pos.y - y >= 0 && tiles[Pos.x, Pos.y - y].Type == Type)
                result.start--;
            else break;
        }

        return result;
    }

    public List<int> DestoryElement(List<Vector2Int> checkTiles)
    {
        var result = new List<int>();
        if (checkTiles == null)
            checkTiles = this.checkTiles.ToList();

        foreach (var position in checkTiles)
        {
            var size = CheckLineX(position, tiles[position.x, position.y].Type);
            var len = size.end - size.start + 1;
            if (len >= 3)
            {
                //  AddSource(len);
                result.Add(len);
                for (int x = size.start; x <= size.end; x++)
                {


                    tiles[x, position.y].IsDestroy = true;

                }
            }
            size = CheckLineY(position, tiles[position.x, position.y].Type);
            len = size.end - size.start + 1;
            if (len >= 3)
            {
                result.Add(len);
                //  AddSource(len);
                for (int y = size.start; y <= size.end; y++)
                {
                    tiles[position.x, y].IsDestroy = true;
                }
            }
        }
        return result;
    }
    public Tween RunDestoryElement()
    {
        var sequence = DOTween.Sequence();
        for (int row = 0; row < _config.Rows; row++)
        {
            for (int column = 0; column < _config.Columns; column++)
                if (tiles[column, row].IsDestroy)
                    sequence.Join(tiles[column, row].sr.DOFade(0, 0.3f));

        }
        sequence.Play();
        return sequence;
    }
    public void ClearTiles()
    {
        for (int row = 0; row < _config.Rows; row++)
        {
            for (int column = 0; column < _config.Columns; column++)
            {
                var element = tiles[column, row];
                if (element.IsDestroy)
                {

                    GameObject.Destroy(element.gameObject);
                    tiles[column, row] = null;
                }
            }
        }
    }

    Tween RunMoveElement(SwapElement element, Vector2Int newpos, bool clear = true)
    {

        if (clear)
            tiles[element.position.x, element.position.y] = null;
        tiles[newpos.x, newpos.y] = element;
        element.position = newpos;
        return element.transform.DOLocalMove(new Vector3(newpos.x, newpos.y), 0.3f);
    }

    public bool CheckDestroy(Vector2Int Pos, string Type)
    {

        var size = CheckLineX(Pos, Type);
        if (size.end - size.start >= 2)
            return true;
        size = CheckLineY(Pos, Type);
        if (size.end - size.start >= 2)
            return true;
        return false;
    }

    public bool CheckMove()
    {
        for (int row = 0; row < _config.Rows; row++)
            for (int column = 0; column < _config.Columns; column++)
                if (tiles[column, row] == null)
                    return true;

        return false;
    }
    public Tween RunMove()
    {
        var runsequence = DOTween.Sequence();
        bool run = false;
        for (int row = 0; row < _config.Rows; row++)
        {
            int lastrowdown = -1;
            for (int column = 0; column < _config.Columns; column++)
            {
                if (tiles[column, row] == null)
                {
                    run = true;
                    var movetopos = new Vector2Int(column, row);
                    if (row + 1 < _config.Rows)
                    {
                        var element = tiles[column, row + 1];
                        var find = false;
                        if (element != null && !element.IsNotMoving)
                        {
                            find = true;
                            lastrowdown = column;
                        }
                        if (!find && column - 1 > 0 && lastrowdown != column - 1)
                        {
                            element = tiles[column - 1, row + 1];
                            if (element != null && !element.IsNotMoving)
                                find = true;
                        }
                        if (!find && column + 1 < _config.Columns && tiles[column + 1, row] != null)
                        {
                            element = tiles[column + 1, row + 1];
                            if (element != null && !element.IsNotMoving)
                                find = true;
                        }

                        if (find)
                        {
                            run = true;
                            if (!checkTiles.Contains(movetopos))
                                checkTiles.Add(movetopos);
                            runsequence.Join(RunMoveElement(element, movetopos));
                        }
                    }
                    else
                    {
                        var newelement = GetRandomElement();

                        newelement.transform.localPosition = new Vector3(column, row + 1);

                        if (!checkTiles.Contains(movetopos))
                            checkTiles.Add(movetopos);
                        runsequence.Join(RunMoveElement(newelement, new Vector2Int(column, row), false));

                    }
                }
            }
        }
        return runsequence;
    }

    public void CheckAndSetSwap()
    {
        if (!CheckSwap())
        {
          
            var r = Random.Range(0, _config.patternSwap.Count);
            var pattern = _config.patternSwap[r];
            var maxx = Mathf.Max(pattern.delta1.x, pattern.delta2.x);
            var maxy = Mathf.Max(pattern.delta1.y, pattern.delta2.y);

            SwapElement startelement = null;
            while (startelement == null)
            {
                var randomx = Random.Range(1, _config.Columns - maxx);
                var randomy = Random.Range(1, _config.Rows - maxy);
                var temp = tiles[randomx, randomy];
                if (!temp.IsNotMoving && !tiles[randomx + pattern.checkDelta.x, randomy + pattern.checkDelta.y].IsNotMoving)
                    startelement = temp;

            }
            var posx = startelement.position.x;
            var posy = startelement.position.y;
            GameObject.Destroy(tiles[posx + pattern.delta1.x, posy + pattern.delta1.y].gameObject);
            GameObject.Destroy(tiles[posx + pattern.delta2.x, posy + pattern.delta2.y].gameObject);

            var element = GameObject.Instantiate<SwapElement>(startelement);
            tiles[posx + pattern.delta1.x, posy + pattern.delta1.y] = element;
            element.position = new Vector2Int(posx + pattern.delta1.x, posy + pattern.delta1.y);
            element.transform.localPosition = new Vector3(posx + pattern.delta1.x, posy + pattern.delta1.y);

            element = GameObject.Instantiate<SwapElement>(startelement);
            tiles[posx + pattern.delta2.x, posy + pattern.delta2.y] = element;
            element.position = new Vector2Int(posx + pattern.delta2.x, posy + pattern.delta2.y);
            element.transform.localPosition = new Vector3(posx + pattern.delta2.x, posy + pattern.delta2.y);

        }
    }
    bool CheckSwap()
    {
        for (int row = 0; row < _config.Rows; row++)
            for (int column = 0; column < _config.Columns; column++)
            {
                if (CheckLine(row, column, tiles[row, column].Type))
                {
                    return true;
                }
            }
        return false;
    }


    bool CheckLine(int x, int y, string type)
    {

        foreach (var pattern in _config.patternSwap)
        {
            if ((Mathf.Max(pattern.delta1.x, pattern.delta2.x) + x < _config.Columns) &&
                (Mathf.Min(pattern.delta1.x, pattern.delta2.x) + x >= 0) &&
                (Mathf.Max(pattern.delta1.y, pattern.delta2.y) + y < _config.Rows) &&
                (Mathf.Min(pattern.delta1.y, pattern.delta2.y) + y >= 0))
            {
                if (tiles[x + pattern.delta1.x, y + pattern.delta1.y].Type == type && tiles[x + pattern.delta2.x, y + pattern.delta2.y].Type == type && !tiles[x + pattern.checkDelta.x, y + pattern.checkDelta.y].IsNotMoving)
                    return true;

            }
        }

        return false;
    }

}
