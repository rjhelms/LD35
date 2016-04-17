﻿using UnityEngine;
using UnityEngine.UI;
using System;

public enum SensorState
{
    BASIC,
    OMNI,
    INFRARED,
    LONGRANGE
}

public class GameController : MonoBehaviour
{
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

    public Text[] SensorText;
    public Text[] ChassisText;
    public Text[] ToolText;

    public GameObject PrefabTileSprite;

    public SpriteRenderer[,] VisibleSprites;
    public Transform VisibleSpritesParent;

    public bool EGAMode = true;
    private SpriteDefinitions spriteDefinitions;
    public TileMap TileMap;
    private Sensor sensor;
    private Direction direction;
    private SensorState sensorState;
    public SpriteRenderer HeadSprite;
    public SpriteRenderer BodySprite;
    private Sprite[] headSpriteArray;

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            sensor = new DirectionalSensor(TileMap);
            sensorState = SensorState.BASIC;
            headSpriteArray = spriteDefinitions.EGADirectionalHead;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            sensor = new OmniSensor(TileMap);
            sensorState = SensorState.OMNI;
            headSpriteArray = spriteDefinitions.EGAOmniHead;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            sensor = new IRSensor(TileMap);
            sensorState = SensorState.INFRARED;
            headSpriteArray = spriteDefinitions.EGAIRHead;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            sensor = new LongRangeSensor(TileMap);
            sensorState = SensorState.LONGRANGE;
            headSpriteArray = spriteDefinitions.EGALongRangeHead;
        }

        foreach (Text text in SensorText)
        {
            text.color = InactiveColour;
        }

        SensorText[(int)sensorState].color = ActiveColour;
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

        HeadSprite.sprite = headSpriteArray[(int)direction];

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

        int new_x_pos = PlayerXPos;
        int new_y_pos = PlayerYPos;
        int forward_move = 0;
        int lateral_move = 0;
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
    }

    #region Private Methods

    private void InitializePlayer()
    {
        direction = Direction.NORTH;
        sensorState = SensorState.BASIC;
        sensor = new DirectionalSensor(TileMap);
        headSpriteArray = spriteDefinitions.EGADirectionalHead;
        chassisState = ChassisState.FAST;
        chassis = new Chassis(chassisState);
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
    }

    #endregion
}
