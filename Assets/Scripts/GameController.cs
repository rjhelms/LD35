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

#region AudioEnum
public enum Sound
{
    PLAYER_HIT,
    ENEMY_HIT,
    ENEMY_DESTROYED,
    POWER_UP,
    LASER_FIRE,
    DOOR_OPEN,
    TERMINAL_HACK,
    SWITCH_PULL,
    SELECT
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
    public Text LevelText;
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
    public string LevelString;
    public string SelectString = "CONFIGURING BOT: TAB WHEN DONE";
    public string TooManyString = "TOO MANY PARTS SELECTED";
    public List<string> MessageList;
    public float messageScrollTime = 0.8f;

    public bool[] startSensorsAvailable;
    public bool[] startChassisAvailable;
    public bool[] startToolsAvailable;
    public AudioClip[] Sounds;
    public AudioSource AudioSource;
    #endregion

    #region Private Attributes
    private int playerTick;

    private SpriteDefinitions spriteDefinitions;
    private Sprite[] headSpriteArray;

    private Direction direction;
    private GameState gameState;
    private Sensor sensor;
    private Chassis chassis;

    private int pointerPosition;
    private int lastPlayerTick;
    private int globalTick = 0;
    private float notificationEndTime;

    private string[] Messages;

    private int startHitPoints;
    private int startActiveCount;
    private int startMaxCount;
    private SensorState startSensorState;
    private ChassisState startChassisState;
    private ToolState startToolState;

    private float nextMessageTime;
    private int messagePointer;
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
        // debug controls
        if (Input.GetKeyDown(KeyCode.L))
        {
            setState(GameState.LOST);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            setState(GameState.LEVEL_WON);
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            ScoreManager.Instance.MaxPartCount++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ScoreManager.Instance.MaxPartCount--;
        }
        // end debug controls
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
    public void PlaySound(Sound sound)
    {
        AudioSource.PlayOneShot(Sounds[(int)sound]);
    }

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

        SensorText[(int)ScoreManager.Instance.sensorState].color = ActiveColour;
        ChassisText[(int)ScoreManager.Instance.chassisState].color = ActiveColour;
        ToolText[(int)ScoreManager.Instance.toolState].color = ActiveColour;

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

        CountText.text = string.Format("ACTIVE  {0}\r\nMAX     {1}", ScoreManager.Instance.ActivePartCount, ScoreManager.Instance.MaxPartCount);

        if (ScoreManager.Instance.ActivePartCount > ScoreManager.Instance.MaxPartCount)
        {
            CountText.color = ErrorColour;
        }
        else
        {
            CountText.color = InactiveColour;
        }

        HeadSprite.sprite = headSpriteArray[(int)direction];
        BodySprite.sprite = spriteDefinitions.EGAChassis[(int)ScoreManager.Instance.chassisState];
        ToolSprite.sprite = spriteDefinitions.EGATool[(int)ScoreManager.Instance.toolState];
        if (direction == Direction.WEST | direction == Direction.SOUTH)
        {
            ToolSprite.flipX = true;
        }
        else
        {
            ToolSprite.flipX = false;
        }
        BatteryText.text = string.Format("BATTERY: {0,3}%", ScoreManager.Instance.HitPoints);
        displayMessage();
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
            MessageList.Clear();
            moved = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            this.direction++;
            if (this.direction > Direction.WEST)
            {
                this.direction = Direction.NORTH;
            }
            MessageList.Clear();
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
            switch (ScoreManager.Instance.toolState)
            {
                case ToolState.NONE:
                    break;
                case ToolState.LASER:
                    MessageList.Clear();
                    if (playerFire())
                    {
                        moved = true;
                    }
                    break;
                case ToolState.ACTUATOR:
                    MessageList.Clear();
                    if (playerActuate())
                    {
                        moved = true;
                    }
                    break;
                case ToolState.PROBE:
                    MessageList.Clear();
                    if (playerProbe())
                    {
                        moved = true;
                    }
                    break;


            }
        }
        if (forward_move != 0 | lateral_move != 0)
        {
            MessageList.Clear();
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
            bool can_enter = false;
            if (ScoreManager.Instance.chassisState == ChassisState.OFFROAD)
                can_enter = TileMap.TileArray[new_x_pos, new_y_pos].CanEnterOffroad();
            else
            {
                can_enter = TileMap.TileArray[new_x_pos, new_y_pos].CanEnter();
            }
            if (can_enter)
            {
                Debug.LogFormat("Player moved at {0}, {1}", PlayerXPos, PlayerYPos);
                PlayerXPos = new_x_pos;
                PlayerYPos = new_y_pos;
                TileMap.TileArray[PlayerXPos, PlayerYPos].SetInvisible();
                Tick();
                if (ScoreManager.Instance.chassisState != ChassisState.SILENT)
                {
                    MadeNoiseLastMove = true;
                }
                if (TileMap.TileArray[PlayerXPos, PlayerYPos].Contents >= TileContents.POWERUP_BATTERY &
                    TileMap.TileArray[PlayerXPos, PlayerYPos].Contents <= TileContents.POWERUP_CPU)
                {
                    getPowerUp(TileMap.TileArray[PlayerXPos, PlayerYPos]);
                }
            }
            else
            {
                TileMap.TileArray[new_x_pos, new_y_pos].SetVisible();
                switch (TileMap.TileArray[new_x_pos, new_y_pos].Contents)
                {
                    case TileContents.RUBBLE:
                        MessageList.Add("YOU ARE BLOCKED BY RUBBLE");
                        break;
                    case TileContents.BASIC_DOOR:
                        MessageList.Add("YOU ARE BLOCKED BY A DOOR");
                        break;
                    case TileContents.SWITCHED_DOOR:
                        MessageList.Add("YOU ARE BLOCKED BY A DOOR");
                        break;
                    case TileContents.COMPUTER_DOOR:
                        MessageList.Add("YOU ARE BLOCKED BY A DOOR");
                        break;
                    case TileContents.SWITCH:
                        MessageList.Add("YOU ARE BLOCKED BY A SWITCH");
                        break;
                    case TileContents.TERMINAL:
                        MessageList.Add("YOU ARE BLOCKED BY A TERMINAL");
                        break;

                }
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
                    MessageList.Add("FOUND A BATTERY");
                    PlaySound(Sound.POWER_UP);
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
                    MessageList.Add("FOUND THE OMNI SENSOR");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_HEAD_IR:
                if (!ScoreManager.Instance.SensorsAvailable[(int)SensorState.INFRARED])
                {
                    Debug.Log("Got the IR head!");
                    ScoreManager.Instance.SensorsAvailable[(int)SensorState.INFRARED] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE INFRARED SENSOR\r\n\r\n\r\nTHIS SENSOR CAN SEE \r\nTHROUGH WALLS\r\n\r\n\r\n\r\n");
                    MessageList.Add("FOUND THE INFRARED SENSOR");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_HEAD_LONGRANGE:
                if (!ScoreManager.Instance.SensorsAvailable[(int)SensorState.LONGRANGE])
                {
                    Debug.Log("Got the long range head!");
                    ScoreManager.Instance.SensorsAvailable[(int)SensorState.LONGRANGE] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE LONG RANGE \r\nSENSOR\r\n\r\nTHIS SENSOR CAN SEE \r\nONE TILE FARTHER\r\n\r\n\r\n\r\n");
                    MessageList.Add("FOUND THE LONG RANGE SENSOR");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_CHASSIS_SILENT:
                if (!ScoreManager.Instance.ChassisAvailable[(int)ChassisState.SILENT])
                {
                    Debug.Log("Got the silent chassis!");
                    ScoreManager.Instance.ChassisAvailable[(int)ChassisState.SILENT] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE SILENT CHASSIS\r\n\r\n\r\nENEMIES WILL NOT HEAR YOU \r\nMOVING WITH THIS EQUIPPED \r\n\r\n\r\n");
                    MessageList.Add("FOUND THE SILENT CHASSIS");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_CHASSIS_FAST:
                if (!ScoreManager.Instance.ChassisAvailable[(int)ChassisState.FAST])
                {
                    Debug.Log("Got the fast chassis!");
                    ScoreManager.Instance.ChassisAvailable[(int)ChassisState.FAST] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE FAST CHASSIS\r\n\r\n\r\nYOU WILL MOVE MUCH FASTER \r\nTHAN YOUR ENEMIES WITH\r\nTHIS EQUIPPED \r\n\r\n");
                    MessageList.Add("FOUND THE FAST CHASSIS");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_CHASSIS_OFFROAD:
                if (!ScoreManager.Instance.ChassisAvailable[(int)ChassisState.OFFROAD])
                {
                    Debug.Log("Got the offroad chassis!");
                    ScoreManager.Instance.ChassisAvailable[(int)ChassisState.OFFROAD] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE OFF-ROAD CHASSIS\r\n\r\n\r\nYOU CAN RUN OVER\r\nRUBBLE WITH THIS\r\nEQUIPPED\r\n\r\n");
                    MessageList.Add("FOUND THE OFFROAD CHASSIS");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_TOOL_LASER:
                if (!ScoreManager.Instance.ToolsAvailable[(int)ToolState.LASER])
                {
                    Debug.Log("Got the laser!");
                    ScoreManager.Instance.ToolsAvailable[(int)ToolState.LASER] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE LASER \r\n\r\n\r\nYOU CAN BLAST YOUR ENEMIES\r\nTO BITS WITH THIS \r\n\r\n\r\n");
                    MessageList.Add("FOUND THE LASER");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_TOOL_ACTUATOR:
                if (!ScoreManager.Instance.ToolsAvailable[(int)ToolState.ACTUATOR])
                {
                    Debug.Log("Got the actuator!");
                    ScoreManager.Instance.ToolsAvailable[(int)ToolState.ACTUATOR] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE ACTUATOR\r\n\r\n\r\nYOU CAN OPEN DOORS AND USE\r\nSWITCHES WITH THIS\r\n\r\n\r\n\r\n");
                    MessageList.Add("FOUND THE ACTUATOR");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_TOOL_PROBE:
                if (!ScoreManager.Instance.ToolsAvailable[(int)ToolState.PROBE])
                {
                    Debug.Log("Got the probe!");
                    ScoreManager.Instance.ToolsAvailable[(int)ToolState.PROBE] = true;
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND THE PROBE \r\n\r\n\r\nYOU CAN DISABLE COMPUTER\r\nTERMINALS WITH THIS \r\n\r\n\r\n\r\n");
                    MessageList.Add("FOUND THE PROBE");
                    PlaySound(Sound.POWER_UP);
                }
                break;
            case TileContents.POWERUP_CPU:
                if (ScoreManager.Instance.MaxPartCount == 1)
                {
                    tile.Contents = TileContents.EMPTY_TILE;
                    Notify("FOUND A CPU UPGRADE\r\n\r\n\r\nCPU UPGRADES ALLOW YOU TO \r\nHAVE MORE PARTS ACTIVE\r\nAT A TIME \r\n\r\n\r\n");
                    ScoreManager.Instance.MaxPartCount++;
                    tile.Contents = TileContents.EMPTY_TILE;
                    MessageList.Add("FOUND A CPU UPGRADE");
                    PlaySound(Sound.POWER_UP);
                }
                else if (ScoreManager.Instance.MaxPartCount < 3)
                {
                    ScoreManager.Instance.MaxPartCount++;
                    tile.Contents = TileContents.EMPTY_TILE;
                    MessageList.Add("FOUND A CPU UPGRADE");
                    PlaySound(Sound.POWER_UP);
                }
                else
                {
                    MessageList.Add("NO ROOM FOR MORE CPU UPGRADES");
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
            Projectiles[i].Move();  // projectiles move two tiles a tick?
        }
        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            Projectiles[i].Move();  // projectiles move two tiles a tick?
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
                PlaySound(Sound.LASER_FIRE);
            }
            else if (TileMap.TileArray[facing_x, facing_y].Contents == TileContents.DUMB_BOT |
                     TileMap.TileArray[facing_x, facing_y].Contents == TileContents.SENTINEL_BOT_EW |
                     TileMap.TileArray[facing_x, facing_y].Contents == TileContents.SENTINEL_BOT_NS)
            {
                Enemy hit = GetEnemyAtTile(facing_x, facing_y);
                Debug.Log("Player hit " + hit.Name + " directly");
                MessageList.Add("YOUR LASER HITS!");
                hit.Hit(PlayerLaserDamage);
                MadeNoiseLastMove = true;
                moved = true;
                PlaySound(Sound.ENEMY_HIT);
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
                PlaySound(Sound.LASER_FIRE);
            }
        }
        return moved;
    }
    private bool playerActuate()
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
        switch (TileMap.TileArray[facing_x, facing_y].Contents)
        {
            case TileContents.BASIC_DOOR:
                MessageList.Add("YOU OPEN THE DOOR");
                TileMap.TileArray[facing_x, facing_y].Contents = TileContents.EMPTY_TILE;
                MadeNoiseLastMove = true;
                moved = true;
                PlaySound(Sound.DOOR_OPEN);
                break;
            case TileContents.SWITCHED_DOOR:
                MessageList.Add("THIS DOOR IS SWITCH CONTROLLED");
                MadeNoiseLastMove = true;
                moved = false;
                break;
            case TileContents.COMPUTER_DOOR:
                MessageList.Add("THIS DOOR IS COMPUTER...");
                MessageList.Add("...CONTROLLED");
                MadeNoiseLastMove = true;
                moved = false;
                break;
            case TileContents.SWITCH:
                MessageList.Add("YOU PULL THE SWITCH");
                MessageList.Add("DOORS OPEN SOMEWHERE");
                PlaySound(Sound.SWITCH_PULL);
                TileMap.TileArray[facing_x, facing_y].Contents = TileContents.EMPTY_TILE;
                for (int x = 0; x < TileMap.TileArray.GetUpperBound(0); x++)
                {
                    for (int y = 0; y < TileMap.TileArray.GetUpperBound(1); y++)
                    {
                        Tile tile = TileMap.TileArray[x, y];
                        if (tile.Contents == TileContents.SWITCHED_DOOR)
                        {
                            tile.Contents = TileContents.EMPTY_TILE;
                        }
                    }
                }
                MadeNoiseLastMove = true;
                moved = true;
                break;

            default:
                MessageList.Add("NOTHING TO ACTUATE");
                moved = false;
                break;
        }
        return moved;
    }

    private bool playerProbe()
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
        switch (TileMap.TileArray[facing_x, facing_y].Contents)
        {
            case TileContents.TERMINAL:
                MessageList.Add("YOU DISABLE THE TERMINAL");
                MessageList.Add("DOORS OPEN SOMEWHERE");
                TileMap.TileArray[facing_x, facing_y].Contents = TileContents.EMPTY_TILE;
                moved = true;
                for (int x = 0; x < TileMap.TileArray.GetUpperBound(0); x++)
                {
                    for (int y = 0; y < TileMap.TileArray.GetUpperBound(1); y++)
                    {
                        Tile tile = TileMap.TileArray[x, y];
                        if (tile.Contents == TileContents.COMPUTER_DOOR)
                        {
                            tile.Contents = TileContents.EMPTY_TILE;
                        }
                    }
                }
                MadeNoiseLastMove = true;
                PlaySound(Sound.TERMINAL_HACK);
                break;
            default:
                MessageList.Add("NOTHING TO PROBE");
                moved = false;
                break;
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
            if (ScoreManager.Instance.ActivePartCount <= ScoreManager.Instance.MaxPartCount)
            {
                setState(GameState.MOVEMENT);
                PlaySound(Sound.SELECT);
                Tick();
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) | Input.GetKeyDown(KeyCode.S))
        {
            PlaySound(Sound.SELECT);
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
            PlaySound(Sound.SELECT);
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
            PlaySound(Sound.SELECT);
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

            ScoreManager.Instance.ActivePartCount = 0;
            if (ScoreManager.Instance.sensorState > SensorState.BASIC)
                ScoreManager.Instance.ActivePartCount++;
            if (ScoreManager.Instance.chassisState > ChassisState.BASIC)
                ScoreManager.Instance.ActivePartCount++;
            if (ScoreManager.Instance.toolState > ToolState.NONE)
                ScoreManager.Instance.ActivePartCount++;
        }
    }

    private void initializePlayer()
    {
        direction = Direction.NORTH;
        setSensor(ScoreManager.Instance.sensorState);
        setChassis(ScoreManager.Instance.chassisState);
        setTool(ScoreManager.Instance.toolState);

        // store relevant states at the start of the level, to be restored on lose condition
        startHitPoints = ScoreManager.Instance.HitPoints;
        startSensorState = ScoreManager.Instance.sensorState;
        startChassisState = ScoreManager.Instance.chassisState;
        startToolState = ScoreManager.Instance.toolState;
        startActiveCount = ScoreManager.Instance.ActivePartCount;
        startMaxCount = ScoreManager.Instance.MaxPartCount;
        startSensorsAvailable = new bool[4];
        startChassisAvailable = new bool[4];
        startToolsAvailable = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            startSensorsAvailable[i] = ScoreManager.Instance.SensorsAvailable[i];
            startChassisAvailable[i] = ScoreManager.Instance.ChassisAvailable[i];
            startToolsAvailable[i] = ScoreManager.Instance.ToolsAvailable[i];
        }

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
        LevelText.text = string.Format("LEVEL: {0}", ScoreManager.Instance.CurrentLevel + 1);
        MessageList = new List<string>();
        messagePointer = 0;
        nextMessageTime = Time.time + messageScrollTime;
    }

    private void setChassis(ChassisState state)
    {
        ScoreManager.Instance.chassisState = state;
        chassis = new Chassis(state);
    }

    private void setSensor(SensorState state)
    {
        ScoreManager.Instance.sensorState = state;
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
                pointerPosition = (int)ScoreManager.Instance.sensorState;
                PointerText.gameObject.SetActive(true);
                break;
            case GameState.LEVEL_WON:
                ScoreManager.Instance.CurrentLevel++;
                if (ScoreManager.Instance.CurrentLevel == ScoreManager.Instance.MaxLevels)
                {
                    SceneManager.LoadScene("winscreen");
                }
                else
                {
                    SceneManager.LoadScene("levelclear");
                }
                break;
            case GameState.LOST:
                // reset the score manager state properties to the ones recorded at the start of the level
                ScoreManager.Instance.HitPoints = startHitPoints;
                ScoreManager.Instance.sensorState = startSensorState;
                ScoreManager.Instance.chassisState = startChassisState;
                ScoreManager.Instance.toolState = startToolState;
                ScoreManager.Instance.ActivePartCount = startActiveCount;
                ScoreManager.Instance.MaxPartCount = startMaxCount;
                ScoreManager.Instance.SensorsAvailable = startSensorsAvailable;
                ScoreManager.Instance.ChassisAvailable = startChassisAvailable;
                ScoreManager.Instance.ToolsAvailable = startToolsAvailable;
                SceneManager.LoadScene("losescreen");
                break;
            case GameState.NOTIFICATION:
                notificationEndTime = Time.time + 2f;
                break;
        }
        gameState = state;
    }

    private void setTool(ToolState state)
    {
        ScoreManager.Instance.toolState = state;
    }

    private void displayMessage()
    {
        if (Time.time > nextMessageTime)
        {
            messagePointer++;
            nextMessageTime = Time.time + messageScrollTime;
        }
        if (messagePointer >= MessageList.Count)
        {
            messagePointer = 0;
            nextMessageTime = Time.time + messageScrollTime;
        }

        switch (gameState)
        {

            case GameState.SELECTION:
                if (ScoreManager.Instance.ActivePartCount <= ScoreManager.Instance.MaxPartCount)
                {
                    MessageText.text = SelectString;
                }
                else
                {
                    MessageText.text = TooManyString;
                }
                break;
            default:
                if (MessageList.Count > 0)
                {
                    MessageText.text = MessageList[messagePointer];
                }
                else
                {
                    MessageText.text = LevelString;
                }
                break;
        }
    }
    #endregion
}
