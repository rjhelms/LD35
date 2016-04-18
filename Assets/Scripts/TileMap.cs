public enum TileContents
{
    EMPTY_TILE,
    WALL,
    EXIT_STAIRS,
    DUMB_BOT,
    SENTINEL_BOT_EW,
    SENTINEL_BOT_NS,
    LASER_N,
    LASER_E,
    LASER_S,
    LASER_W,
    POWERUP_BATTERY,
    POWERUP_HEAD_OMNI,
    POWERUP_HEAD_IR,
    POWERUP_HEAD_LONGRANGE,
    POWERUP_CHASSIS_SILENT,
    POWERUP_CHASSIS_FAST,
    POWERUP_CHASSIS_OFFROAD,
    POWERUP_TOOL_LASER,
    POWERUP_TOOL_ACTUATOR,
    POWERUP_TOOL_PROBE,
    RUBBLE,
}

public class Tile
{
    public TileContents Contents;
    public bool Visible;
    public TileContents KnownContents;

    public Tile(TileContents contents)
    {
        Contents = contents;
        KnownContents = TileContents.EMPTY_TILE;
    }

    public void SetVisible()
    {
        KnownContents = Contents;
        Visible = true;
    }

    public void SetInvisible()
    {
        Visible = false;
    }

    public bool CanEnterOffroad()
    {
        bool non_offroad = CanEnter();
        switch (Contents)
        {
            case TileContents.RUBBLE:
                return true;
            default:
                return non_offroad;
        }
    }

    public bool CanEnter()
    {
        switch (Contents)
        {
            case TileContents.EMPTY_TILE:
                return true;
            case TileContents.EXIT_STAIRS:
                return true;
            case TileContents.LASER_E:
                return true;
            case TileContents.LASER_N:
                return true;
            case TileContents.LASER_S:
                return true;
            case TileContents.LASER_W:
                return true;
            case TileContents.POWERUP_BATTERY:
                return true;
            case TileContents.POWERUP_HEAD_OMNI:
                return true;
            case TileContents.POWERUP_HEAD_IR:
                return true;
            case TileContents.POWERUP_HEAD_LONGRANGE:
                return true;
            case TileContents.POWERUP_CHASSIS_SILENT:
                return true;
            case TileContents.POWERUP_CHASSIS_FAST:
                return true;
            case TileContents.POWERUP_CHASSIS_OFFROAD:
                return true;
            case TileContents.POWERUP_TOOL_LASER:
                return true;
            case TileContents.POWERUP_TOOL_ACTUATOR:
                return true;
            case TileContents.POWERUP_TOOL_PROBE:
                return true;
            default:
                return false;
        }
    }

    public bool CanSeeThrough()
    {
        switch (Contents)
        {
            case TileContents.EMPTY_TILE:
                return true;
            case TileContents.WALL:
                return false;
            case TileContents.EXIT_STAIRS:
                return true;
            case TileContents.LASER_E:
                return true;
            case TileContents.LASER_N:
                return true;
            case TileContents.LASER_S:
                return true;
            case TileContents.LASER_W:
                return true;
            case TileContents.POWERUP_BATTERY:
                return true;
            case TileContents.POWERUP_HEAD_OMNI:
                return true;
            case TileContents.POWERUP_HEAD_IR:
                return true;
            case TileContents.POWERUP_HEAD_LONGRANGE:
                return true;
            case TileContents.POWERUP_CHASSIS_SILENT:
                return true;
            case TileContents.POWERUP_CHASSIS_FAST:
                return true;
            case TileContents.POWERUP_CHASSIS_OFFROAD:
                return true;
            case TileContents.POWERUP_TOOL_LASER:
                return true;
            case TileContents.POWERUP_TOOL_ACTUATOR:
                return true;
            case TileContents.POWERUP_TOOL_PROBE:
                return true;
            default:
                return false;
        }
    }
}

public class TileMap
{

    public Tile[,] TileArray;

    public TileMap(int x_size, int y_size)
    {
        TileArray = new Tile[x_size, y_size];
        for (int x = 0; x < x_size; x++)
        {
            for (int y = 0; y < y_size; y++)
            {
                TileArray[x, y] = new Tile(TileContents.EMPTY_TILE);
            }
        }
    }

}
