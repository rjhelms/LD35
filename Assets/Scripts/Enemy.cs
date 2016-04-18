using UnityEngine;
using System.Collections;

public class Enemy
{
    public int PositionX;
    public int PositionY;

    protected Direction direction;
    protected TileMap tileMap;
    protected GameController controller;

    public Enemy(int x_pos, int y_pos, TileMap map, GameController player)
    {
        Debug.Log("Base enemy constructor");
        PositionX = x_pos;
        PositionY = y_pos;
        tileMap = map;
        controller = player;
        controller.Enemies.Add(this);
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
    }

    public override void Move()
    {
        Debug.Log("DumbBot move");
        float roll = Random.value;
        if (roll < turnChance)
        {
            float turnDir = 0.5f - Random.value;
            Debug.Log("DumbBot turning");
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
            Debug.Log("DumbBot Walking");
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
            if (new_tile.Contents == TileContents.EMPTY_TILE)
            {
                PositionX = new_x;
                PositionY = new_y;
                old_tile.Contents = TileContents.EMPTY_TILE;
                new_tile.Contents = TileContents.DUMB_BOT;
            }
            else
            {
                Debug.Log("DumbBot hit wall");
            }
        }

    }
}