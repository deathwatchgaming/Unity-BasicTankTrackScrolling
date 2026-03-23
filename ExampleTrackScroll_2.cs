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

    // The offset float
    private float offset;

    // Update is called once per frame
    private void Update()
    {
        offset = Time.time * scrollSpeed;

        _trackRenderers[0].material.mainTextureOffset = new Vector2(0f, offset);
        _trackRenderers[1].material.mainTextureOffset = new Vector2(0f, offset);
    }
}
