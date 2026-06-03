using UnityEngine;

public class GateVisualManager : MonoBehaviour
{
    public GateLabelController gate1;
    public GateLabelController gate2;
    public GateLabelController gate3;
    public GateLabelController gate4;

    public TollGate tollGate1;
    public TollGate tollGate2;
    public TollGate tollGate3;
    public TollGate tollGate4;

    void Start()
    {
        RefreshGateVisual();
    }

    public void RefreshGateVisual()
    {
        gate1.SetUnlocked(tollGate1.isUnlocked);
        gate2.SetUnlocked(tollGate2.isUnlocked);
        gate3.SetUnlocked(tollGate3.isUnlocked);
        gate4.SetUnlocked(tollGate4.isUnlocked);
    }
}