using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class CoolDown : MonoBehaviour
{
    public float cooldownTime = 2f;

    public void ActivateCoolDown(Button o)
    {
        StartCoroutine(CoolDownRoutine(o));
    }

    IEnumerator CoolDownRoutine(Button b)
    {
        b.interactable = false;

        yield return new WaitForSeconds(cooldownTime);

        b.interactable = true;
    }
}
