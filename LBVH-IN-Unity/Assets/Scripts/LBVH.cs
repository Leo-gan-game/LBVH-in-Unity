using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AABB
{
    public uint morton;
    public Vector4 min;
    public Vector4 max;
    public Vector4 color;
    public Vector3 index;
}
public class LBVH 
{
    // Start is called before the first frame update
    private Vector3Int size;
    private List<AABB> array;

    public Vector3Int Size { get { return size; } }
    public int Count { get {return size.x * size.y * size.z; } }
    public List<AABB> Array { get => array; set => array = value; }

    /// <summary>
    /// this function creates an array of the structure of AABB, to create this prepare to produce a hierarchy
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void CreateAABB(int x,int y,int z)
    {
        size = new Vector3Int(x, y, z);
        array = new List<AABB>();
        //creating aabb array
        for(int i = 0; i < z; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < x; k++)
                {
                    Vector4 position = new Vector4(k, j, i);
                    Vector4 min = position-Vector4.one;
                    Vector4 max = position+ Vector4.one;
                    long m = GanerateMortonCode(k, j, i);
                    array.Add(new AABB()
                    {
                        morton = (uint)m,
                        min = min,
                        max = max,
                        color = new Vector4(k / (float)x, j / (float)y, i / (float)z, 0.5f),
                        index = new Vector3(k, j, i)
                    });
                }
            }
        }

        //do sorting
        array.Sort((AABB a1, AABB a2) => {
            return a1.morton >= a2.morton?1:-1;
        });
        foreach(var aabb in array)
        {
            Debug.Log("0x" + Convert.ToString(aabb.morton, 2)+"index:"+aabb.index);
        }
    }

    public long GanerateMortonCode(int x, int y, int z)
    {
        long m=0;
        for (int i = 0; i < sizeof(int); i++) 
        {
            m |= (x & 1U << i) << i*2 | (y & 1U << i) << (i*2 + 1)| (z & 1U << i)<<(i*2+2);
        }
        Debug.Log("0x"+Convert.ToString(m,16));
        return m;
    }
}
