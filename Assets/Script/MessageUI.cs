using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.Message += UpdateMessage;
    }

    public void UpdateMessage(Player player, GameState state)
    {
        TMP_Text myText = GetComponent<TMP_Text>();

        switch (state)
        {
            case GameState.Click:
                myText.text = " Click a Piece";
                break;
            case GameState.Move:
                myText.text = "Move a Piece";
                break;
            case GameState.Finished:
                myText.text = player == Player.Red ? "Red wins" : "Blue wins";
                break;
        }

        myText.color = player == Player.Red ? Color.red : Color.blue;
    }
}
