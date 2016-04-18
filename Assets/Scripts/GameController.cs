using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#region State Enums
public enum SensorState
{
    BASIC,
    OMNI,
    INFRARED,
    LONGRANGE
}

public enum ChassisState
{
    BASIC,
    SILENT,
    FAST,
    OFFROAD
}

public enum ToolState
{
    NONE,
    LASER,
    ACTUATOR,
    PROBE
}

public enum GameState
{
    MOVEMENT,
    SELECTION,
    WAIT,
    NOTIFICATION,
    LEVEL_WON,
    LOST
}
#endregion

public class GameController : MonoBehaviour
{
    #region Public Attributes
    public Material RenderMaterial;
    public Camera WorldCamera;

    public int TargetX = 320;
    public int TargetY = 200;

    public int PlayerXPos;
    public int PlayerYPos;

    public int XSpriteOffset = 16;
    public int XSpriteSize = 32;
    public int YSpriteOffset = 13;
    public int YSpriteSize = 25;

    public Color ActiveColour;
    public Color InactiveColour;
    public Color ErrorColour;

    public Text[] SensorText;
    public Text[] ChassisText;
    public Text[] ToolText;
    public Text PointerText;
    public Text CountText;
    public Text BatteryText;
    public Text MessageText;
    public Text NotificationText;

    public Canvas NotificationCanvas;

    public GameObject PrefabTileSprite;

    public SpriteRenderer[,] VisibleSprites;
    public Transform VisibleSpritesParent;

    public bool EGAMode = true;

    public TileMap TileMap;

    public SpriteRenderer HeadSprite;
    public SpriteRenderer BodySprite;
    public SpriteRenderer ToolSprite;

    public List<Enemy> Enemies = new List<Enemy>();
    public List<Projectile> Projectiles = new List<Projectile>();

    public int StartingHitPoints = 100;
    public int MaxHitPoints = 150;
    public int PlayerLaserDamage = 10;
    public int BatteryHitPoints = 50;
    public bool MadeNoiseLastMove = false;
    #endregion

    #region Private Attributes
    private int playerTick;

    private SpriteDefinitions spriteDefinitions;
    private Sprite[] headSpriteArray;

    private Direction direction;
    private SensorState sensorState;
    private ChassisState chassisState;
    private ToolState toolState;
    private GameState gameState;
    private Sensor sensor;
    private Chassis chassis;

    private int activeCount = 0;
    private int maxCount = 1;
    private int pointerPosition;
    private int lastPlayerTick;
    private int globalTick = 0;
    private float notificationEndTime;
    #endregion

    #region Internal Properites
    internal Chassis Chassis
    {
        get
        {
            return chassis;
        }
    }
    #endregion

    #region MonoBehaviour Methods
    // Use this for initialization
    void Start()
    {
        spriteDefinitions = FindObjectOfType<SpriteDefinitions>();
        initializePlayer();
        initializeUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            maxCount++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            maxCount--;
        }
        switch (gameState)
        {
            case GameState.MOVEMENT:
                doMovement();
                if (lastPlayerTick != playerTick)
                {

                    lastPlayerTick = playerTick;
                }

                break;
            case GameState.SELECTION:
                doSelection();
                break;
            case GameState.NOTIFICATION:
                doNotification();
                break;
        }

        updateUI();

    }

    private void doNotification()
    {
        if (Time.time < notificationEndTime)
            return;

        NotificationCanvas.gameObject.SetActive(false);
        setState(GameState.MOVEMENT);
        updateMap();

    }
    #endregion

    #region Public Methods
    public void Hit(Projectile projectile, int damage)
    {
        if (projectile.Origin != null)
        {
            Debug.Log("Hit by projectile from " + projectile.Origin.Name);
            takeDamage(damage);
        }
    }

    public void Hit(Enemy enemy, int damage)
    {
        Debug.Log("Hit directly by " + enemy.Name);
        takeDamage(damage);
    }

    public Enemy GetEnemyAtTile(int tile_x, int tile_y)
    {
        foreach (Enemy item in Enemies)
        {
            if (item.PositionX == tile_x & item.PositionY == tile_y)
            {
                return item;
            }
        }
        return null;
    }

    public Projectile GetProjectileAtTile(int tile_x, int tile_y)
    {
        foreach (Projectile item in Projectiles)
        {
            if (item.PositionX == tile_x & item.PositionY == tile_y)
            {
                return item;
            }
        }
        return null;
    }

    public void Notify(string v)
    {
        NotificationText.text = v;
        NotificationCanvas.gameObject.SetActive(true);
        setState(GameState.NOTIFICATION);
    }
    #endregion

    #region Private Methods

    private void takeDamage(int damage)
    {
        ScoreManager.Instance.HitPoints -= damage;
        if (ScoreManager.Instance.HitPoints < 0)
            ScoreManager.Instance.HitPoints = 0;
        if (ScoreManager.Instance.HitPoints == 0)
            setState(GameState.LOST);
    }
    private void updateMap()
    {
        Debug.LogFormat("Updating map, tick {0}", globalTick);
        sensor.Scan(PlayerXPos, PlayerYPos, this.direction);
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                int x_array_pos = (x - 3) + PlayerXPos;
                int y_array_pos = (y - 3) + PlayerYPos;
                Tile this_tile = TileMap.TileArray[x_array_pos, y_array_pos];

                if (this_tile.Visible)
                {
                    VisibleSprites[x, y].sprite = spriteDefinitions.EGAVisibleSprites[(int)this_tile.KnownContents];
                }
                else
                {
                    VisibleSprites[x, y].sprite = spriteDefinitions.EGAInvisibleSprites[(int)this_tile.KnownContents];
                }
            }
        }
    }

    private void updateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            SensorText[i].color = InactiveColour;
            SensorText[i].gameObject.SetActive(ScoreManager.Instance.SensorsAvailable[i]);
            ChassisText[i].color = InactiveColour;
            ChassisText[i].gameObject.SetActive(ScoreManager.Instance.ChassisAvailable[i]);
            ToolText[i].color = InactiveColour;
            ToolText[i].gameObject.SetActive(ScoreManager.Instance.ToolsAvailable[i]);
        }

        SensorText[(int)sensorState].color = ActiveColour;
        ChassisText[(int)chassisState].color = ActiveColour;
        ToolText[(int)toolState].color = ActiveColour;

        if (pointerPosition < 4)        // is pointing at a Sensor
        {
            PointerText.transform.position = new Vector2(PointerText.transform.position.x,
                                                         SensorText[pointerPosition].transform.position.y);
        }
        else if (pointerPosition < 8)   // is pointing at a Chassis
        {
            PointerText.transform.position = new Vector2(PointerText.transform.position.x,
                                                         ChassisText[pointerPosition - 4].transform.position.y);
        }
        else                            // pointing at a Tool
        {
            PointerText.transform.position = new Vector2(PointerText.transform.position.x,
                                                         ToolText[pointerPosition - 8].transform.position.y);
        }

        CountText.text = string.Format("ACTIVE  {0}\r\nMAX     {1}", activeCount, maxCount);

        if (activeCount > maxCount)
        {
            CountText.color = ErrorColour;
        }
        else
        {
            CountText.color = InactiveColour;
        }

        HeadSprite.sprite = headSpriteArray[(int)direction];
        BodySprite.sprite = spriteDefinitions.EGAChassis[(int)chassisState];
        ToolSprite.sprite = spriteDefinitions.EGATool[(int)toolState];
        if (direction == Direction.WEST | direction == Direction.SOUTH)
        {
            ToolSprite.flipX = true;
        }
        else
        {
            ToolSprite.flipX = false;
        }
        BatteryText.text = string.Format("BATTERY: {0,3}%", ScoreManager.Instance.HitPoints);
    }

    private void doMovement()
    {
        if (TileMap.TileArray[PlayerXPos, PlayerYPos].Contents == TileContents.EXIT_STAIRS)
        {
            setState(GameState.LEVEL_WON);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            setState(GameState.SELECTION);
            return;
        }
        bool moved = false;
        int new_x_pos = PlayerXPos;
        int new_y_pos = PlayerYPos;
        int forward_move = 0;
        int lateral_move = 0;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            this.direction--;
            if (this.direction < Direction.NORTH)
            {
                this.direction = Direction.WEST;
            }
            moved = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            this.direction++;
            if (this.direction > Direction.WEST)
            {
                this.direction = Direction.NORTH;
            }
            moved = true;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            forward_move++;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            forward_move--;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            lateral_move--;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            lateral_move++;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (toolState)
            {
                case ToolState.NONE:
                    break;
                case ToolState.LASER:
                    if (playerFire())
                        moved = true;
                    break;
                case ToolState.ACTUATOR:
                    break;
                case ToolState.PROBE:
                    break;


            }
        }
        if (forward_move != 0 | lateral_move != 0)
        {
            moved = true;
        }

        if (moved)
        {
            switch (this.direction)
            {
                case Direction.NORTH:
                    new_y_pos += forward_move;
                    new_x_pos += lateral_move;
                    break;
                case Direction.EAST:
                    new_x_pos += forward_move;
                    new_y_pos -= lateral_move;
                    break;
                case Direction.SOUTH:
                    new_y_pos -= forward_move;
                    new_x_pos -= lateral_move;
                    break;
                case Direction.WEST:
                    new_x_pos -= forward_move;
                    new_y_pos += lateral_move;
                    break;
            }

            if (TileMap.TileArray[new_x_pos, new_y_pos].CanEnter())
            {
                Debug.LogFormat("Player moved at {0}, {1}", PlayerXPos, PlayerYPos);
                PlayerXPos = new_x_pos;
                PlayerYPos = new_y_pos;
                TileMap.TileArray[PlayerXPos, PlayerYPos].SetInvisible();
                Tick();
                if (chassisState != ChassisState.SILENT)
                {
                    MadeNoiseLastMove = true;
                }
                if (TileMap.TileArray[PlayerXPos, PlayerYPos].Contents >= TileContents.POWERUP_BATTERY &
                    TileMap.TileArray[PlayerXPos, PlayerYPos].Contents <= TileContents.POWERUP_TOOL_PROBE)
                {
                    getPowerUp(TileMap.TileArray[PlayerXPos, PlayerYPos]);
                }
            }
            else
            {
                TileMap.TileArray[new_x_pos, new_y_pos].SetVisible();
            }
        }
    }

    private void getPowerUp(Tile tile)
    {
        switch (tile.Contents)
        {
            case TileContents.POWERUP_BATTERY:
                if (ScoreManager.Instance.HitPoints < MaxHitPoints)
                {
                    Debug.Log("Got a battery!");
                    ScoreManager.Instance.HitPoints += BatteryHitPoints;
                    tile.Contents = TileContents.EMPTY_TILE;
                }
                else
                {
                    Debug.Log("Found a battery, but already fully charged.");
                }
                break;
            case TileContents.POWERUP_HEAD_OMNI:
                if (!ScoreManager.Instance.SensorsAvailable[(int)SensorState.OMNI])
                {
                    Debug.Log("Got the omni head!");
                    ScoreManager.Instance.SensorsAvailable[(int)SensorState.OMNI] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE OMNI SENSOR\r\n\r\n\r\nTHIS SENSOR CAN SEE \r\nIN ALL DIRECTIONS \r\n\r\n\r\n\r\n");
                }
                break;
            case TileContents.POWERUP_HEAD_IR:
                if (!ScoreManager.Instance.SensorsAvailable[(int)SensorState.INFRARED])
                {
                    Debug.Log("Got the IR head!");
                    ScoreManager.Instance.SensorsAvailable[(int)SensorState.INFRARED] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE INFRARED SENSOR\r\n\r\n\r\nTHIS SENSOR CAN SEE \r\nTHROUGH WALLS\r\n\r\n\r\n\r\n");
                }
                break;
            case TileContents.POWERUP_HEAD_LONGRANGE:
                if (!ScoreManager.Instance.SensorsAvailable[(int)SensorState.LONGRANGE])
                {
                    Debug.Log("Got the long range head!");
                    ScoreManager.Instance.SensorsAvailable[(int)SensorState.LONGRANGE] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE LONG RANGE \r\nSENSOR\r\n\r\nTHIS SENSOR CAN SEE \r\nONE TILE FARTHER\r\n\r\n\r\n\r\n");
                }
                break;
            case TileContents.POWERUP_CHASSIS_SILENT:
                if (!ScoreManager.Instance.ChassisAvailable[(int)ChassisState.SILENT])
                {
                    Debug.Log("Got the silent chassis!");
                    ScoreManager.Instance.ChassisAvailable[(int)ChassisState.SILENT] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE SILENT CHASSIS\r\n\r\n\r\nENEMIES WILL NOT HEAR YOU \r\nMOVING WITH THIS EQUIPPED \r\n\r\n\r\n");
                }
                break;
            case TileContents.POWERUP_CHASSIS_FAST:
                if (!ScoreManager.Instance.ChassisAvailable[(int)ChassisState.FAST])
                {
                    Debug.Log("Got the fast chassis!");
                    ScoreManager.Instance.ChassisAvailable[(int)ChassisState.FAST] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE FAST CHASSIS\r\n\r\n\r\nYOU WILL MOVE MUCH FASTER \r\nTHAN YOUR ENEMIES WITH\r\nTHIS EQUIPPED \r\n\r\n");
                }
                break;
            case TileContents.POWERUP_CHASSIS_OFFROAD:
                if (!ScoreManager.Instance.ChassisAvailable[(int)ChassisState.OFFROAD])
                {
                    Debug.Log("Got the offroad chassis!");
                    ScoreManager.Instance.ChassisAvailable[(int)ChassisState.OFFROAD] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE OFF-ROAD CHASSIS\r\n\r\n\r\nYOU CAN RUN OVER SMALL\r\nOBSTACLES WITH THIS\r\nEQUIPPED\r\n\r\n");
                }
                break;
            case TileContents.POWERUP_TOOL_LASER:
                if (!ScoreManager.Instance.ToolsAvailable[(int)ToolState.LASER])
                {
                    Debug.Log("Got the laser!");
                    ScoreManager.Instance.ToolsAvailable[(int)ToolState.LASER] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE LASER \r\n\r\n\r\nYOU CAN BLAST YOUR ENEMIES\r\nTO BITS WITH THIS \r\n\r\n\r\n");
                }
                break;
            case TileContents.POWERUP_TOOL_ACTUATOR:
                if (!ScoreManager.Instance.ToolsAvailable[(int)ToolState.ACTUATOR])
                {
                    Debug.Log("Got the actuator!");
                    ScoreManager.Instance.ToolsAvailable[(int)ToolState.ACTUATOR] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE ACTUATOR\r\n\r\n\r\nYOU CAN OPEN DOORS AND USE\r\nSWITCHES WITH THIS\r\n\r\n\r\n\r\n");
                }
                break;
            case TileContents.POWERUP_TOOL_PROBE:
                if (!ScoreManager.Instance.ToolsAvailable[(int)ToolState.PROBE])
                {
                    Debug.Log("Got the probe!");
                    ScoreManager.Instance.ToolsAvailable[(int)ToolState.PROBE] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE PROBE \r\n\r\n\r\nYOU CAN DISABLE COMPUTER\r\nTERMINALS WITH THIS \r\n\r\n\r\n\r\n");
                }
                break;
        }
        updateMap();
    }



    private void Tick()
    {

        lastPlayerTick = playerTick;
        playerTick++;
        globalTick++;
        Debug.LogFormat("Tick {0}, player tick {1}", globalTick, playerTick);
        doEnemyMovement();

        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            Projectiles[i].Move();
        }

        // battery over 100% fades each tick
        if (ScoreManager.Instance.HitPoints > 100)
        {
            ScoreManager.Instance.HitPoints--;
        }
        updateMap();
    }

    private bool playerFire()
    {
        bool moved = false;
        int facing_x = PlayerXPos;
        int facing_y = PlayerYPos;
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
        {
            if (TileMap.TileArray[facing_x, facing_y].Contents == TileContents.EMPTY_TILE)
            {
                new Projectile(PlayerXPos, PlayerYPos, this, TileMap, direction, null, PlayerLaserDamage);
                MadeNoiseLastMove = true;
                moved = true;
            }
            else if (TileMap.TileArray[facing_x, facing_y].Contents == TileContents.DUMB_BOT |
                     TileMap.TileArray[facing_x, facing_y].Contents == TileContents.SENTINEL_BOT_EW |
                     TileMap.TileArray[facing_x, facing_y].Contents == TileContents.SENTINEL_BOT_NS)
            {
                Enemy hit = GetEnemyAtTile(facing_x, facing_y);
                Debug.Log("Player hit " + hit.Name + " directly");
                hit.Hit(PlayerLaserDamage);
                MadeNoiseLastMove = true;
                moved = true;
            }
            else if (TileMap.TileArray[facing_x, facing_y].Contents == TileContents.LASER_N |
                     TileMap.TileArray[facing_x, facing_y].Contents == TileContents.LASER_E |
                     TileMap.TileArray[facing_x, facing_y].Contents == TileContents.LASER_S |
                     TileMap.TileArray[facing_x, facing_y].Contents == TileContents.LASER_W)
            {
                Projectile hit = GetProjectileAtTile(facing_x, facing_y);
                Debug.Log("Player destroyed projectile directly");
                hit.Destroy();
                MadeNoiseLastMove = true;
                moved = true;
                doEnemyMovement();
            }
        }

        return moved;
    }

    private void doEnemyMovement()
    {
        if (playerTick >= chassis.MaxTicks)
        {
            playerTick = 0;
            lastPlayerTick = 0;
            Debug.LogFormat("Processing enemy moves, noise {0}", MadeNoiseLastMove);
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemies[i].Move();
            }

            MadeNoiseLastMove = false;
        }
    }

    private void doSelection()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (activeCount <= maxCount)
            {
                setState(GameState.MOVEMENT);
                Tick();
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) | Input.GetKeyDown(KeyCode.S))
        {
            pointerPosition++;
            bool validPointerPosition = false;
            while (!validPointerPosition)
            {
                if (pointerPosition == 12)
                {
                    pointerPosition = 0;
                }
                if (pointerPosition < 4)
                {
                    if (ScoreManager.Instance.SensorsAvailable[pointerPosition])
                        validPointerPosition = true;
                    else pointerPosition++;
                }
                else if (pointerPosition >= 4 & pointerPosition < 8)
                {
                    if (ScoreManager.Instance.ChassisAvailable[pointerPosition - 4])
                        validPointerPosition = true;
                    else pointerPosition++;
                }
                else if (pointerPosition >= 8)
                {
                    if (ScoreManager.Instance.ToolsAvailable[pointerPosition - 8])
                        validPointerPosition = true;
                    else pointerPosition++;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) | Input.GetKeyDown(KeyCode.W))
        {
            pointerPosition--;
            bool validPointerPosition = false;
            while (!validPointerPosition)
            {
                if (pointerPosition == -1)
                {
                    pointerPosition = 11;
                }
                if (pointerPosition < 4)
                {
                    if (ScoreManager.Instance.SensorsAvailable[pointerPosition])
                        validPointerPosition = true;
                    else pointerPosition--;
                }
                else if (pointerPosition >= 4 & pointerPosition < 8)
                {
                    if (ScoreManager.Instance.ChassisAvailable[pointerPosition - 4])
                        validPointerPosition = true;
                    else pointerPosition--;
                }
                else if (pointerPosition >= 8)
                {
                    if (ScoreManager.Instance.ToolsAvailable[pointerPosition - 8])
                        validPointerPosition = true;
                    else pointerPosition--;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (pointerPosition)
            {
                case 0:
                    setSensor(SensorState.BASIC);
                    break;
                case 1:
                    setSensor(SensorState.OMNI);
                    break;
                case 2:
                    setSensor(SensorState.INFRARED);
                    break;
                case 3:
                    setSensor(SensorState.LONGRANGE);
                    break;
                case 4:
                    setChassis(ChassisState.BASIC);
                    break;
                case 5:
                    setChassis(ChassisState.SILENT);
                    break;
                case 6:
                    setChassis(ChassisState.FAST);
                    break;
                case 7:
                    setChassis(ChassisState.OFFROAD);
                    break;
                case 8:
                    setTool(ToolState.NONE);
                    break;
                case 9:
                    setTool(ToolState.LASER);
                    break;
                case 10:
                    setTool(ToolState.ACTUATOR);
                    break;
                case 11:
                    setTool(ToolState.PROBE);
                    break;
            }

            activeCount = 0;
            if (sensorState > SensorState.BASIC)
                activeCount++;
            if (chassisState > ChassisState.BASIC)
                activeCount++;
            if (toolState > ToolState.NONE)
                activeCount++;
        }
    }

    private void initializePlayer()
    {
        direction = Direction.NORTH;
        setSensor(SensorState.BASIC);
        setChassis(ChassisState.BASIC);
        setTool(ToolState.NONE);
        gameState = GameState.MOVEMENT;
        playerTick = 0;
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                TileMap.TileArray[PlayerXPos + x, PlayerYPos + y].SetVisible();
            }
        }
    }

    private void initializeUI()
    {
        VisibleSprites = new SpriteRenderer[7, 7];
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                float spriteX = XSpriteOffset + (XSpriteSize * x);
                float spriteY = YSpriteOffset + (YSpriteSize * y);
                GameObject o = (GameObject)Instantiate(PrefabTileSprite, new Vector3(spriteX, spriteY), Quaternion.identity);
                SpriteRenderer newSprite = o.GetComponent<SpriteRenderer>();
                newSprite.sprite = spriteDefinitions.EGAInvisibleSprites[(int)TileContents.EMPTY_TILE];
                newSprite.transform.parent = VisibleSpritesParent;
                VisibleSprites[x, y] = newSprite;
            }
        }

        float pixelRatioAdjustment = (float)TargetX / (float)TargetY;
        if (pixelRatioAdjustment <= 1)
        {
            RenderMaterial.mainTextureScale = new Vector2(pixelRatioAdjustment, 1);
            RenderMaterial.mainTextureOffset = new Vector2((1 - pixelRatioAdjustment) / 2, 0);
            WorldCamera.orthographicSize = TargetY / 2;
        }
        else
        {
            pixelRatioAdjustment = 1f / pixelRatioAdjustment;
            RenderMaterial.mainTextureScale = new Vector2(1, pixelRatioAdjustment);
            RenderMaterial.mainTextureOffset = new Vector2(0, (1 - pixelRatioAdjustment) / 2);
            WorldCamera.orthographicSize = TargetX / 2;
        }

        pointerPosition = 0;
        PointerText.color = InactiveColour;
        updateMap();
    }

    private void setChassis(ChassisState state)
    {
        chassisState = state;
        chassis = new Chassis(state);
    }

    private void setSensor(SensorState state)
    {
        sensorState = state;
        switch (state)
        {
            case SensorState.BASIC:
                sensor = new DirectionalSensor(TileMap);
                headSpriteArray = spriteDefinitions.EGADirectionalHead;
                break;
            case SensorState.OMNI:
                sensor = new OmniSensor(TileMap);
                headSpriteArray = spriteDefinitions.EGAOmniHead;
                break;
            case SensorState.INFRARED:
                sensor = new IRSensor(TileMap);
                headSpriteArray = spriteDefinitions.EGAIRHead;
                break;
            case SensorState.LONGRANGE:
                sensor = new LongRangeSensor(TileMap);
                headSpriteArray = spriteDefinitions.EGALongRangeHead;
                break;
        }
    }

    private void setState(GameState state)
    {
        switch (state)
        {
            case GameState.MOVEMENT:
                Debug.Log("Entering movement state");
                PointerText.gameObject.SetActive(false);
                break;
            case GameState.SELECTION:
                Debug.Log("Entering selection state");
                pointerPosition = (int)sensorState;
                PointerText.gameObject.SetActive(true);
                break;
            case GameState.LEVEL_WON:
                ScoreManager.Instance.CurrentLevel++;
                if (ScoreManager.Instance.CurrentLevel == ScoreManager.Instance.MaxLevels)
                {
                    Debug.LogError("You won!");
                }
                else
                {
                    SceneManager.LoadScene("level");
                }
                break;
            case GameState.LOST:
                Debug.LogError("You lost!");
                break;
            case GameState.NOTIFICATION:
                notificationEndTime = Time.time + 2f;
                break;
        }
        gameState = state;
    }

    private void setTool(ToolState state)
    {
        toolState = state;
    }
    #endregion
}
