using UnityEngine;
using System.Collections;

public class TollGate : MonoBehaviour
{
    private CarAI currentCar;
    public int level = 1;
    float GetDelay()
    {
        if (level == 1) return 1.5f;
        if (level == 2) return 2f;
        if (level == 3) return 0.5f;

        return 2f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            Debug.Log("MOBIL MASUK GATE");

            CarAI car = other.GetComponentInParent<CarAI>();

            if (car != null)
            {
                currentCar = car;

                // 🔥 LANGSUNG STOP
                car.StartPaying();
            }
        }
    }

    // 🔥 DIPANGGIL DARI BUTTON
    public void PayAndRelease()
    {
        if (currentCar != null)
        {
            StartCoroutine(PayRoutine());
        }
    }

    IEnumerator PayRoutine()
    {
        Debug.Log("PROSES BAYAR...");

        float delay = GetDelay();

        yield return new WaitForSeconds(delay);

        // 💰 uang masuk
        GameManager.instance.AddMoney(1000);

        // 🚀 mobil jalan lagi
        currentCar.StopPaying();

        currentCar = null;
    }
}