using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertTriList
{
    public int[][] list;

    //    Indexable - use "vertTri[i]" to get the list of triangles for vertex i.
    public int[] this[int index]
    {
        get { return list[index]; }
    }

    public VertTriList(int[] tri, int numVerts)
    {
        Init(tri, numVerts);
    }

    public VertTriList(Mesh mesh)
    {
        Init(mesh.triangles, mesh.vertexCount);
    }

    //    You don't usually need to call this - it's just to assist the implementation
    //    of the constructors.
    public void Init(int[] tri, int numVerts)
    {
        //    First, go through the triangles, keeping a count of how many times
        //    each vert is used.
        int[] counts = new int[numVerts];

        for (int i = 0; i < tri.Length; i++)
        {
            counts[tri[i]]++;
        }

        //    Initialise an empty jagged array with the appropriate number of elements
        //    for each vert.
        list = new int[numVerts][];

        for (int i = 0; i < counts.Length; i++)
        {
            list[i] = new int[counts[i]];
        }

        //    Assign the appropriate triangle number each time a given vert
        //    is encountered in the triangles.
        for (int i = 0; i < tri.Length; i++)
        {
            int vert = tri[i];
            list[vert][--counts[vert]] = i / 3;
        }
    }
}
