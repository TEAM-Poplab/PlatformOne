// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Microsoft.MixedReality.Toolkit.Experimental.UI
{
    /// <summary>
    /// Add a Dockable component to any object that has a <see cref="Dockable"/> and an <see cref="Microsoft.MixedReality.Toolkit.UI.ObjectManipulator"/>
    /// or <see cref="Microsoft.MixedReality.Toolkit.UI.ManipulationHandler"/> to make it dockable in Docks. That allows this object to be used
    /// as part of a palette, shelf or navigation bar together with other objects.
    /// </summary>
    /// <seealso cref="Dock"/>
    /// <seealso cref="DockPosition"/>
    [AddComponentMenu("Scripts/MRTK/Experimental/Dock/Dockable")]
    public class Dockable : MonoBehaviour
    {
        [Experimental]
        [SerializeField, ReadOnly]
        [Tooltip("Current state of this dockable in regards to a dock.")]
        private DockingState dockingState = DockingState.Undocked;

        [SerializeField]
        [Tooltip("Time to animate any move/scale into or out of the dock.")]
        private float moveLerpTime = 0.1f;

        [SerializeField]
        [Tooltip("Time to animate an element when it's following the dock (use 0 for tight attachment)")]
        private float moveLerpTimeWhenDocked = 0.05f;

        /// <summary>
        /// True if this object can currently be docked, false otherwise.
        /// </summary>
        public bool CanDock => dockingState == DockingState.Undocked || dockingState == DockingState.Undocking;

        /// <summary>
        /// True if this object can currently be undocked, false otherwise.
        /// </summary>
        public bool CanUndock => dockingState == DockingState.Docked;

        public Quaternion OriginalRotation => originalRotation;

        public bool BlockScaleToFit = false;

        // Constants
        private const float DistanceTolerance = 0.01f; // in meters
        private const float AngleTolerance = 3.0f; // in degrees
        private const float ScaleTolerance = 0.01f; // in percentage

        private DockPosition dockedPosition = null;
        private Vector3 dockedPositionScale = Vector3.one;
        private Vector3 savedDockedPositionScale = Vector3.zero;
        private Vector3 minimumScale = new Vector3(0.01f, 0.01f, 0.01f);

        private HashSet<DockPosition> overlappingPositions = new HashSet<DockPosition>();
        private Vector3 originalScale = Vector3.one;
        private Quaternion originalRotation = Quaternion.identity;
        private Vector3 originalPositionInParent = Vector3.zero;
        private bool isDragging = false;
        private ObjectManipulator objectManipulator;
        private ManipulationHandler manipulationHandler;

        private bool hasBeenDockedYet = false;  //when a geoemtry is taken from import dock directly to projection dock, it never passes to a dock and if then is docked using FastDock, it's dock scale is wrong

        public bool IsDragging
        {
            get => isDragging;
            set => isDragging = value;
        }

        /// <summary>
        /// Subscribes to manipulation events.
        /// </summary>
        //private void OnEnable()
        //{
        //    objectManipulator = gameObject.GetComponent<ObjectManipulator>();
        //    if (objectManipulator != null)
        //    {
        //        objectManipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
        //        objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
        //    }
        //    else
        //    {
        //        manipulationHandler = gameObject.GetComponent<ManipulationHandler>();
        //        if (manipulationHandler != null)
        //        {
        //            manipulationHandler.OnManipulationStarted.AddListener(OnManipulationStarted);
        //            manipulationHandler.OnManipulationEnded.AddListener(OnManipulationEnded);
        //        }
        //    }

        //    Assert.IsTrue(objectManipulator != null || manipulationHandler != null,
        //        "A Dockable object must have either an ObjectManipulator or a ManipulationHandler component.");

        //    Assert.IsNotNull(gameObject.GetComponent<Collider>(), "A Dockable object must have a Collider component.");
        //}

        /// <summary>
        /// Unsubscribes from manipulation events.
        /// </summary>
        private void OnDisable()
        {
            //if (objectManipulator != null)
            //{
            //    objectManipulator.OnManipulationStarted.RemoveListener(OnManipulationStarted);
            //    objectManipulator.OnManipulationEnded.RemoveListener(OnManipulationEnded);

            //    objectManipulator = null;
            //}

            //if (manipulationHandler != null)
            //{
            //    manipulationHandler.OnManipulationStarted.RemoveListener(OnManipulationStarted);
            //    manipulationHandler.OnManipulationEnded.RemoveListener(OnManipulationEnded);

            //    manipulationHandler = null;
            //}

            if (dockedPosition != null)
            {
                dockedPosition.DockedObject = null;
                dockedPosition = null;
            }

            overlappingPositions.Clear();
            dockingState = DockingState.Undocked;
        }

        /// <summary>
        /// Updates the transform and state of this object every frame, depending on 
        /// manipulations and docking state.
        /// </summary>
        public void Update()
        {
            if (isDragging && overlappingPositions.Count > 0)
            {
                var closestPosition = GetClosestPosition();
                if (closestPosition.IsOccupied)
                {
                    closestPosition.GetComponentInParent<Dock>().TryMoveToFreeSpace(closestPosition);
                }
            }

            if (dockingState == DockingState.Docked || dockingState == DockingState.Docking)
            {
                Assert.IsNotNull(dockedPosition, "When a dockable is docked, its dockedPosition must be valid.");
                Assert.AreEqual(dockedPosition.DockedObject, this, "When a dockable is docked, its dockedPosition reference the dockable.");

                var lerpTime = dockingState == DockingState.Docked ? moveLerpTimeWhenDocked : moveLerpTime;

                if (!isDragging)
                {
                    // Don't override dragging
                    transform.position = Solver.SmoothTo(transform.position, dockedPosition.transform.position, Time.deltaTime, lerpTime);
                    transform.rotation = Solver.SmoothTo(transform.rotation, dockedPosition.transform.rotation, Time.deltaTime, lerpTime);
                }

                transform.localScale = Solver.SmoothTo(transform.localScale, dockedPositionScale, Time.deltaTime, lerpTime);

                if (VectorExtensions.CloseEnough(dockedPosition.transform.position, transform.position, DistanceTolerance) &&
                    QuaternionExtensions.AlignedEnough(dockedPosition.transform.rotation, transform.rotation, AngleTolerance) &&
                    AboutTheSameSize(dockedPositionScale.x, transform.localScale.x))
                {
                    // Finished docking
                    dockingState = DockingState.Docked;

                    // Snap to position
                    transform.position = dockedPosition.transform.position;
                    transform.rotation = dockedPosition.transform.rotation;
                    transform.localScale = dockedPositionScale;
                }

                //Object rotation while docked
                if (dockingState == DockingState.Docked)
                {
                    //transform.GetChild(0).transform.RotateAround(transform.TransformPoint(dockedPosition.gameObject.transform.localPosition), Vector3.up, 25 * Time.deltaTime);
                    transform.GetChild(0).rotation = Quaternion.AngleAxis(25 * Time.deltaTime, Vector3.up)* transform.GetChild(0).rotation;
                }
            }
            else if (dockedPosition == null && dockingState == DockingState.Undocking)
            {
                transform.localScale = Solver.SmoothTo(transform.localScale, originalScale, Time.deltaTime, moveLerpTime);

                if (AboutTheSameSize(originalScale.x, transform.localScale.x))
                {
                    // Finished undocking
                    dockingState = DockingState.Undocked;

                    // Snap to size
                    transform.localScale = originalScale;
                    transform.GetChild(0).localRotation = originalRotation;
                    transform.GetChild(0).localPosition = originalPositionInParent;
                }
            }
        }

        //private void LateUpdate()
        //{
        //    if (dockingState == DockingState.Docked)
        //    {
        //        transform.localScale = dockedPositionScale; //Forcing consistent scale while docked
        //    }
        //}

        /// <summary>
        /// Docks this object in a given <see cref="DockPosition"/>.
        /// </summary>
        /// <param name="position">The <see cref="DockPosition"/> where we'd like to dock this object.</param>
        public void Dock(DockPosition position)
        {
            if (!CanDock)
            {
                Debug.LogError($"Trying to dock an object that was not undocked. State = {dockingState}");
                return;
            }

            Debug.Log($"Docking object {gameObject.name} on position {position.gameObject.name}");

            dockedPosition = position;
            dockedPosition.DockedObject = this;

            //ATM, when anobject is not docked or platformed (ie: being manipulated in player's hands), it goes back to its original manipulation scale set when imported
            //So the scaleToFit parameter, after being calculated just the first time the object is docked, doesn't need to be recalculated, ideally it's unchanged.
            //The value is controlled into GeometrySetModule class
            //if (!BlockScaleToFit)
            //{
            //    float scaleToFit = gameObject.GetComponent<Collider>().bounds.GetScaleToFitInside(dockedPosition.GetComponent<Collider>().bounds);
            //    dockedPositionScale = transform.localScale * scaleToFit;

            //    if (Vector3.Distance(minimumScale, dockedPositionScale) < 0)
            //    {
            //        dockedPositionScale = savedDockedPositionScale;
            //    }
            //    else
            //    {
            //        savedDockedPositionScale = dockedPositionScale;
            //    }
            //}

            if (!BlockScaleToFit)
            {
                dockedPositionScale = transform.localScale;
                while (dockedPositionScale.x * GetComponent<BoxCollider>().size.x > 0.12f)
                {
                    dockedPositionScale -= new Vector3(0.001f, 0.001f, 0.001f);
                }
            }

            if (dockingState == DockingState.Undocked)
            {
                // Only register the original scale and rotation when first docking
                originalScale = transform.localScale;
                originalRotation = transform.GetChild(0).localRotation;
                originalPositionInParent = transform.GetChild(0).localPosition;
            }

            dockingState = DockingState.Docking;            
            hasBeenDockedYet = true;
        }

        public void FastDock(DockPosition position)
        {
            if (!CanDock)
            {
                Debug.LogError($"Trying to dock an object that was not undocked. State = {dockingState}");
                return;
            }

            Debug.Log($"Docking object {gameObject.name} on position {position.gameObject.name}");

            dockedPosition = position;
            dockedPosition.DockedObject = this;

            //float scaleToFit = gameObject.GetComponent<Collider>().bounds.GetScaleToFitInside(dockedPosition.GetComponent<Collider>().bounds);
            //dockedPositionScale = transform.localScale * scaleToFit;

            //if (Vector3.Distance(minimumScale,dockedPositionScale) < 0f)
            //{
            //    dockedPositionScale = savedDockedPositionScale;
            //} else
            //{
            //    savedDockedPositionScale = dockedPositionScale;
            //}
            if (!hasBeenDockedYet)
            {
                if (!BlockScaleToFit)
                {
                    dockedPositionScale = transform.localScale;
                    while (dockedPositionScale.x * GetComponent<BoxCollider>().size.x > 0.12f)
                    {
                        dockedPositionScale -= new Vector3(0.001f, 0.001f, 0.001f);
                    }
                }
                hasBeenDockedYet = true;
            }

            if (dockingState == DockingState.Undocked)
            {
                // Only register the original scale and rotation when first docking
                originalScale = transform.localScale;
                originalRotation = transform.GetChild(0).localRotation;
                originalPositionInParent = transform.GetChild(0).localPosition;
            }

            dockingState = DockingState.Docking;
            isDragging = false;
        }

        /// <summary>
        /// Undocks this <see cref="Dockable"/> from the current <see cref="DockPosition"/> where it is docked.
        /// </summary>
        public void Undock()
        {
            if (!CanUndock)
            {
                Debug.LogError($"Trying to undock an object that was not docked. State = {dockingState}");
                return;
            }

            Debug.Log($"Undocking object {gameObject.name} from position {dockedPosition.gameObject.name}");

            dockedPosition.DockedObject = null;
            dockedPosition = null;
            //For now, original geometry scale won't be changed between docks
            //dockedPositionScale = Vector3.one;
            dockingState = DockingState.Undocking;
        }

        #region Collision events

        void OnTriggerEnter(Collider collider)
        {
            var dockPosition = collider.gameObject.GetComponent<DockPosition>();
            if (dockPosition != null)
            {
                overlappingPositions.Add(dockPosition);
                Debug.Log($"{gameObject.name} collided with {dockPosition.name}");
            }
        }

        void OnTriggerExit(Collider collider)
        {
            var dockPosition = collider.gameObject.GetComponent<DockPosition>();
            if (overlappingPositions.Contains(dockPosition))
            {
                overlappingPositions.Remove(dockPosition);
            }
        }

        #endregion

        #region Manipulation events

        private void OnManipulationStarted(ManipulationEventData e)
        {
            isDragging = true;

            if (CanUndock)
            {
                Undock();
            }
        }

        private void OnManipulationEnded(ManipulationEventData e)
        {
            isDragging = false;

            if (overlappingPositions.Count > 0 && CanDock)
            {
                var closestPosition = GetClosestPosition();
                if (closestPosition.IsOccupied)
                {
                    if (!closestPosition.GetComponentInParent<Dock>().TryMoveToFreeSpace(closestPosition))
                    {
                        return;
                    }
                }
                Dock(closestPosition);
            }
        }

        public void OnManipulationStarted()
        {
            isDragging = true;

            if (CanUndock)
            {
                Undock();
            }
        }

        public void OnManipulationEnded()
        {
            isDragging = false;

            if (overlappingPositions.Count > 0 && CanDock)
            {
                var closestPosition = GetClosestPosition();
                if (closestPosition.IsOccupied)
                {
                    if (!closestPosition.GetComponentInParent<Dock>().TryMoveToFreeSpace(closestPosition))
                    {
                        //Destroy(gameObject);
                        return;
                    }
                }
                Dock(closestPosition);
            }
        }

        #endregion

        /// <summary>
        /// Gets the overlapping <see cref="DockPosition"/> that is closest to this Dockable.
        /// </summary>
        /// <returns>The overlapping <see cref="DockPosition"/> that is closest to this <see cref="Dockable"/>, or null if no positions overlap.</returns>
        private DockPosition GetClosestPosition()
        {
            var bounds = gameObject.GetComponent<Collider>().bounds;
            var minDistance = float.MaxValue;
            DockPosition closestPosition = null;
            foreach (var position in overlappingPositions)
            {
                var distance = (position.gameObject.GetComponent<Collider>().bounds.center - bounds.center).sqrMagnitude;
                if (closestPosition == null || distance < minDistance)
                {
                    closestPosition = position;
                    minDistance = distance;
                }
            }

            return closestPosition;
        }

        #region Helpers

        private static bool AboutTheSameSize(float scale1, float scale2)
        {
            Assert.AreNotEqual(0.0f, scale2, "Cannot compare scales with an object that has scale zero.");
            return Mathf.Abs(scale1 / scale2 - 1.0f) < ScaleTolerance;
        }
        #endregion
    }
}
