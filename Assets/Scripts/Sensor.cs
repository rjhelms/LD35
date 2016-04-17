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

    // dummy Scan method - sets everything invisible
    public virtual void Scan(int x_pos, int y_pos, Direction dir)
    {
        foreach (Tile tile in tileMap.TileArray)
        {
            tile.SetInvisible();
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
        foreach (Tile tile in tileMap.TileArray)
        {
            tile.SetVisible();
        }
    }
}