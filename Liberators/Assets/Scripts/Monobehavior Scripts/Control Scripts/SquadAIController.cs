using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadAIController : MonoBehaviour
{
    public operationsAI squadAI;
    bool inactive = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetComponent<SquadController>().isMoving())
        {
            inactive = true;
        }
        if (inactive)
        {
            aiCycle();
        }
    }

    void aiCycle()
    {
        switch (squadAI)
        {
            case operationsAI.ATTACK:
                attackAI();
                inactive = false;
                break;

            case operationsAI.WANDER:
                break;
        }
    }

    void attackAI()
    {
        OperationController operationController = gameObject.GetComponent<SquadController>().getOperationController();
        GameObject target = operationController.getSquadOfTeam(0);
        operationController.moveSquadToLocation(gameObject, operationController.gameObject.GetComponent<MapController>().gridTilePos(target.transform.position));
    }
}
