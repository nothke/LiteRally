using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager e;
    void Awake() { e = this; }

    public ControlScheme[] controlSchemes;

    public string[] GetControlSchemeNames()
    {
        string[] names = new string[controlSchemes.Length];

        for (int i = 0; i < controlSchemes.Length; i++)
            names[i] = controlSchemes[i].name;

        return names;
    }
}
