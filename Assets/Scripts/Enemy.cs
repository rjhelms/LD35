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

    protected void fire()
    {
        int facing_x = PositionX;
        int facing_y = PositionY;
        switch (direction)
        {
            case Direction.NORTH:
                facing_y++;
                break;
            case Direction.EAST:
                facing_x++;
                break;
            case Direction.SOUTH:
                facing_y--;
                break;
            case Direction.WEST:
                facing_x--;
                break;
        }
        if (controller.PlayerXPos == facing_x & controller.PlayerYPos == facing_y)
        {
            controller.Hit(this);
        } else
        {
            new Projectile(facing_x, facing_y, controller, tileMap, direction, this);
        }
    }

}

public class DumbBot : Enemy
{
    private float turnChance = 0.25f;
    private float walkChance = 0.75f;
    private float fireChance = 0.80f;
    public DumbBot(int x_pos, int y_pos, TileMap map, GameController player) : base(x_pos, y_pos, map, player)
    {
        map.TileArray[PositionX, PositionY].Contents = TileContents.DUMB_BOT;
        direction = (Direction)Random.Range(0, 4);
        name = "Drone";
    }

    public override void Move()
    {
        float roll = Random.value;
        if (roll < turnChance)
        {
            float turnDir = 0.5f - Random.value;
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

        }
        else if (roll < fireChance)
        {
            fire();
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
        }
        else
        {
            map.TileArray[PositionX, PositionY].Contents = TileContents.SENTINEL_BOT_NS;
        }
        direction = dir;
        name = "Sentinel";
    }

    public override void Move()
    {
        int facing_x = 0;
        int facing_y = 0;
        switch (direction)
        {
            case Direction.NORTH:
                facing_y++;
                break;
            case Direction.EAST:
                facing_x++;
                break;
            case Direction.SOUTH:
                facing_y--;
                break;
            case Direction.WEST:
                facing_x--;
                break;
        }
        bool to_fire = false;
        int check_tile_x = PositionX;
        int check_tile_y = PositionY;
        for (int i = 0; i < 4; i++)
        {
            check_tile_x += facing_x;
            check_tile_y += facing_y;
            if (controller.PlayerXPos == check_tile_x & controller.PlayerYPos == check_tile_y)
            {
                to_fire = true;
            }
            else if (tileMap.TileArray[check_tile_x, check_tile_y].Contents != TileContents.EMPTY_TILE)
            {
                break;
            }
        }
        if (to_fire)
        {
            fire();
            return;
        }
        if (turnState > 0)
        {
            direction++;
            if (direction > Direction.WEST)
            {
                direction = Direction.NORTH;
            }
            turnState--;
        }
        else if (turnState < 0)
        {
            direction--;
            if (direction < Direction.NORTH)
            {
                direction = Direction.WEST;
            }
            turnState++;
        }
        else
        {
            int new_x = PositionX + facing_x;
            int new_y = PositionY + facing_y;

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
                float turn_roll = Random.value;
                if (turn_roll < 0.5f)
                {
                    turnState = -2;
                }
                else
                {
                    turnState = 2;
                }
            }
        }
    }
}