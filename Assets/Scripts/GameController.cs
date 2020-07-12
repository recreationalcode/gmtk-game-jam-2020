using UnityEngine;

public class GameController : MonoBehaviour
{

  public AudioManager audioManager;
  public HumanManager humanManager;
  public PlayerController playerController;
  public GameObject playButton;

  public bool isGameStarted = false;

  void Start()
  {
    audioManager.Play("Title Music");
  }

  public void StartGame()
  {
    playButton.SetActive(false);

    isGameStarted = true;

    playerController.StartPlaying();

    audioManager.Stop("Title Music");
    audioManager.Play("Game Music");

    humanManager.StartSpawning();

  }

  public void Win()
  {
    Debug.Log("You live to see another day! Well, kinda ...!");
    StopGame();
  }

  public void Lose()
  {
    Debug.Log("You died again! This time for real.");
    StopGame();
  }

  void StopGame()
  {
    isGameStarted = false;

    playerController.StopPlaying();

    audioManager.Stop("Game Music");
    audioManager.Play("Title Music");

    humanManager.StopSpawning();

    playButton.SetActive(true);
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