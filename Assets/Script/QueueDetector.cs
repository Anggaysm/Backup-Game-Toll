using UnityEngine;
using System.Collections.Generic;

public class QueueDetector : MonoBehaviour
{
    public List<CarAI> queuedCars = new List<CarAI>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            CarAI car = other.GetComponentInParent<CarAI>();

            if (car != null && !queuedCars.Contains(car))
            {
                queuedCars.Add(car);

                Debug.Log($"🚗 MASUK ANTRIAN: {car.carID}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            CarAI car = other.GetComponentInParent<CarAI>();

            if (car != null && queuedCars.Contains(car))
            {
                queuedCars.Remove(car);

                Debug.Log($"🚙 KELUAR ANTRIAN: {car.carID}");
            }
        }
    }
}