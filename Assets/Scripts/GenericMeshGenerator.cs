/*
 * File:        GenericMeshGenerator.cs
 * Author:      Igor Spiridonov
 * Description: Class GenericMeshGenerator is a parametric generating routine of a 3D mesh
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericMeshGenerator : MonoBehaviour
{

    public Mesh Form
    {
        get; private set;
    }

    [SerializeField]
    [Range(4, 30000)]
    protected int verticesCount = 100;
    [SerializeField]
    [Range(0, 20)]
    protected int innerHolesCount = 0;
    protected bool hasEntryHole = true;
    protected bool hasExitHole = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual Mesh GenerateMesh()
    {
        Form = new Mesh();
        return Form;
    }
}
