using UnityEngine;

public class HumanManager : MonoBehaviour
{
  private GameController gameController;

  void Start()
  {
    gameController = GameObject.Find("GameController").GetComponent<GameController>();
  }

  void Update()
  {
    if (transform.childCount == 0)
    {
      gameController.Win();
    }
  }
}