using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    enum MoveState
    {
        ok, blocked
    }
    MoveState currMoveState;
    public GameObject playerBall;
    public GameplayManager gameplayManager;
    Vector2 startPoint;
    Vector2 endPoint;

    public Canvas cursorDrawZone;
    public Texture2D cursorTexture;
    public Texture2D pointer;
    public GameObject currentCursor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize()
    {
        currMoveState = MoveState.ok;
    }

    // Update is called once per frame
    void Update()
    {
        int movesRemaining = gameplayManager.GetMoves();
        if (playerBall.GetComponent<BallPhysics>().getVelocity() == 0)
        {
            currMoveState = MoveState.ok;
            gameplayManager.processIntersects();
        }

        if (Input.GetMouseButtonDown(0) && currMoveState == MoveState.ok && movesRemaining > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            startPoint = Input.mousePosition;
            currentCursor = new GameObject();
            RectTransform rectTransform =  currentCursor.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = startPoint;
            rectTransform.parent = cursorDrawZone.transform;
            GameObject cursorObj = new GameObject();
            RectTransform innerRectTransform = cursorObj.AddComponent<RectTransform>();
            innerRectTransform.parent = currentCursor.transform;
            innerRectTransform.anchoredPosition = Vector2.zero;
            cursorObj.AddComponent<RawImage>().texture = cursorTexture;
            GameObject lnObject = new GameObject();
            RectTransform innerLineRectTransform = lnObject.AddComponent<RectTransform>();
            innerLineRectTransform.parent = currentCursor.transform;
            innerLineRectTransform.anchoredPosition = Vector2.zero;
            lnObject.AddComponent<RawImage>().texture = pointer;
        }

        if (currentCursor)
        {
            RectTransform rectTransform = currentCursor.transform.GetChild(0).GetComponent<RectTransform>();
            Vector2 temp = -(new Vector2(Input.mousePosition.x, Input.mousePosition.y) - startPoint).normalized;
            float degrees = Mathf.Atan2(temp.x, -temp.y) * Mathf.Rad2Deg;
            rectTransform.eulerAngles = new Vector3(180, 180, degrees);
            rectTransform.position = Input.mousePosition;

        }

        if (Input.GetMouseButtonUp(0) && currMoveState == MoveState.ok && movesRemaining > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            endPoint = Input.mousePosition;
            // get magnitude of move
            float dragMagnitude = (Vector2.Distance(startPoint, endPoint) / Screen.width) / 0.35f;
            if (dragMagnitude > 1f)
            {
                dragMagnitude = 1f;
            }

            // get direction
            Vector2 dir = -(endPoint - startPoint).normalized;


            // apply movement and direction to ball
            playerBall.GetComponent<BallPhysics>().setBallMovement(dir, 10 * dragMagnitude);
            currMoveState = MoveState.blocked;
            gameplayManager.SetMoves(gameplayManager.GetMoves() - 1);
            Destroy(currentCursor);
        }
    }
}
