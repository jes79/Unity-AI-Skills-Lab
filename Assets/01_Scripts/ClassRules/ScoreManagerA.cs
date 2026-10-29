using UnityEngine;
using UnityEngine.InputSystem;

// A · rules skill not used (reproduced): a typical "score manager" from old tutorials
public class ScoreManagerA : MonoBehaviour
{
    public int Score;
    public ScorePop Pop;

    void Update()
    {
        // find the coin every frame
        Pop = FindObjectOfType<ScorePop>();

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Score += 10;
            Debug.Log("Score: " + Score);
            Pop.Play();

            if (Score >= 100)
            {
                Debug.Log("Clear!");
            }
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 40;
        GUI.Label(new Rect(160, 300, 600, 60), "Score: " + Score);
        if (Score >= 100)
        {
            GUI.Label(new Rect(160, 360, 600, 60), "Clear!");
        }
    }
}
