using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaiVQScript.DroneSystem
{
    public class DroneStatsGraphView : ImmediateModeElement
    {
        public new class UxmlFactory : UxmlFactory<DroneStatsGraphView> { }

        public DroneStatsGraphView()
        {
            this.AddManipulator(new DroneStatsGraphNavigationManipulator(this));
        }

        public class Axis
        {
            public string name;
            public float min;
            public float max;
            public int numRoundDigits;

            public Axis(string name, float min, float max, int numRoundDigits)
            {
                this.name = name;
                this.min = min;
                this.max = max;
                this.numRoundDigits = numRoundDigits;
            }
        }
        private Axis _xAxis = null, _yAxis = null; 

        public struct PlotData
        {
            public string name;
            public Vector2[] points;
            public Color lineColor;
            public Color pointColor;

            public PlotData(string name, Vector2[] points, Color lineColor, Color pointColor)
            {
                this.name = name;
                this.points = points;
                this.lineColor = lineColor;
                this.pointColor = pointColor;
            }
        }
        private PlotData[] _plotDatas = null;

        private static readonly Color colorBgr = new Color(0.1f,0.1f,0.1f,1f);
        private static readonly bool isDrawPoint = true;
        private static readonly bool isDrawPointValue = true;
        private static readonly Vector3 labelOffsetPosition = Vector3.up;
        private static readonly int numDigitsRound = 1;
        private static readonly Vector2 plotRectOffset = new Vector2(0.05f, 0.075f);
        private static readonly Vector2 plotRectScale = new Vector2(0.925f,0.9f);

        private static readonly Color colorAxis = Color.gray;
        private static readonly Color colorAxisLabel = Color.gray;
        private static readonly Vector2 axisLineExceed = Vector2.one * 0.01f;

        private static readonly int maxNumYStepAllowed = 30;
        private static readonly int[] availableStepNums = new int[] { 1, 5, 10, 20, 25, 50, 100, 200, 250, 500, 1000, 2000, 2500, 5000, 10000, 12000, 12500, 25000, 50000, 100000 };

        private static readonly Vector2 legendMargin = new Vector2(0.005f, 0.02f);
        private static readonly float legendLineLength = 0.03f;
        private static readonly float legendLabelSpace = 0.005f;
        private static readonly float legendSpacing = 0.03f;

        // navigation
        public Vector2 centerOffset = Vector2.zero;
        public Vector3 zoom = Vector2.one;
        public Rect YAxisRectArea => new Rect(0, 0 + this.contentRect.height * (1f - plotRectScale.y - plotRectOffset.y), this.contentRect.width * plotRectOffset.x, this.contentRect.height * plotRectScale.y);
        public Rect XAxisRectArea => new Rect(0 + this.contentRect.width * plotRectOffset.x, this.contentRect.height * (1f - plotRectOffset.y), this.contentRect.width * plotRectScale.x, this.contentRect.height * plotRectOffset.y);
        public Rect PlotRectArea => new Rect(0 + this.contentRect.width * plotRectOffset.x, 0 + this.contentRect.height * (1f - plotRectScale.y - plotRectOffset.y), this.contentRect.width * plotRectScale.x, this.contentRect.height * plotRectScale.y);

        public void SetPlotData(Axis xAxis, Axis yAxis, PlotData[] plotDatas)
        {
            _xAxis = xAxis;
            _yAxis = yAxis;
            _plotDatas = plotDatas;

            centerOffset = Vector2.zero;
            zoom = Vector3.one;
        }

        protected override void ImmediateRepaint()
        {
            var rect = new Rect(0, 0, this.contentRect.width, this.contentRect.height);
            EditorGUI.DrawRect(rect, colorBgr);
            DrawAxisX(XAxisRectArea);
            DrawAxisY(YAxisRectArea);
            Plot(PlotRectArea);
            DrawLegend(PlotRectArea);
        }

        private void DrawAxisX(Rect rect)
        {
            if (_xAxis == null) return;

            float step = 1;

            int numSteps = Mathf.FloorToInt(_xAxis.max / step);

            UnityEditor.Handles.color = colorAxis;

            UnityEditor.Handles.DrawLine(
                new Vector3(rect.xMin, rect.yMin),
                new Vector3(rect.xMax, rect.yMin)
                );

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = colorAxisLabel;
            labelStyle.alignment = TextAnchor.MiddleRight;

            float yMin = 0;
            float yMax = 1;
            for (int i = 0; i <= numSteps; i++)
            {
                DrawPointOnAxis(i * step);
            }
            DrawPointOnAxis(1f);

            void DrawPointOnAxis(float value)
            {
                float roundMuliplier = 1f;
                for (int i = 0; i < _xAxis.numRoundDigits; i++) roundMuliplier *= 10f;

                Vector3 pos = new Vector3((value - _xAxis.min) / (_xAxis.max - _xAxis.min), 1f, 0);
                Vector3 drawPos = new Vector3(rect.xMin + pos.x * rect.width, rect.yMax - ((pos.y - yMin) / (yMax - yMin)) * rect.height, 0);
                Vector3 transformedPos = drawPos;
                transformedPos.x = (drawPos.x - rect.center.x + centerOffset.x) * zoom.x + rect.center.x;

                UnityEditor.Handles.DrawSolidDisc(transformedPos, Vector3.back, 2f);
                UnityEditor.Handles.Label(transformedPos, (Mathf.Round(value * roundMuliplier) / roundMuliplier).ToString(), labelStyle);
            }
        }

        private void DrawAxisY(Rect rect)
        {
            if (_yAxis == null) return;

            float step = 0;

            for (int i = 0; i < availableStepNums.Length; i++)
            {
                if (Mathf.CeilToInt(_yAxis.max / availableStepNums[i]) <= maxNumYStepAllowed)
                {
                    step = availableStepNums[i];
                    break;
                }
            }

            int numSteps = Mathf.FloorToInt(_yAxis.max / step);

            UnityEditor.Handles.color = colorAxis;

            UnityEditor.Handles.DrawLine(
                new Vector3(rect.xMax, rect.yMin),
                new Vector3(rect.xMax, rect.yMax)
                );

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = colorAxisLabel;
            labelStyle.alignment = TextAnchor.MiddleRight;

            float yMin = 0;
            float yMax = 1;
            for (int i = 0; i <= numSteps; i++)
            {
                DrawPointOnAxis(i * step);
            }
            DrawPointOnAxis(1f);

            void DrawPointOnAxis(float value)
            {
                float roundMuliplier = 1f;
                for (int i = 0; i < _yAxis.numRoundDigits; i++) roundMuliplier *= 10f;

                Vector3 pos = new Vector3(1f, (value - _yAxis.min) / (_yAxis.max - _yAxis.min), 0);
                Vector3 drawPos = new Vector3(rect.xMin + pos.x * rect.width, rect.yMax - ((pos.y - yMin) / (yMax - yMin)) * rect.height, 0);
                Vector3 transformedPos = drawPos;
                transformedPos.y = (drawPos.y - rect.center.y + centerOffset.y) * zoom.y + rect.center.y;
                UnityEditor.Handles.DrawSolidDisc(transformedPos, Vector3.back, 2f);
                UnityEditor.Handles.Label(transformedPos, (Mathf.Round(value * roundMuliplier) / roundMuliplier).ToString(), labelStyle);
            }
        }
        private void DrawLegend(Rect rect)
        {
            if (_plotDatas == null) return;

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleLeft;
            for (int plotIndex = 0; plotIndex < _plotDatas.Length; plotIndex++)
            {
                Vector3 lineStart = new Vector3(rect.xMin + legendMargin.x * rect.width, rect.yMin + legendMargin.y * rect.height + legendSpacing * plotIndex * rect.height, 0);
                Vector3 lineEnd = lineStart + Vector3.right * legendLineLength * rect.width;
                UnityEditor.Handles.color = _plotDatas[plotIndex].lineColor;
                UnityEditor.Handles.DrawLine(lineStart, lineEnd);

                labelStyle.normal.textColor = _plotDatas[plotIndex].pointColor;
                UnityEditor.Handles.Label(lineEnd + Vector3.right * legendLabelSpace * rect.width, _plotDatas[plotIndex].name, labelStyle);
            }
        }

        private void Plot(Rect rect)
        {
            if (_plotDatas == null) return;

            float roundMuliplier = 1f;
            for (int i = 0; i < _yAxis.numRoundDigits; i++) roundMuliplier *= 10f;

            float yMin = 0;
            float yMax = 1;

            GUIStyle labelStyle = new GUIStyle();

            for (int plotIndex = 0; plotIndex < _plotDatas.Length; plotIndex++)
            {
                Vector3 prevPos = new Vector3((_plotDatas[plotIndex].points[0].x - _xAxis.min) / (_xAxis.max - _xAxis.min), (_plotDatas[plotIndex].points[0].y - _yAxis.min) / (_yAxis.max - _yAxis.min), 0);
                UnityEditor.Handles.color = _plotDatas[plotIndex].lineColor;
                for (int i = 0; i < _plotDatas[plotIndex].points.Length; i++)
                {
                    float x = _plotDatas[plotIndex].points[i].x;
                    float y = _plotDatas[plotIndex].points[i].y;

                    Vector3 pos = new Vector3((x - _xAxis.min) / (_xAxis.max - _xAxis.min), (y - _yAxis.min) / (_yAxis.max - _yAxis.min), 0);

                    Vector3 prevDrawPos = new Vector3(rect.xMin + prevPos.x * rect.width, rect.yMax - ((prevPos.y - yMin) / (yMax - yMin)) * rect.height, 0);
                    Vector3 drawPos = new Vector3(rect.xMin + pos.x * rect.width, rect.yMax - ((pos.y - yMin) / (yMax - yMin)) * rect.height, 0);

                    Vector3 transformedPrevPos = Vector3.Scale(prevDrawPos - (Vector3)rect.center + (Vector3)centerOffset, zoom) + (Vector3)rect.center;
                    Vector3 transformedPos = Vector3.Scale(drawPos - (Vector3)rect.center + (Vector3)centerOffset, zoom) + (Vector3)rect.center;

                    UnityEditor.Handles.DrawLine(transformedPrevPos, transformedPos);

                    if (isDrawPoint)
                    {
                        UnityEditor.Handles.color = _plotDatas[plotIndex].lineColor;
                        UnityEditor.Handles.DrawSolidDisc(transformedPos, Vector3.back, 2f);
                    }
                    if (isDrawPointValue)
                    {
                        labelStyle.normal.textColor = _plotDatas[plotIndex].pointColor;
                        UnityEditor.Handles.Label(transformedPos + labelOffsetPosition, (Mathf.Round(y * roundMuliplier) / roundMuliplier).ToString(), labelStyle);
                    }

                    prevPos = pos;
                }
            }
        }

        private void DrawRectBound(Rect rect)
        {
            UnityEditor.Handles.color = Color.gray;
            UnityEditor.Handles.DrawPolyLine(new Vector3[] {
                new Vector3(rect.xMin, rect.yMin, 0),
                new Vector3(rect.xMax, rect.yMin, 0),
                new Vector3(rect.xMax, rect.yMax, 0),
                new Vector3(rect.xMin, rect.yMax, 0),
                new Vector3(rect.xMin, rect.yMin, 0)
            });
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawSolidDisc(rect.center, Vector3.back, 2f);
        }
    }
}