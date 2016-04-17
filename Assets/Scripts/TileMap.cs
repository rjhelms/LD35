public enum TileContents
{
    EMPTY_TILE,
    WALL
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

    public bool CanEnter()
    {
        if (Contents == TileContents.EMPTY_TILE)
        {
            return true;
        } else
        {
            return false;
        }
    }
}

public class TileMap
{

    public Tile[,] TileArray;

    public TileMap()
    {
        TileArray = new Tile[11, 11];
        for (int x = 0; x < 11; x++)
        {
            for (int y = 0; y < 11; y++)
            {
                if (x < 3 | x > 7 | y < 3 | y > 7)
                {
                    TileArray[x, y] = new Tile(TileContents.WALL);
                }
                else
                {
                    TileArray[x, y] = new Tile(TileContents.EMPTY_TILE);
                }
            }
        }
    }

}
