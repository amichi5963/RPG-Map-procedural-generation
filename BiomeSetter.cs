using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeSetter : MonoBehaviour
{
    [SerializeField] public List<SerializableKeyPair<SerializableKeyPair<float, float>, RuleTile>> Setting;
}
