using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseMaintenanceProcedure : MonoBehaviour
{
    public InstructionManager InstructionManager;

    public void Close()
    {
        InstructionManager.EndProcedure();
    }
}
