/*
 * Unity: ExampleTrackScroll1.cs
 * Edits By: DeathwatchGaming
 * License: MIT
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleTrackScroll1 : MonoBehaviour
{
    [Header("Properties")]

    [Tooltip("The track mesh renderers.")]
    // Create a List of MeshRenderers ie: Track Renderers and start with 2 empty track elements ie: [0,1]
    [SerializeField] private List<MeshRenderer> _trackRenderers = new List<MeshRenderer>(new MeshRenderer[2]);

    [Tooltip("Set if using shader graph.")]
    // Bool for if using shader graph
    [SerializeField] private bool usingShaderGraph;

    [Tooltip("Set the main texture for shader graph.")]
    // String for setting the main texture for shader graph
    [SerializeField] private string setMainTexture = "_Albedo_Map";

    [Tooltip("The scroll speed.")]
    // Scroll the main texture based on time
    [SerializeField] private float scrollSpeed = 0.5f;

    [Tooltip("The invert scroll bool.")]
    // Invert the scroll bool
    [SerializeField] private bool invertScroll;

    // The offset float
    private float offset;

    // Update is called once per frame
    private void Update()
    {
        if (invertScroll)
        {
            offset = Time.time * -scrollSpeed;
        }

        else if (!invertScroll)
        {
            offset = Time.time * scrollSpeed;
        }

        foreach (var _meshRenderer in _trackRenderers)
        {
            if (usingShaderGraph)
            {
                _meshRenderer.materials[0].SetTextureOffset(setMainTexture, new Vector2(0f, offset));
            }

            else if (!usingShaderGraph)
            {
                _meshRenderer.materials[0].mainTextureOffset = new Vector2(0f, offset);   
            }     
        }
    }
}
