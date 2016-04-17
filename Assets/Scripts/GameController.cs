using UnityEngine;
using UnityEngine.UI;
using System;

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
    NOTIFICATION
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

    public GameObject PrefabTileSprite;

    public SpriteRenderer[,] VisibleSprites;
    public Transform VisibleSpritesParent;

    public bool EGAMode = true;

    public TileMap TileMap;

    public SpriteRenderer HeadSprite;
    public SpriteRenderer BodySprite;
    public SpriteRenderer ToolSprite;
    #endregion

    #region Private Attributes
    private int playerTicks;

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

    #endregion

    #region MonoBehaviour Methods
    // Use this for initialization
    void Start()
    {
        spriteDefinitions = FindObjectOfType<SpriteDefinitions>();
        InitializePlayer();
        InitializeUI();
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameState)
        {
            case GameState.MOVEMENT:
                DoMovement();
                UpdateMap();
                break;
            case GameState.SELECTION:
                DoSelection();
                break;
        }

        UpdateUI();

    }
    #endregion

    #region Private Methods
    private void UpdateMap()
    {
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

    private void UpdateUI()
    {
        foreach (Text text in SensorText)
        {
            text.color = InactiveColour;
        }

        foreach (Text text in ChassisText)
        {
            text.color = InactiveColour;
        }

        foreach (Text text in ToolText)
        {
            text.color = InactiveColour;
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

        CountText.text = String.Format("ACTIVE  {0}\r\nMAX     {1}", activeCount, maxCount);

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
    }

    private void DoMovement()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetState(GameState.SELECTION);
            return;
        }

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
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            this.direction++;
            if (this.direction > Direction.WEST)
            {
                this.direction = Direction.NORTH;
            }
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
            PlayerXPos = new_x_pos;
            PlayerYPos = new_y_pos;
        }
        else
        {
            TileMap.TileArray[new_x_pos, new_y_pos].SetVisible();
        }
    }

    private void DoSelection()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (activeCount <= maxCount)
                SetState(GameState.MOVEMENT);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) | Input.GetKeyDown(KeyCode.S))
        {
            pointerPosition += 1;
            if (pointerPosition == 12)
            {
                pointerPosition = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) | Input.GetKeyDown(KeyCode.W))
        {
            pointerPosition -= 1;
            if (pointerPosition == -1)
            {
                pointerPosition = 11;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            switch (pointerPosition)
            {
                case 0:
                    SetSensor(SensorState.BASIC);
                    break;
                case 1:
                    SetSensor(SensorState.OMNI);
                    break;
                case 2:
                    SetSensor(SensorState.INFRARED);
                    break;
                case 3:
                    SetSensor(SensorState.LONGRANGE);
                    break;
                case 4:
                    SetChassis(ChassisState.BASIC);
                    break;
                case 5:
                    SetChassis(ChassisState.SILENT);
                    break;
                case 6:
                    SetChassis(ChassisState.FAST);
                    break;
                case 7:
                    SetChassis(ChassisState.OFFROAD);
                    break;
                case 8:
                    SetTool(ToolState.NONE);
                    break;
                case 9:
                    SetTool(ToolState.LASER);
                    break;
                case 10:
                    SetTool(ToolState.ACTUATOR);
                    break;
                case 11:
                    SetTool(ToolState.PROBE);
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

    private void InitializePlayer()
    {
        direction = Direction.NORTH;
        SetSensor(SensorState.BASIC);
        SetChassis(ChassisState.BASIC);
        SetTool(ToolState.NONE);
        gameState = GameState.MOVEMENT;
    }

    private void InitializeUI()
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
    }

    private void SetChassis(ChassisState state)
    {
        chassisState = state;
        chassis = new Chassis(state);
    }

    private void SetSensor(SensorState state)
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

    private void SetState(GameState state)
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
        }
        gameState = state;
    }

    private void SetTool(ToolState state)
    {
        toolState = state;
    }
    #endregion
}
