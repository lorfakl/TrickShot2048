using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSpecificGlobals;

public class ContactTypes : ScriptableObject
{
    

    string contactType;
    ContactType contactTypeEnum;
    BlockController block;
    SimpleBlock cubeBlock;
    Collision collision;

    public ContactType CType
    {
        get { return contactTypeEnum; }
    }

    public BlockController Block
    {
        get { return block; }
    }

    public Collision Collision
    {
        get { return collision; }
    }

    public void GenerateContactData(BlockController thisObject, Collision col)
    {
        block = thisObject;
        this.collision = col;
    }

    public void GenerateContactData(SimpleBlock thisObject, Collision col)
    {
        cubeBlock = thisObject;
        this.collision = col;
    }

    public void UpdateContactType(ContactType ct)
    {
        contactType = ct.ToString();
        contactTypeEnum = ct;
    }
}
