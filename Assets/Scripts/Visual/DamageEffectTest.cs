using UnityEngine;
using System.Collections;

public class DamageEffectTest : MonoBehaviour {

    public static DamageEffectTest Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            DamageEffect.CreateDamageEffect(this.gameObject, Random.Range(1, 100));
    }
}
