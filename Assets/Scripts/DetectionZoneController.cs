using UnityEngine;

public class DetectionZoneController : MonoBehaviour
{
  public HumanController humanController;

  void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.name.Equals("Player"))
    {
      humanController.HandleZombieDetection(other.transform);
    }
  }
}