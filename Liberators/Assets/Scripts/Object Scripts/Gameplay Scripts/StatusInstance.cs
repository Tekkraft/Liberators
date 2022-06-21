using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusInstance
{
    Status status;
    int durationLeft;
    bool permanent;
    GameObject statusSource;

    public StatusInstance(Status status, GameObject statusSource)
    {
        this.status = status;
        this.statusSource = statusSource;
        if (durationLeft == -1)
        {
            this.durationLeft = 1;
            permanent = true;
        } else
        {
            this.durationLeft = status.getDuration();
            permanent = false;
        }
    }

    public bool modifyDuration(int modifyBy)
    {
        if (!permanent)
        {
            durationLeft += modifyBy;
            return durationLeft <= 0;
        }
        return false;
    }

    public bool update()
    {
        if (!permanent)
        {
            durationLeft--;
            return durationLeft <= 0;
        }
        return false;
    }

    public Status getStatus()
    {
        return status;
    }

    public GameObject getStatusSource()
    {
        return statusSource;
    }
}
