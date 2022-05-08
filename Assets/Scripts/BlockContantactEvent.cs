using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

[CreateAssetMenu(fileName = "NewBlockContactEvent", menuName = "BlockEvents")]
public class BlockContantactEvent : ScriptableObject
{
    private UnityEvent<ContactTypes> contactEvent = new UnityEvent<ContactTypes>();
    
    public UnityEvent<ContactTypes> ContactEvent
    {
        get { return contactEvent; }
    }

    public void NotifyListeners(ContactTypes ct)
    {
        //HelperFunctions.Log("Listeners notified");
        contactEvent.Invoke(ct);
    }
}
