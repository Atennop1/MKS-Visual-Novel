// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections;
using UnityEngine;

namespace Naninovel.UI
{
    public class LoadingIcon : MonoBehaviour
    {
        [SerializeField] private float animationSpeed = 100f;
        [SerializeField] private float animationDelay = 2f;

        private WaitForSeconds waitForDelay;

        private IEnumerator Start ()
        {
            waitForDelay = new WaitForSeconds(animationDelay);

            while (Application.isPlaying)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180);

                while (transform.rotation.eulerAngles.z > 0)
                {
                    var newZ = Mathf.Clamp(transform.rotation.eulerAngles.z - animationSpeed * Time.deltaTime, 0, 180);
                    transform.rotation = Quaternion.Euler(0, 0, newZ);
                    yield return null;
                }

                yield return waitForDelay;
            }
        }
    }
}
