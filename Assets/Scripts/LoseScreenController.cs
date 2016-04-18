using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoseScreenController : MonoBehaviour {

    public Material RenderMaterial;
    public Camera WorldCamera;

    public int TargetX = 320;
    public int TargetY = 200;
    public Text pointerText;
    public bool Retry = true;
    public Text retryText;
    public Text quitText;
    // Use this for initialization
    void Start () {
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
	void Update () {
        if (Input.GetKeyDown(KeyCode.A) | Input.GetKeyDown(KeyCode.D) | Input.GetKeyDown(KeyCode.Z) |
            Input.GetKeyDown(KeyCode.Keypad4) | Input.GetKeyDown(KeyCode.Keypad6) | Input.GetKeyDown(KeyCode.LeftArrow) | Input.GetKeyDown(KeyCode.RightArrow))
            Retry = !Retry;

        if (Retry)
        {
            pointerText.transform.position = new Vector2(retryText.transform.position.x - 16, pointerText.transform.position.y);
        } else
        {
            pointerText.transform.position = new Vector2(quitText.transform.position.x - 16, pointerText.transform.position.y);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) | Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (Retry)
                SceneManager.LoadScene("level");
            else
                Application.Quit();
        }
    }
}
