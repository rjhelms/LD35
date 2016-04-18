using UnityEngine;
using System.Collections;

public class Enemy
{
    public int PositionX;
    public int PositionY;

    protected Direction direction;
    protected TileMap tileMap;
    protected GameController controller;
    protected string name;

    public virtual string Name
    {
        get { return name; }
    }
    public Enemy(int x_pos, int y_pos, TileMap map, GameController player)
    {
        Debug.Log("Base enemy constructor");
        PositionX = x_pos;
        PositionY = y_pos;
        tileMap = map;
        controller = player;
        controller.Enemies.Add(this);
        name = "Enemy";
    }

    public virtual void Move()
    {
    }

    public void Die()
    {
        tileMap.TileArray[PositionX, PositionY].Contents = TileContents.EMPTY_TILE;
        controller.Enemies.Remove(this);
    }

}

public class DumbBot : Enemy
{
    private float turnChance = 0.25f;
    private float walkChance = 0.75f;
    public DumbBot(int x_pos, int y_pos, TileMap map, GameController player) : base(x_pos, y_pos, map, player)
    {
        map.TileArray[PositionX, PositionY].Contents = TileContents.DUMB_BOT;
        direction = (Direction)Random.Range(0, 4);
        name = "Drone";
    }

    public override void Move()
    {
        Debug.Log(Name + " move");
        float roll = Random.value;
        if (roll < turnChance)
        {
            float turnDir = 0.5f - Random.value;
            Debug.Log(Name + " turning");
            if (turnDir < 0)
            {
                direction--;
                if (direction < Direction.NORTH)
                {
                    direction = Direction.WEST;
                }
            }
            else
            {
                direction++;
                if (direction > Direction.WEST)
                {
                    direction = Direction.NORTH;
                }
            }
        }
        else if (roll < walkChance)
        {
            int new_x = PositionX;
            int new_y = PositionY;
            Debug.Log(Name + " walking");
            switch (direction)
            {
                case Direction.NORTH:
                    new_y++;
                    break;
                case Direction.EAST:
                    new_x++;
                    break;
                case Direction.SOUTH:
                    new_y--;
                    break;
                case Direction.WEST:
                    new_x--;
                    break;
            }
            Tile old_tile = tileMap.TileArray[PositionX, PositionY];
            Tile new_tile = tileMap.TileArray[new_x, new_y];
            if (new_tile.Contents == TileContents.EMPTY_TILE &
                (controller.PlayerXPos != new_x | controller.PlayerYPos != new_y))
            {
                PositionX = new_x;
                PositionY = new_y;
                old_tile.Contents = TileContents.EMPTY_TILE;
                new_tile.Contents = TileContents.DUMB_BOT;
            }
            else
            {
                Debug.Log(Name + " hit wall");
            }
        }

    }
}

public class Sentinel : Enemy
{
    private int turnState;
    public Sentinel(int x_pos, int y_pos, TileMap map, GameController player, Direction dir) : base(x_pos, y_pos, map, player)
    {
        if (dir == Direction.EAST | dir == Direction.WEST)
        {
            map.TileArray[PositionX, PositionY].Contents = TileContents.SENTINEL_BOT_EW;
        } else
        {
            map.TileArray[PositionX, PositionY].Contents = TileContents.SENTINEL_BOT_NS;
        }
        direction = dir;
        name = "Sentinel";
    }

    public override void Move()
    {
        Debug.Log(Name + " move");
        if (turnState > 0)
        {
            Debug.Log(Name + " turn right");
            direction++;
            if (direction > Direction.WEST)
            {
                direction = Direction.NORTH;
            }
            turnState--;
        } else if (turnState < 0)
        {
            Debug.Log(Name + " turn left");
            direction--;
            if (direction < Direction.NORTH)
            {
                direction = Direction.WEST;
            }
            turnState++;
        } else
        {
            int new_x = PositionX;
            int new_y = PositionY;
            Debug.Log(Name + " walking");
            switch (direction)
            {
                case Direction.NORTH:
                    new_y++;
                    break;
                case Direction.EAST:
                    new_x++;
                    break;
                case Direction.SOUTH:
                    new_y--;
                    break;
                case Direction.WEST:
                    new_x--;
                    break;
            }
            Tile old_tile = tileMap.TileArray[PositionX, PositionY];
            Tile new_tile = tileMap.TileArray[new_x, new_y];
            if (new_tile.Contents == TileContents.EMPTY_TILE &
                (controller.PlayerXPos != new_x | controller.PlayerYPos != new_y))
            {
                PositionX = new_x;
                PositionY = new_y;
                old_tile.Contents = TileContents.EMPTY_TILE;
                if (direction == Direction.EAST | direction == Direction.WEST)
                {
                    new_tile.Contents = TileContents.SENTINEL_BOT_EW;
                }
                else
                {
                    new_tile.Contents = TileContents.SENTINEL_BOT_NS;
                }
            }
            else
            {
                Debug.Log(Name + " hit wall");
                float turn_roll = Random.value;
                if (turn_roll < 0.5f)
                {
                    Debug.Log(Name + " turn around left");
                    turnState = -2;
                } else
                {
                    Debug.Log(Name + " turn around right");
                    turnState = 2;
                }
            }
        }
    }
}