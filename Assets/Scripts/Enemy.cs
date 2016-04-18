using UnityEngine;
using System;
using System.Collections;

public class Enemy
{
    public int PositionX;
    public int PositionY;

    protected Direction direction;
    protected TileMap tileMap;
    protected GameController controller;
    protected string name;
    protected int damage;
    protected int hitPoints;

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
        damage = 10;
        hitPoints = 10;
    }

    public virtual void Move()
    {
    }

    public void Hit(int damage)
    {
        hitPoints -= damage;
        if (hitPoints <= 0)
        {
            die();
        }
        else
        {
            Debug.LogFormat("{0} has {1} hit points", name, hitPoints);
        }
    }

    protected void die()
    {
        Debug.LogFormat("{0} is killed!", name, hitPoints);
        controller.MessageList.Add("ENEMY DESTROYED");
        tileMap.TileArray[PositionX, PositionY].Contents = TileContents.EMPTY_TILE;
        controller.Enemies.Remove(this);
        controller.PlaySound(Sound.ENEMY_DESTROYED);
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
            controller.Hit(this, damage);
        }
        else if (tileMap.TileArray[facing_x, facing_y].Contents == TileContents.EMPTY_TILE)
        {
            new Projectile(facing_x, facing_y, controller, tileMap, direction, this, damage);
        }
        else
        {
            Debug.LogFormat("{0} fire at ({1}, {2}) blocked by level", name, facing_x, facing_y);
        }
    }

}

public class DumbBot : Enemy
{
    // these chances are top ends of ranges
    private float turnChance = 0.25f;
    private float walkChance = 0.75f;
    private float fireChance = 0.80f;
    private int coolDown;
    public DumbBot(int x_pos, int y_pos, TileMap map, GameController player) : base(x_pos, y_pos, map, player)
    {
        map.TileArray[PositionX, PositionY].Contents = TileContents.DUMB_BOT;
        direction = (Direction)UnityEngine.Random.Range(0, 4);
        name = "Drone";
        damage = 5;
        hitPoints = 10;
        coolDown = 0;
    }

    public override void Move()
    {
        if (coolDown > 0)   // cooldown if fired last turn
        {
            Debug.LogFormat("{0} at {1}, {2} cooling down", name, PositionX, PositionY);
            coolDown--;
            return;
        }
        float roll = UnityEngine.Random.value;
        if (roll < turnChance)
        {
            float turnDir = 0.5f - UnityEngine.Random.value;
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
            coolDown = 1;
        }

    }
}

public class Sentinel : Enemy
{
    private int turnState;
    private int coolDown;
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
        damage = 10;
        hitPoints = 15;
        name = "Sentinel";
    }

    public override void Move()
    {
        if (coolDown > 0)   // cooldown if fired last turn
        {
            Debug.LogFormat("{0} at {1}, {2} cooling down", name, PositionX, PositionY);
            coolDown--;
            return;
        }

        // check for line of sight for player
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
                Debug.LogFormat("{2} at {0}, {1} has line of sight", PositionX, PositionY, name);
                to_fire = true;
            }
            else if (!tileMap.TileArray[check_tile_x, check_tile_y].CanSeeThrough())
            {
                break;
            }
        }

        // check sound for a player
        if (controller.MadeNoiseLastMove)
        {
            if (Math.Abs(PositionX - controller.PlayerXPos) + Math.Abs(PositionY - controller.PlayerYPos) <= 3)
                {
                Debug.LogFormat("{2} at {0}, {1} heard the player", PositionX, PositionY, name);
                to_fire = true;
            }
        }
        if (to_fire)
        {
            coolDown = 2;
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
                float turn_roll = UnityEngine.Random.value;
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