using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Sensor
{
    protected TileMap tileMap;

    public Sensor(TileMap map)
    {
        tileMap = map;
    }

    // sets everything invisible, as starting point for derivative class behaviour
    public virtual void Scan(int x_pos, int y_pos, Direction dir)
    {
        foreach (Tile tile in tileMap.TileArray)
        {
            tile.SetInvisible();
            tileMap.TileArray[x_pos, y_pos].SetVisible();
        }
    }
}

public class DirectionalSensor : Sensor
{
    public DirectionalSensor(TileMap map) : base(map)
    {
    }

    public override void Scan(int x_pos, int y_pos, Direction dir)
    {
        base.Scan(x_pos, y_pos, dir);

        int scan_x = 0;
        int scan_y = 0;
        switch (dir)
        {
            case Direction.NORTH:
                scan_y = 1;
                break;
            case Direction.EAST:
                scan_x = 1;
                break;
            case Direction.SOUTH:
                scan_y = -1;
                break;
            case Direction.WEST:
                scan_x = -1;
                break;
        }

        if (scan_x != 0)
        {
            tileMap.TileArray[x_pos + scan_x, y_pos].SetVisible();
            if (tileMap.TileArray[x_pos + scan_x, y_pos].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos + 2 * scan_x, y_pos].SetVisible();
                if (tileMap.TileArray[x_pos + 2 * scan_x, y_pos].Contents == TileContents.EMPTY_TILE)
                    tileMap.TileArray[x_pos + 3 * scan_x, y_pos].SetVisible();
            }
            tileMap.TileArray[x_pos + scan_x, y_pos - 1].SetVisible();
            if (tileMap.TileArray[x_pos + scan_x, y_pos - 1].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos + 2 * scan_x, y_pos - 2].SetVisible();
                if (tileMap.TileArray[x_pos + 2 * scan_x, y_pos - 2].Contents == TileContents.EMPTY_TILE)
                    tileMap.TileArray[x_pos + 3 * scan_x, y_pos - 3].SetVisible();
            }
            tileMap.TileArray[x_pos + scan_x, y_pos + 1].SetVisible();
            if (tileMap.TileArray[x_pos + scan_x, y_pos + 1].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos + 2 * scan_x, y_pos + 2].SetVisible();
                if (tileMap.TileArray[x_pos + 2 * scan_x, y_pos + 2].Contents == TileContents.EMPTY_TILE)
                    tileMap.TileArray[x_pos + 3 * scan_x, y_pos + 3].SetVisible();
            }
            if (tileMap.TileArray[x_pos + scan_x, y_pos].Contents == TileContents.EMPTY_TILE |
                tileMap.TileArray[x_pos + scan_x, y_pos + 1].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos + 2 * scan_x, y_pos + 1].SetVisible();
                if (tileMap.TileArray[x_pos + 2 * scan_x, y_pos + 1].Contents == TileContents.EMPTY_TILE)
                {
                    tileMap.TileArray[x_pos + 3 * scan_x, y_pos + 1].SetVisible();
                    tileMap.TileArray[x_pos + 3 * scan_x, y_pos + 2].SetVisible();
                }
            }
            if (tileMap.TileArray[x_pos + scan_x, y_pos].Contents == TileContents.EMPTY_TILE |
                tileMap.TileArray[x_pos + scan_x, y_pos - 1].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos + 2 * scan_x, y_pos - 1].SetVisible();
                if (tileMap.TileArray[x_pos + 2 * scan_x, y_pos - 1].Contents == TileContents.EMPTY_TILE)
                {
                    tileMap.TileArray[x_pos + 3 * scan_x, y_pos - 1].SetVisible();
                    tileMap.TileArray[x_pos + 3 * scan_x, y_pos - 2].SetVisible();
                }
            }
        }
        else if (scan_y != 0)
        {
            tileMap.TileArray[x_pos, y_pos + scan_y].SetVisible();
            if (tileMap.TileArray[x_pos, y_pos + scan_y].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos, y_pos + 2 * scan_y].SetVisible();
                if (tileMap.TileArray[x_pos, y_pos + 2 * scan_y].Contents == TileContents.EMPTY_TILE)
                    tileMap.TileArray[x_pos, y_pos + 3 * scan_y].SetVisible();
            }
            tileMap.TileArray[x_pos - 1, y_pos + scan_y].SetVisible();
            if (tileMap.TileArray[x_pos - 1, y_pos + scan_y].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos - 2, y_pos + 2 * scan_y].SetVisible();
                if (tileMap.TileArray[x_pos - 2, y_pos + 2 * scan_y].Contents == TileContents.EMPTY_TILE)
                    tileMap.TileArray[x_pos - 3, y_pos + 3 * scan_y].SetVisible();
            }
            tileMap.TileArray[x_pos + 1, y_pos + scan_y].SetVisible();
            if (tileMap.TileArray[x_pos + 1, y_pos + scan_y].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos + 2, y_pos + 2 * scan_y].SetVisible();
                if (tileMap.TileArray[x_pos + 2, y_pos + 2 * scan_y].Contents == TileContents.EMPTY_TILE)
                    tileMap.TileArray[x_pos + 3, y_pos + 3 * scan_y].SetVisible();
            }
            if (tileMap.TileArray[x_pos, y_pos + scan_y].Contents == TileContents.EMPTY_TILE |
                tileMap.TileArray[x_pos + 1, y_pos + scan_y].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos + 1, y_pos + 2 * scan_y].SetVisible();
                if (tileMap.TileArray[x_pos + 1, y_pos + 2 * scan_y].Contents == TileContents.EMPTY_TILE)
                {
                    tileMap.TileArray[x_pos + 1, y_pos + 3 * scan_y].SetVisible();
                    tileMap.TileArray[x_pos + 2, y_pos + 3 * scan_y].SetVisible();
                }
            }
            if (tileMap.TileArray[x_pos, y_pos + scan_y].Contents == TileContents.EMPTY_TILE |
                tileMap.TileArray[x_pos - 1, y_pos + scan_y].Contents == TileContents.EMPTY_TILE)
            {
                tileMap.TileArray[x_pos - 1, y_pos + 2 * scan_y].SetVisible();
                if (tileMap.TileArray[x_pos - 1, y_pos + 2 * scan_y].Contents == TileContents.EMPTY_TILE)
                {
                    tileMap.TileArray[x_pos - 1, y_pos + 3 * scan_y].SetVisible();
                    tileMap.TileArray[x_pos - 2, y_pos + 3 * scan_y].SetVisible();
                }
            }
        }
    }
}

public class OmniSensor : Sensor
{
    public OmniSensor(TileMap map) : base(map)
    {
    }

    public override void Scan(int x_pos, int y_pos, Direction dir)
    {
        base.Scan(x_pos, y_pos, dir);

        tileMap.TileArray[x_pos - 1, y_pos - 2].SetVisible();
        tileMap.TileArray[x_pos, y_pos - 2].SetVisible();
        tileMap.TileArray[x_pos + 1, y_pos - 2].SetVisible();
        tileMap.TileArray[x_pos - 2, y_pos - 1].SetVisible();
        tileMap.TileArray[x_pos - 1, y_pos - 1].SetVisible();
        tileMap.TileArray[x_pos, y_pos - 1].SetVisible();
        tileMap.TileArray[x_pos + 1, y_pos - 1].SetVisible();
        tileMap.TileArray[x_pos + 2, y_pos - 1].SetVisible();
        tileMap.TileArray[x_pos - 2, y_pos].SetVisible();
        tileMap.TileArray[x_pos - 1, y_pos].SetVisible();
        tileMap.TileArray[x_pos, y_pos].SetVisible();
        tileMap.TileArray[x_pos + 1, y_pos].SetVisible();
        tileMap.TileArray[x_pos + 2, y_pos].SetVisible();
        tileMap.TileArray[x_pos - 2, y_pos + 1].SetVisible();
        tileMap.TileArray[x_pos - 1, y_pos + 1].SetVisible();
        tileMap.TileArray[x_pos, y_pos + 1].SetVisible();
        tileMap.TileArray[x_pos + 1, y_pos + 1].SetVisible();
        tileMap.TileArray[x_pos + 2, y_pos + 1].SetVisible();
        tileMap.TileArray[x_pos - 1, y_pos + 2].SetVisible();
        tileMap.TileArray[x_pos, y_pos + 2].SetVisible();
        tileMap.TileArray[x_pos + 1, y_pos + 2].SetVisible();
    }
}

public class IRSensor : Sensor
{
    public IRSensor(TileMap map) : base(map)
    {
    }

    public override void Scan(int x_pos, int y_pos, Direction dir)
    {
        base.Scan(x_pos, y_pos, dir);
        switch (dir)
        {
            case Direction.NORTH:
                tileMap.TileArray[x_pos - 1, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos - 1, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos + 3].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos + 3].SetVisible();
                tileMap.TileArray[x_pos - 1, y_pos + 3].SetVisible();
                tileMap.TileArray[x_pos, y_pos + 3].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos + 3].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos + 3].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos + 3].SetVisible();
                break;
            case Direction.EAST:
                tileMap.TileArray[x_pos + 1, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos + 3].SetVisible();
                break;
            case Direction.SOUTH:
                tileMap.TileArray[x_pos - 1, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos - 1, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos - 1, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos + 1, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos + 2, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos + 3, y_pos - 3].SetVisible();
                break;
            case Direction.WEST:
                tileMap.TileArray[x_pos - 1, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos - 1, y_pos].SetVisible();
                tileMap.TileArray[x_pos - 1, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos - 2, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos - 3].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos - 2].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos - 1].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos + 1].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos + 2].SetVisible();
                tileMap.TileArray[x_pos - 3, y_pos + 3].SetVisible();
                break;
        }
    }
}