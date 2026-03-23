/*
 * Unity: ExampleTrackScroll_2.cs
 * Edits By: DeathwatchGaming
 * License: MIT
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleTrackScroll_2 : MonoBehaviour
{
    [Header("Properties")]

    [Tooltip("The track mesh renderers.")]
    // Create a List of MeshRenderers ie: Track Renderers and start with 2 empty track elements ie: [0,1]
    [SerializeField] private List<MeshRenderer> _trackRenderers = new List<MeshRenderer>(new MeshRenderer[2]);

    [Tooltip("The scroll speed.")]
    // Scroll the main texture based on time
    [SerializeField] private float scrollSpeed = 0.5f;

    [Tooltip("The invert scroll 0 bool.")]
    // Invert the scroll 0 bool
    [SerializeField] private bool invertScroll0 = true;

    [Tooltip("The invert scroll 1 bool.")]
    // Invert the scroll 1 bool
    [SerializeField] private bool invertScroll1 = false;

    // The offset0 float
    private float offset0;

    // The offset1 float
    private float offset1;

    // Update is called once per frame
    private void Update()
    {
        if (invertScroll0)
        {
            offset0 = Time.time * -scrollSpeed;
        }

        else if (!invertScroll0)
        {
            offset0 = Time.time * scrollSpeed;
        }

        if (invertScroll1)
        {
            offset1 = Time.time * -scrollSpeed;
        }

        else if (!invertScroll1)
        {
            offset1 = Time.time * scrollSpeed;
        }

        _trackRenderers[0].material.mainTextureOffset = new Vector2(0f, offset0);
        _trackRenderers[1].material.mainTextureOffset = new Vector2(0f, offset1);
    }
}
