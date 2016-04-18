using UnityEngine;

public class Projectile
{
    public int PositionX;
    public int PositionY;
    public Enemy Origin;

    private GameController controller;
    private TileMap tileMap;
    private Direction direction;
    private int damage;
    private TileContents tileContent;
    private int coolDown;

    //public Projectile(int positionX, int positionY, GameController controller, TileMap tileMap, Direction direction, Enemy origin)
    //{
    //    damage = 10;
    //    Initialize(positionX, positionY, controller, tileMap, direction, origin);
    //}

    public Projectile(int positionX, int positionY, GameController controller, TileMap tileMap, Direction direction, Enemy origin, int damage)
    {
        this.damage = damage;
        Initialize(positionX, positionY, controller, tileMap, direction, origin);
    }

    private void Initialize(int positionX, int positionY, GameController controller, TileMap tileMap, Direction direction, Enemy origin)
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
    public void Destroy()
    {
        tileMap.TileArray[PositionX, PositionY].Contents = TileContents.EMPTY_TILE;
        controller.Projectiles.Remove(this);
    }

    public void Move()
    {
        if (Origin == null)
        {
            Debug.LogFormat("Moving player projectile at {0}, {1}", PositionX, PositionY);
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
                // hit a player
                Destroy();
                if (Origin != null) // no friendly fire
                {
                    controller.Hit(this, damage);
                    controller.MessageList.Add("YOU ARE HIT!");
                }
            }
            else if (next_tile.Contents == TileContents.EMPTY_TILE)
            {
                // move into an empty tile
                this_tile.Contents = TileContents.EMPTY_TILE;
                next_tile.Contents = this.tileContent;
                PositionX = facing_x;
                PositionY = facing_y;
            }
            else if (next_tile.Contents == TileContents.LASER_N |
                     next_tile.Contents == TileContents.LASER_E |
                     next_tile.Contents == TileContents.LASER_S |
                     next_tile.Contents == TileContents.LASER_W)
            {
                // hit a projectile
                Projectile hit = controller.GetProjectileAtTile(facing_x, facing_y);
                if (Origin != null)
                {
                    Debug.LogFormat("Projectile destroyed by projectile from {0}", Origin.Name);
                } else
                {
                    Debug.Log("Projectile destroyed by projectile from player");
                }
                hit.Destroy();
                Destroy();
            }
            else if (next_tile.Contents == TileContents.DUMB_BOT |
                     next_tile.Contents == TileContents.SENTINEL_BOT_EW |
                     next_tile.Contents == TileContents.SENTINEL_BOT_NS)
            {
                Enemy hit = controller.GetEnemyAtTile(facing_x, facing_y);
                hit.Hit(damage);
                if (Origin != null)
                {
                    Debug.LogFormat("{0} hit by projectile from {1}", hit.Name, Origin.Name);
                }
                else
                {
                    controller.MessageList.Add("YOUR LASER HITS!");
                    Debug.LogFormat("{0} hit by projectile from player", hit.Name);
                }
                Destroy();
            }
            else
            {
                Destroy();
            }
        }
        else
        {
            coolDown--;
        }
    }
}
