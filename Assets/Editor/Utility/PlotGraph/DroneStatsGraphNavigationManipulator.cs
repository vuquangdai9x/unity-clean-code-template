using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaiVQScript.DroneSystem
{
    public class DroneStatsGraphNavigationManipulator : MouseManipulator
    {
        private const float wheelSensitivity = .01f;
        private const float dragSensitivity = 1f;

        private DroneStatsGraphView _graphView = null;


        public DroneStatsGraphNavigationManipulator(VisualElement target)
        {
            this.target = target;
            _graphView = (DroneStatsGraphView)target;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(PointerDownHandler);
            target.RegisterCallback<MouseMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<MouseUpEvent>(PointerUpHandler);
            target.RegisterCallback<MouseCaptureOutEvent>(PointerCaptureOutHandler);
            target.RegisterCallback<WheelEvent>(WheelHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(PointerDownHandler);
            target.UnregisterCallback<MouseMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<MouseUpEvent>(PointerUpHandler);
            target.UnregisterCallback<MouseCaptureOutEvent>(PointerCaptureOutHandler);
            target.UnregisterCallback<WheelEvent>(WheelHandler);
        }

        private Vector2 pointerStartPosition { get; set; }
        private Vector2 originCenterOffset { get; set; }

        private bool enabled { get; set; }

        private void PointerDownHandler(MouseDownEvent evt)
        {
            originCenterOffset = _graphView.centerOffset;
            pointerStartPosition = evt.mousePosition;
            target.CaptureMouse();
            enabled = true;
        }

        private void PointerMoveHandler(MouseMoveEvent evt)
        {
            if (enabled && target.HasMouseCapture())
            {
                Vector2 pointerDelta = evt.mousePosition - pointerStartPosition;
                _graphView.centerOffset = originCenterOffset + pointerDelta / _graphView.zoom * dragSensitivity;
                target.MarkDirtyRepaint();
            }
        }

        private void PointerUpHandler(MouseUpEvent evt)
        {
            if (enabled && target.HasMouseCapture())
            {
                target.ReleaseMouse();
                target.MarkDirtyRepaint();
            }
        }

        private void PointerCaptureOutHandler(MouseCaptureOutEvent evt)
        {
            if (enabled)
            {
                enabled = false;
                target.MarkDirtyRepaint();
            }
        }

        private void WheelHandler(WheelEvent evt)
        {
            if (evt.modifiers.HasFlag(EventModifiers.Shift))
            {
                Vector3 zoomDelta = new Vector3(-evt.delta.y * wheelSensitivity * _graphView.zoom.x, 0, 0);
                _graphView.zoom += zoomDelta;
            }
            else if (evt.modifiers.HasFlag(EventModifiers.Alt))
            {
                Vector3 zoomDelta = new Vector3(0, -evt.delta.y * wheelSensitivity * _graphView.zoom.y, 0);
                _graphView.zoom += zoomDelta;
            }
            else
            {
                Vector3 zoomDelta = -evt.delta.y * wheelSensitivity * _graphView.zoom;
                zoomDelta.z = 0;
                _graphView.zoom += zoomDelta;
            }
            target.MarkDirtyRepaint();
        }
    }
}