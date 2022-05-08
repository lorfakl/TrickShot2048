using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;
    public UnityEvent<List<FloatReference>> Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.DeregisterListener(this);
    }

    public void OnEventRaised(List<FloatReference> relevantInfo)
    {
        Response.Invoke(relevantInfo);
    }
}
