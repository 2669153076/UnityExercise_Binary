using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TowerInfoContainer data = BinaryDataMgr.Instance.GetTable<TowerInfoContainer>();

        print(data.dataDic[5].name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
