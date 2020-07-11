using UnityEngine;
using UnityEngine.UI;

public class ColdBloodManager : MonoBehaviour
{

  public Text coldBloodLabel;
  public float totalColdBlood = 100.0f;
  public float coldBloodLossRate = 0.1f;

  private float coldBlood;
  private GameController gameController;

  void Start()
  {
    coldBlood = totalColdBlood;
    gameController = GameObject.Find("GameController").GetComponent<GameController>();
  }

  void Update()
  {
    coldBloodLabel.text = string.Format("{0:0} cold blood", coldBlood);

    if (coldBlood == 0.0f)
    {
      gameController.Lose();
    }

    DepleteColdBlood(coldBloodLossRate * Time.deltaTime);
  }

  public void AddColdBlood(float coldBlood)
  {
    this.coldBlood = Mathf.Min(totalColdBlood, this.coldBlood + coldBlood);
  }

  public void RemoveColdBlood(float coldBlood)
  {
    this.coldBlood = Mathf.Max(0, this.coldBlood - coldBlood);
  }

  public void DepleteColdBlood(float percentage)
  {
    RemoveColdBlood(totalColdBlood * percentage);
  }

  public void GainColdBlood(float percentage)
  {
    AddColdBlood(totalColdBlood * percentage);
  }
}