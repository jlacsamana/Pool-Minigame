using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateHandler : MonoBehaviour
{
    public Canvas Menu;
    public Canvas Endscreen;
    // Start is called before the first frame update
    void Start()
    {
        Resume();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartNewGame()
    {
        Time.timeScale = 1;
        Menu.enabled = false;
        Endscreen.enabled = false;
    }

    public void Pause ()
    {
        Time.timeScale = 0;
        Menu.enabled = true;   
    }

    public void Resume ()
    {
        Time.timeScale = 1;
        Menu.enabled = false;
    }

    public void FinishGame()
    {
        Time.timeScale = 0;
        Menu.enabled = false;
        Endscreen.enabled = true;
    }

    public void PostGame() {
        Time.timeScale = 0;
        Menu.enabled = true;
        Endscreen.enabled = false;
    }
}
