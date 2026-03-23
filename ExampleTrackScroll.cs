/*
 * Unity: ExampleTrackScroll.cs
 * Edits By: DeathwatchGaming
 * License: MIT
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleTrackScroll : MonoBehaviour
{
    // Create a List of MeshRenderers ie: trackRenderers and start with 2 empty track elements ie: [0,1]
    [SerializeField] private List<MeshRenderer> _trackRenderers = new List<MeshRenderer>(new MeshRenderer[2]);

    // Scroll the main texture based on time
    [SerializeField] private float scrollSpeed = 0.5f;

    private float offset;

    private void Update()
    {
        offset = Time.time * scrollSpeed;

        foreach (var _meshRenderer in _trackRenderers)
        {
            _meshRenderer.materials[0].mainTextureOffset = new Vector2(0f, offset);
        }
    }
}
