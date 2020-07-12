using UnityEngine;

public class GameController : MonoBehaviour
{

  public AudioManager audioManager;

  void Start()
  {
    // audioManager.Play("Title Music");
    audioManager.Play("Game Music");
  }

  void StartGame()
  {
    audioManager.Play("Game Music");
  }

  public void Win()
  {
    Debug.Log("You live to see another day! Well, kinda ...!");
    QuitGame();
  }

  public void Lose()
  {
    Debug.Log("You died again! This time for real.");
    QuitGame();
  }

  private void QuitGame()
  {
    // save any game data here
#if UNITY_EDITOR
         // Application.Quit() does not work in the editor so
         // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
         UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }
}