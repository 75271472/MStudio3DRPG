using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterSubComponent
{
    public ICharacter Character { get; protected set; }
}
