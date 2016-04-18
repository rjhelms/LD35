using UnityEngine;

public class Projectile
{
    public int PositionX;
    public int PositionY;
    public Enemy Origin;

    private GameController controller;
    private TileMap tileMap;
    private Direction direction;

    private TileContents tileContent;
    private int coolDown;

    public Projectile(int positionX, int positionY, GameController controller, TileMap tileMap, Direction direction, Enemy origin)
    {
        PositionX = positionX;
        PositionY = positionY;
        Origin = origin;
        this.controller = controller;
        this.tileMap = tileMap;
        this.direction = direction;
        coolDown = 0;
        switch (direction)
        {
            case Direction.NORTH:
                tileContent = TileContents.LASER_N;
                break;
            case Direction.EAST:
                tileContent = TileContents.LASER_E;
                break;
            case Direction.SOUTH:
                tileContent = TileContents.LASER_S;
                break;
            case Direction.WEST:
                tileContent = TileContents.LASER_W;
                break;
        }
        tileMap.TileArray[positionX, positionY].Contents = tileContent;
        controller.Projectiles.Add(this);
    }

    public void Move()
    {
        if (Origin == null)
        {
            Debug.Log("Moving player projectile");
        }
        if (coolDown == 0)
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
            Tile this_tile = tileMap.TileArray[PositionX, PositionY];
            Tile next_tile = tileMap.TileArray[facing_x, facing_y];
            if (controller.PlayerXPos == facing_x & controller.PlayerYPos == facing_y)
            {
                controller.Projectiles.Remove(this);
                if (Origin != null) // no friendly fire
                {
                    controller.Hit(this);
                }
                this_tile.Contents = TileContents.EMPTY_TILE;
            }
            else if (next_tile.Contents == TileContents.EMPTY_TILE)
            {
                this_tile.Contents = TileContents.EMPTY_TILE;
                next_tile.Contents = this.tileContent;
                PositionX = facing_x;
                PositionY = facing_y;
            }
            else
            {
                this_tile.Contents = TileContents.EMPTY_TILE;
                controller.Projectiles.Remove(this);
            }
        } else
        {
            coolDown--;
        }
    }
}
