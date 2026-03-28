/*
 * Unity: ExampleTrackScroll3.cs
 * Edits By: DeathwatchGaming
 * License: MIT
 */

using System;
using UnityEngine;
using System.Collections.Generic;

public class ExampleTrackScroll3 : MonoBehaviour
{
    [Serializable]
    public struct Track
    {
        [Tooltip("The mesh renderer.")]
        public MeshRenderer meshRenderer;

        [Tooltip("The bool to mark as left track.")]
        public bool left;

        [Tooltip("The bool to mark as right track.")]
        public bool right;
    }

    [Header("Properties")]

    [Tooltip("The track elements.")]
    // Create a List of Tracks ie: Tracks and start with 2 track elements ie: [0,1]
    [SerializeField] private List<Track> _tracks = new List<Track>(new Track[2]);

    [Tooltip("Set if using shader graph.")]
    // Bool for if using shader graph
    [SerializeField] private bool usingShaderGraph;

    [Tooltip("Set the main texture for shader graph.")]
    // String for setting the main texture for shader graph
    [SerializeField] private string setMainTexture = "_Albedo_Map";

    [Tooltip("The scroll speed.")]
    // Scroll the main texture based on time
    [SerializeField] private float scrollSpeed = 0.5f;

    [Tooltip("The invert scroll left bool.")]
    // Invert the scroll left bool
    [SerializeField] private bool invertScrollLeft;

    [Tooltip("The invert scroll right bool.")]
    // Invert the scroll right bool
    [SerializeField] private bool invertScrollRight;

    // The offset float
    private float offset;

    // Update is called once per frame
    private void Update()
    {
        foreach (var track in _tracks)
        {
            if (track.left)
            {
                if (invertScrollLeft)
                {
                    offset = Time.time * -scrollSpeed;
                }

                else if (!invertScrollLeft)
                {
                    offset = Time.time * scrollSpeed;
                }
            }

            if (track.right)
            {
                if (invertScrollRight)
                {
                    offset = Time.time * -scrollSpeed;
                }

                else if (!invertScrollRight)
                {
                    offset = Time.time * scrollSpeed;
                }
            }

            if (usingShaderGraph)
            {
                track.meshRenderer.materials[0].SetTextureOffset(setMainTexture, new Vector2(0f, offset));
            }

            else if (!usingShaderGraph)
            {
                track.meshRenderer.materials[0].mainTextureOffset = new Vector2(0f, offset);
            }            
        }
    }
}
