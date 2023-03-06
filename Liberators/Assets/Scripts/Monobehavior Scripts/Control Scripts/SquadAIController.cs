using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadAIController : MonoBehaviour
{
    public OperationsAI squadAI;
    bool inactive = true;
    bool disabled = true;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(startTimer());
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetComponent<SquadController>().isMoving())
        {
            inactive = true;
        }
        if (inactive && !disabled)
        {
            aiCycle();
        }
    }

    void aiCycle()
    {
        switch (squadAI)
        {
            case OperationsAI.ATTACK:
                attackAI();
                inactive = false;
                break;

            case OperationsAI.WANDER:
                break;
        }
    }

    void attackAI()
    {
        OperationController operationController = gameObject.GetComponent<SquadController>().getOperationController();
        GameObject target = operationController.getSquadOfTeam(0);
        operationController.moveSquadToLocation(gameObject, operationController.gameObject.GetComponent<MapController>().gridTilePos(target.transform.position));
    }

    IEnumerator startTimer()
    {
        disabled = true;
        yield return new WaitForSeconds(3f);
        disabled = false;
    }

    public void forceStart()
    {
        disabled = false;
        StopCoroutine("startTimer");
    }
}
