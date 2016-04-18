using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{

    public TextAsset[] Levels;
    public string LevelFolder;
    //public GameObject LevelTextPrefab;
    //public Transform LevelTextCanvas;
    //public Transform LevelTilesParent;

    //public int GridSize = 16;
    public GameController Controller;

    private TextAsset levelTiles;
    private TileMap levelTileMap;
    //private TextAsset levelTilePrefabs;
    //private TextAsset levelText;
    //private TextAsset levelEntities;

    // Use this for initialization
    void Start()
    {
        string[] levelDef = Regex.Split(Levels[0].ToString(), "\r\n");
        int level_x = int.Parse(levelDef[0]);
        int level_y = int.Parse(levelDef[1]);
        levelTileMap = new TileMap(level_x, level_y);
        TextAsset level_tiles = Instantiate(Resources.Load(LevelFolder + levelDef[2], typeof(TextAsset)) as TextAsset);
        //levelTilePrefabs = Instantiate(Resources.Load(LevelFolder + levelDef[1], typeof(TextAsset)) as TextAsset);
        //levelText = Instantiate(Resources.Load(LevelFolder + levelDef[2], typeof(TextAsset)) as TextAsset);
        //levelEntities = Instantiate(Resources.Load(LevelFolder + levelDef[3], typeof(TextAsset)) as TextAsset);
        LoadLevelTiles(level_tiles, levelTileMap);
        //LoadLevelText(levelText);
        //LoadLevelEntities(levelEntities);
        Controller.TileMap = levelTileMap;
    }

    //private void LoadLevelEntities(TextAsset entities)
    //{
    //    CSVHelper csv = new CSVHelper(entities.ToString(), ",");

    //    foreach (string[] line in csv)
    //    {
    //        GameObject newEntity = Instantiate(Resources.Load(PrefabFolder + line[0], typeof(GameObject)) as GameObject);
    //        newEntity.transform.position = new Vector2(float.Parse(line[1]), float.Parse(line[2]));
    //        newEntity.transform.SetParent(LevelTilesParent.transform, true);
    //    }
    //}

    private void LoadLevelTiles(TextAsset tiles, TileMap tile_map)
    {
        //string[] prefabDef = Regex.Split(prefabs.ToString(), "\r\n");

        CSVHelper csv = new CSVHelper(tiles.ToString(), ",");
        int lineNum = 0;
        foreach (string[] line in csv)
        {
            int yCoordinate = (csv.Count - (lineNum + 1));
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != string.Empty)
                {
                    int value = int.Parse(line[i]);
                    switch (value)
                    {
                        case -1:
                            Controller.PlayerXPos = i;
                            Controller.PlayerYPos = yCoordinate;
                            break;
                        case (int)TileContents.DUMB_BOT:
                            new DumbBot(i, yCoordinate, levelTileMap, Controller);
                            break;
                        default:
                            levelTileMap.TileArray[i, yCoordinate].Contents = (TileContents)int.Parse(line[i]);
                            break;
                    }
                }
            }
            lineNum++;
        }

    }

    //private void LoadLevelText(TextAsset text)
    //{
    //    CSVHelper csv = new CSVHelper(text.ToString(), ",");
    //    foreach (string[] line in csv)
    //    {
    //        GameObject newTextGameObject = GameObject.Instantiate(LevelTextPrefab);
    //        Text newText = newTextGameObject.GetComponent<Text>();
    //        newText.text = line[0];
    //        newText.transform.position = new Vector2(float.Parse(line[1]), float.Parse(line[2]));
    //        newText.transform.SetParent(LevelTextCanvas.transform, true);
    //    }
    //}
}
