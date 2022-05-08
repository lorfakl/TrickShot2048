using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameEvent : ScriptableObject
{
    [SerializeField]
    List<FloatReference> relevantVariables;

    private List<GameEventListener> listeners = new List<GameEventListener>();


    public void Raise()
    {
        for(int i = listeners.Count - 1; i >=0; i--)
        {
            listeners[i].OnEventRaised(relevantVariables);
        }
    }


    public void Raise(List<FloatReference> floatReferences)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(floatReferences);
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        listeners.Add(listener);
    }

    public void DeregisterListener(GameEventListener listener)
    {
        listeners.Remove(listener);
    }
}
