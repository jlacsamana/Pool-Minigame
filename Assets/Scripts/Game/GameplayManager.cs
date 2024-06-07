using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using Unity.VisualScripting;

public class GameplayManager : MonoBehaviour
{
    enum GameState
    {
        started, ended
    }

    GameState currGameState;

    int score;
    int tries;
    int ballColorsAmt;
    int ballAmt;

    public GameObject playerBall;
    public TMP_Text scoreText;
    public TMP_Text triesLeftText;
    public TMP_Text endGameScoreText;
    public TMP_Text colorAmtText;
    public GameObject stage;
    public Controller controller;
    public GameStateHandler gameStateHandler;
    public AudioSource musicPlayer;

    // balls
    public GameObject ballPrefab;
    public List<GameObject> otherBalls;
    public List<GameObject> friendlyBalls;
    public List<Vector2> ballPositions;

    public List<Color> ballColors;
    public Color playerColor;

    // borders
    public GameObject topBorder;
    public GameObject bottomBorder;
    public GameObject leftBorder;
    public GameObject rightBorder;

    // bounds
    float ballRadius;
    float topLimit;
    float bottomLimit;
    float leftLimit;
    float rightLimit;

    // intersect queue
    HashSet<Ball> intersectQueue;


    // Start is called before the first frame update
    void Start()
    {
        ballColorsAmt = 3; //defaults
        ballAmt = 24;
        playerColor = new Color(0.98f, 0.85f, 0.01f); // this is the player color
        playerBall.GetComponent<Renderer>().material.color = playerColor;
        ballColors = new List<Color> { playerColor };
        // calculate bounds
        ballRadius = ballPrefab.GetComponent<Renderer>().bounds.extents.x;
        topLimit = topBorder.transform.position.z - (topBorder.GetComponent<Renderer>().bounds.extents.z) - ballRadius;
        bottomLimit = bottomBorder.transform.position.z + (bottomBorder.GetComponent<Renderer>().bounds.extents.z) + ballRadius;
        leftLimit = leftBorder.transform.position.x + (leftBorder.GetComponent<Renderer>().bounds.extents.x) + ballRadius;
        rightLimit = rightBorder.transform.position.x - (rightBorder.GetComponent<Renderer>().bounds.extents.x) - ballRadius;
        colorAmtText.text = $"Increment Color Count: {ballColorsAmt}";
        Initialize();
    }

    public void Initialize()
    {
        // reset score and moves
        SetScore(0);
        SetMoves(10);

        // reset colors
        ballColors.Clear();
        ballColors.Add(playerColor);

        for (int i = 0; i < ballColorsAmt; i++)
        {
            Color newColor = new Color();
            while (true)
            {
                newColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                if (ballColors.All(col => areColorsSufficientlyDifferent(col, newColor, 0.20f)))
                {
                    break;
                }

            }
            ballColors.Add(newColor);
        }

        //clear field of existing balls
        otherBalls.ForEach((ball) =>
        {
            Destroy(ball);
        });
        otherBalls.Clear();
        ballPositions = new List<Vector2>();

        // add player ball and save position
        playerBall.transform.position = new Vector3(0, 1, stage.transform.position.z);
        ballPositions.Add(new Vector2(playerBall.transform.position.x, playerBall.transform.position.z));


        // add new balls
        for (int i = 0; i < ballAmt; i++)
        {
            Vector2 newBallPos = Vector2.zero;
            while (true)
            {
                newBallPos = new Vector2 (UnityEngine.Random.Range(leftLimit, rightLimit), UnityEngine.Random.Range(bottomLimit, topLimit));
                if (!ballPositions.Any(occupiedPosition => areBallsIntersecting(newBallPos, occupiedPosition, ballPrefab.GetComponent<Renderer>().bounds.size.x)))
                {
                    break;
                }
                

            }

            GameObject newBall = Instantiate(ballPrefab, new Vector3(newBallPos.x, 1, newBallPos.y), Quaternion.identity);
            otherBalls.Add(newBall);
            ballPositions.Add(newBallPos);
        }

        // set colors
        // set a third to player's color
        friendlyBalls.Clear();
        friendlyBalls = otherBalls.Take(otherBalls.Count / 3).ToList();
        friendlyBalls.ForEach((ball) =>
        {
            ball.GetComponent<Renderer>().material.color = playerColor;
            ball.GetComponent<Ball>().Initialize(10);
        });

        // other balls set to random non-player colors
        otherBalls.Where((ball) => !friendlyBalls.Contains(ball)).ToList().ForEach((ball) =>
        {
            ball.GetComponent<Renderer>().material.color = ballColors[UnityEngine.Random.Range(1, ballColors.Count - 1)];
            ball.GetComponent<Ball>().Initialize(-10);
        });

        // reset intersection queue
        intersectQueue = new HashSet<Ball>();

        // init controlller and player ball
        playerBall.GetComponent<BallPhysics>().Initialize();
        controller.Initialize();

        gameStateHandler.StartNewGame();

        // set game state to started
        currGameState = GameState.started;
    }

    // Update is called once per frame
    void Update()
    {
        if ((tries == 0 && playerBall.GetComponent<BallPhysics>().getVelocity() == 0) || friendlyBalls.All(ball => ball.GetComponent<Ball>().isHit()))
        {
            FinishGame();
        }
    }

    private void FixedUpdate()
    {
        checkForIntersects();
        EnforceBoundaries();
    }

    private void checkForIntersects()
    {
        otherBalls.ForEach((ball) =>
        {
            if (areBallsIntersecting(playerBall, ball, 1))
            {
                Ball b = ball.GetComponent<Ball>();
                if (!b.isHit())
                {
                    intersectQueue.Add(b);
                }
            }
        });
    }

    public void processIntersects()
    {
        foreach (Ball b in intersectQueue) {
            b.BallHit();
            SetScore(score + b.getPoints());
        }
        intersectQueue.Clear();
    }

    public void EnforceBoundaries()
    {
        if (!(leftLimit < playerBall.transform.position.x && playerBall.transform.position.x < rightLimit && bottomLimit < playerBall.transform.position.z && playerBall.transform.position.z < topLimit))
        {
            BallPhysics ballPhysics = playerBall.GetComponent<BallPhysics>();
            Vector2 dir = -ballPhysics.getDirection();
            playerBall.transform.Translate(dir.x * (Time.fixedDeltaTime * 15), 0, dir.y * (Time.fixedDeltaTime * 15));
            playerBall.GetComponent<BallPhysics>().setBallMovement(Vector2.zero, 0);
        }
    }

    public bool areBallsIntersecting(Vector2 pos1, Vector2 pos2, float ballSize)
    {
        return Vector2.Distance(pos1, pos2) <= ballSize;
    }

    public bool areBallsIntersecting(GameObject pos1, GameObject pos2, float ballSize)
    {
        return Vector3.Distance(pos1.transform.position, pos2.transform.position) <= ballSize;
    }

    public bool areColorsSufficientlyDifferent(Color color1, Color color2, float tolerance)
    {
        return Math.Abs(color2.r - color1.r) > tolerance || Math.Abs(color2.g - color1.g) > tolerance || Math.Abs(color2.b - color1.b) > tolerance;
    }

    public void FinishGame()
    {
        if (currGameState != GameState.ended) {
            currGameState = GameState.ended;
            // pause game
            Time.timeScale = 0;
            // display scorebaord
            gameStateHandler.FinishGame();
            endGameScoreText.text = $"You earned {score} points in {10 - tries} turns!";
        }
    }

    public void SetMoves(int newTries)
    {
        tries = newTries;
        triesLeftText.text = $"Moves Remaining: {tries}";
    }

    public int GetMoves()
    {
        return tries;
    }

    public void SetScore(int newScore)
    {
        score = newScore;
        scoreText.text = $"Score: {score}";
    }

    public int GetScore()
    {
        return score;
    }

    public void ChangeNumberOfColors()
    {
        ballColorsAmt++;
        if (ballColorsAmt > 5)
        {
            ballColorsAmt = 2;
        }

        colorAmtText.text = $"Increment Color Count: {ballColorsAmt}";
        // reload game
        Initialize();
    }

    public void ToggleMusic()
    {
        musicPlayer.mute = !musicPlayer.mute;
    }

}
