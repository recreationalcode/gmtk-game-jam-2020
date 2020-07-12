using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColdBloodManager : MonoBehaviour
{

  public TextMeshProUGUI coldBloodLabel;
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
    coldBloodLabel.SetText(string.Format("{0:0} cold blood", coldBlood));

    if (coldBlood == 0.0f)
    {
      gameController.Lose();
    }

    if (!gameController.isGameStarted) return;
    DepleteColdBlood(coldBloodLossRate * Time.deltaTime);
  }

  public void ResetColdBlood()
  {
    coldBlood = totalColdBlood;
  }

  public float GetColdBloodPercentage()
  {
    return coldBlood / totalColdBlood;
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