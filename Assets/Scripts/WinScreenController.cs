using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class WinScreenController : MonoBehaviour
{
    public GameObject winScreen;        
    public GameObject loseScreen;       
    public TextMeshProUGUI winText;     // Text for the win screen
    public TextMeshProUGUI loseText;    // Text for the lose screen
    public float waveSpeed = 2f;        // Speed of wavy animation 
    public float waveHeight = 5f;       // Height of wave

    private TMP_TextInfo textInfo;
    private bool isShowingWinScreen = false;
    private bool isShowingLoseScreen = false;

    private void Start()
    {
        // Both screens are hidden at the start
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }

    private void Update()
    {
        if (isShowingWinScreen || isShowingLoseScreen)
        {
            AnimateWavyText(isShowingWinScreen ? winText : loseText);
        }
    }

    public void ShowWinScreen()
    {
        // Enable win screen
        winScreen.SetActive(true);
        isShowingWinScreen = true;

        // Prepare the text for animation
        winText.ForceMeshUpdate();
        textInfo = winText.textInfo;

        // Restart the game after 3s.
        StartCoroutine(RestartGameAfterDelay(3f));
    }

    public void ShowLoseScreen()
    {
        // Enable lose screen
        loseScreen.SetActive(true);
        isShowingLoseScreen = true;

        // Prepare the text for animation
        loseText.ForceMeshUpdate();
        textInfo = loseText.textInfo;

        // Restart the game after 3s.
        StartCoroutine(RestartGameAfterDelay(3f));
    }

    public void ShowCustomLoseMessage(string message)
    {
        // Set the custom lose message
        loseText.text = message;

        // Enable lose screen
        loseScreen.SetActive(true);
        isShowingLoseScreen = true;

        // Prepare the text for animation
        loseText.ForceMeshUpdate();
        textInfo = loseText.textInfo;

        // Restart the game after 3s.
        StartCoroutine(RestartGameAfterDelay(3f));
    }


    private void AnimateWavyText(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.ForceMeshUpdate();
        textInfo = text.textInfo;

        float time = Time.time;

        // Loop through each char to animate 
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            float waveOffset = Mathf.Sin(time * waveSpeed + i * 0.5f) * waveHeight;

            // Apply wave offset to all vertices of the character
            vertices[vertexIndex + 0].y += waveOffset; // UL
            vertices[vertexIndex + 1].y += waveOffset; // UR
            vertices[vertexIndex + 2].y += waveOffset; // DL
            vertices[vertexIndex + 3].y += waveOffset; // DR 
        }

        // Update the mesh with modified vertices
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }


    private IEnumerator RestartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Restart the game 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
