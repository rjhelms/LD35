﻿using UnityEngine;

public class GameController : MonoBehaviour
{
    public Material RenderMaterial;
    public Camera WorldCamera;

    public int TargetX = 320;
    public int TargetY = 200;

    // Use this for initialization
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {

    }
}
