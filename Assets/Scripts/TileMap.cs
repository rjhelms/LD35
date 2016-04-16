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
}

public class TileMap
{

    public Tile[,] TileArray;

}
