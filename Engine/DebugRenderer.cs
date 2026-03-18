using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlappyBird.Engine;

//
// Summary:
//      DebugRenderer is a simple utility class for drawing debug shapes.
//      It batches draw calls and should be efficient for drawing large numbers
//      of shapes.
public class DebugRenderer : IDisposable
{
    const int kDefaultCircleSegments = 16;
    const int kDefaultRoundCapSegments = 6;

    // --------------------------------------------------------------------
    // -- Private Members
    // --------------------------------------------------------------------
    private GraphicsDevice _graphicsDevice = null;
    private BasicEffect _effect = null;

    private int _triVertexCount = 0;
    private int _triVertexCapacity = 0;
    private VertexPositionColor[] _triVertices = null;

    private int _lineVertexCount = 0;
    private int _lineVertexCapacity = 0;
    private VertexPositionColor[] _lineVertices = null;

    // --------------------------------------------------------------------
    // -- Public Methods
    // --------------------------------------------------------------------
    public DebugRenderer(int triCapacity, int lineCapacity, GraphicsDevice graphicsDevice)
    {
        if (lineCapacity <= 0)
        {
            throw new ArgumentException("lineCapacity must be greater than 0");
        }

        if (triCapacity <= 0)
        {
            throw new ArgumentException("triCapacity must be greater than 0");
        }

        if (graphicsDevice == null)
        {
            throw new ArgumentNullException("graphicsDevice");
        }

        _graphicsDevice = graphicsDevice;

        _triVertexCount = 0;
        _triVertexCapacity = triCapacity * 3;
        _triVertices = new VertexPositionColor[_triVertexCapacity];

        _lineVertexCount = 0;
        _lineVertexCapacity = lineCapacity * 3;
        _lineVertices = new VertexPositionColor[_lineVertexCapacity];

        _effect = new BasicEffect(graphicsDevice);
        _effect.World = Matrix.Identity;
        _effect.TextureEnabled = false;
        _effect.LightingEnabled = false;
        _effect.VertexColorEnabled = true;
    }


    public void Begin()
    {
        _triVertexCount = 0;
        _lineVertexCount = 0;
    }

    public void End(Matrix viewMatrix, Matrix projMatrix)
    {
        if (_triVertexCount == 0 && _lineVertexCount == 0)
            return;

        _effect.View = viewMatrix;
        _effect.Projection = projMatrix;

        _graphicsDevice.BlendState = BlendState.NonPremultiplied;
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;


        for (int p = 0; p < _effect.CurrentTechnique.Passes.Count; p++)
        {
            EffectPass pass = _effect.CurrentTechnique.Passes[p];
            pass.Apply();

            if (_triVertexCount > 0)
            {
                _graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, _triVertices, 0, _triVertexCount / 3);
            }

            if (_lineVertexCount > 0)
            {
                _graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, _lineVertices, 0, _lineVertexCount / 2);
            }
        }


        _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.BlendState = BlendState.Opaque;

        Clear();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _effect != null)
        {
            _effect.Dispose();
            _effect = null;
        }
    }

    public void DrawFilledCircle(Vector2 centre, float radius, Color fillColour)
    {
        DrawCircle(centre, radius, fillColour, Color.Black, true, 0.0f, kDefaultCircleSegments);
    }


    public void DrawFilledCircle(Vector3 centre, float radius, Color fillColour, Vector3 axis1, Vector3 axis2)
    {
        DrawCircle(centre, radius, fillColour, Color.Black, true, 0.0f, axis1, axis2, kDefaultCircleSegments);
    }


    public void DrawOutlineCircle(Vector2 centre, float radius, Color edgeColour, float edgeWidth)
    {
        DrawCircle(centre, radius, Color.Black, edgeColour, false, edgeWidth, kDefaultCircleSegments);
    }


    public void DrawOutlineCircle(Vector3 centre, float radius, Color edgeColour, float edgeWidth, Vector3 axis1, Vector3 axis2)
    {
        DrawCircle(centre, radius, Color.Black, edgeColour, false, edgeWidth, axis1, axis2, kDefaultCircleSegments);
    }


    public void DrawOutlineRect(Vector2 minCorner, Vector2 maxCorner, Color edgeColour)
    {
        DrawRect(minCorner, maxCorner, Color.Black, edgeColour, false, 1.0f);
    }


    public void DrawFilledRect(Vector2 minCorner, Vector2 maxCorner, Color colour)
    {
        DrawRect(minCorner, maxCorner, colour, Color.Black, true, 0.0f);
    }


    public void DrawFilledLine(Vector2 startPoint, Vector2 endPoint, Color colour, float width)
    {
        DrawFilledLine(startPoint, endPoint, colour, width, 0);
    }


    public void DrawFilledLineWithCaps(Vector2 startPoint, Vector2 endPoint, Color colour, float width)
    {
        DrawFilledLine(startPoint, endPoint, colour, width, kDefaultRoundCapSegments);
    }


    public void DrawFilledLineWithCaps(Vector2 startPoint, Vector2 endPoint, Color colour, float width, int roundCapSegments)
    {
        DrawFilledLine(startPoint, endPoint, colour, width, roundCapSegments);
    }


    public void DrawCircle(Vector2 centre, float radius, Color fillColour, Color edgeColour, bool filled, float edgeWidth, int segments)
    {
        float innerRadius = Math.Max(0.0f, radius - edgeWidth);
        float radiusRatio = radius / innerRadius;

        float radStep = ((float)Math.PI * 2.0f) / (float)(segments);
        float rad = radStep;

        Vector2 localLastPoint = new Vector2(innerRadius, 0.0f);
        Vector2 lastPoint = centre + localLastPoint;


        for (int i = 0; i < segments; ++i)
        {
            Vector2 localPoint = new Vector2((float)Math.Cos(rad) * innerRadius,
                                                (float)Math.Sin(rad) * innerRadius);
            Vector2 point = centre + localPoint;

            if (filled)
            {
                DrawFilledTriangle(centre, point, lastPoint, fillColour);
            }

            if (edgeWidth > 0.0f)
            {
                Vector2 pointOuter = centre + (localPoint * radiusRatio);
                Vector2 lastPointOuter = centre + (localLastPoint * radiusRatio);

                DrawFilledTriangle(lastPoint, lastPointOuter, point, edgeColour);
                DrawFilledTriangle(point, lastPointOuter, pointOuter, edgeColour);
            }


            localLastPoint = localPoint;
            lastPoint = point;
            rad += radStep;
        }
    }


    public void DrawCircle(Vector3 centre, float radius, Color fillColour, Color edgeColour, bool filled, float edgeWidth, Vector3 axis1, Vector3 axis2, int segments)
    {
        float innerRadius = Math.Max(radius - edgeWidth, 0.0f);
        float radiusRatio = radius / innerRadius;

        Vector3 normal = Vector3.Cross(axis1, axis2);
        Vector3 biSector1 = Vector3.Cross(axis1, normal);
        Vector3 biSector2 = Vector3.Cross(biSector1, normal);

        biSector1 *= innerRadius;
        biSector2 *= innerRadius;


        float radStep = ((float)Math.PI * 2.0f) / (float)(segments);
        float rad = radStep;

        Vector3 localLastPoint = biSector2;
        Vector3 lastPoint = centre + localLastPoint;


        for (int i = 0; i < segments; ++i)
        {
            Vector3 localPoint = (biSector1 * (float)Math.Sin(rad)) + (biSector2 * (float)Math.Cos(rad));
            Vector3 point = centre + localPoint;


            if (filled)
            {
                DrawFilledTriangle(centre, point, lastPoint, fillColour);
            }

            if (edgeWidth > 0.0f)
            {
                Vector3 pointOuter = centre + (localPoint * radiusRatio);
                Vector3 lastPointOuter = centre + (localLastPoint * radiusRatio);

                DrawFilledTriangle(lastPoint, lastPointOuter, point, edgeColour);
                DrawFilledTriangle(point, lastPointOuter, pointOuter, edgeColour);
            }


            localLastPoint = localPoint;
            lastPoint = point;
            rad += radStep;
        }
    }


    public void DrawFilledTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color colour)
    {
        DrawFilledTriangle(new Vector3(p1.X, p1.Y, 0.0f), new Vector3(p2.X, p2.Y, 0.0f), new Vector3(p3.X, p3.Y, 0.0f), colour);
    }


    public void DrawFilledTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color colour)
    {
        if (_triVertexCount >= _triVertexCapacity - 3)
        {
#if DEBUG
            Debug.WriteLine("DebugRenderer triangle vertex capacity exceeded. Increase capacity to draw more triangles.");
#endif
            return;
        }

        _triVertices[_triVertexCount].Position = p1;
        _triVertices[_triVertexCount++].Color = colour;

        _triVertices[_triVertexCount].Position = p2;
        _triVertices[_triVertexCount++].Color = colour;

        _triVertices[_triVertexCount].Position = p3;
        _triVertices[_triVertexCount++].Color = colour;
    }


    public void DrawLine(Vector2 startPoint, Vector2 endPoint, Color colour)
    {
        DrawLine(new Vector3(startPoint.X, startPoint.Y, 0.0f), new Vector3(endPoint.X, endPoint.Y, 0.0f), colour);
    }


    public void DrawLine(Vector3 startPoint, Vector3 endPoint, Color colour)
    {
        if (_lineVertexCount >= _lineVertexCapacity - 2)
        {
#if DEBUG
            Debug.WriteLine("DebugRenderer line vertex capacity exceeded. Increase capacity to draw more lines.");
#endif
            return;
        }

        _lineVertices[_lineVertexCount].Position = startPoint;
        _lineVertices[_lineVertexCount++].Color = colour;

        _lineVertices[_lineVertexCount].Position = endPoint;
        _lineVertices[_lineVertexCount++].Color = colour;
    }


    public void DrawFilledLine(Vector2 startPoint, Vector2 endPoint, Color colour, float width, int roundCapSegments)
    {
        Vector2 direction = endPoint - startPoint;
        direction.Normalize();

        Vector2 left = new Vector2(-direction.Y, direction.X);

        float halfWidth = width * 0.5f;
        Vector2 scaledHalfLeft = left * halfWidth;

        Vector2 p1 = startPoint + scaledHalfLeft;
        Vector2 p2 = startPoint - scaledHalfLeft;
        Vector2 p3 = endPoint - scaledHalfLeft;
        Vector2 p4 = endPoint + scaledHalfLeft;

        DrawFilledTriangle(p1, p2, p3, colour);
        DrawFilledTriangle(p3, p4, p1, colour);


        if (roundCapSegments > 0)
        {
            float radStep = (float)Math.PI / (float)(roundCapSegments);
            float rad = radStep;

            Vector2 lastPoint = p1;
            Vector2 scaledDirection = -direction * halfWidth;
            Vector2 scaledLeft = left * halfWidth;

            for (int i = 0; i < roundCapSegments - 1; ++i)
            {
                Vector2 point = startPoint + (scaledDirection * (float)Math.Sin(rad)) + (scaledLeft * (float)Math.Cos(rad));

                DrawFilledTriangle(startPoint, point, lastPoint, colour);

                lastPoint = point;
                rad += radStep;
            }

            DrawFilledTriangle(startPoint, p2, lastPoint, colour);

            rad += radStep;
            lastPoint = p3;
            for (int i = 0; i < roundCapSegments - 1; ++i)
            {
                Vector2 point = endPoint + (scaledDirection * (float)Math.Sin(rad)) + (scaledLeft * (float)Math.Cos(rad));

                DrawFilledTriangle(endPoint, point, lastPoint, colour);

                lastPoint = point;
                rad += radStep;
            }

            DrawFilledTriangle(endPoint, p4, lastPoint, colour);
        }
    }


    public void DrawFilledLine(Vector3 startPoint, Vector3 endPoint, Vector3 normalisedUp, Color colour, float width)
    {
        Vector3 direction = endPoint - startPoint;
        direction.Normalize();

        Vector3 left = Vector3.Cross(direction, normalisedUp);

        Vector3 scaledHalfLeft = left * width * 0.5f;

        Vector3 p1 = startPoint + scaledHalfLeft;
        Vector3 p2 = startPoint - scaledHalfLeft;
        Vector3 p3 = endPoint - scaledHalfLeft;
        Vector3 p4 = endPoint + scaledHalfLeft;

        DrawFilledTriangle(p1, p2, p3, colour);
        DrawFilledTriangle(p3, p4, p1, colour);



        const int kSegments = 4;
        float radStep = (float)Math.PI / (float)kSegments;
        float rad = radStep;

        Vector3 lastPoint = p1;


        for (int i = 0; i < kSegments; ++i)
        {
            Vector3 localPoint = (left * (float)Math.Sin(rad)) + (direction * (float)Math.Cos(rad));
            Vector3 point = startPoint + localPoint;

            DrawFilledTriangle(startPoint, point, lastPoint, colour);

            lastPoint = point;
            rad += radStep;
        }


    }


    public void DrawQuad(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, Color colour)
    {
        DrawQuad(new Vector3(v1.X, v1.Y, 0.0f),
                    new Vector3(v2.X, v2.Y, 0.0f),
                    new Vector3(v3.X, v3.Y, 0.0f),
                    new Vector3(v4.X, v4.Y, 0.0f), colour);
    }


    public void DrawQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color colour)
    {
        DrawLine(v1, v2, colour);
        DrawLine(v2, v3, colour);
        DrawLine(v3, v4, colour);
        DrawLine(v4, v1, colour);
    }

    public void DrawRect(Rectangle rect, Color colour, Color edgeColour, bool filled, float edgeWidth)
    {
        DrawRect(
            new Vector2(rect.Left, rect.Top),
            new Vector2(rect.Right, rect.Bottom),
            colour, edgeColour, filled, edgeWidth);
    }

    public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color colour, Color edgeColour, bool filled, float edgeWidth)
    {
        Vector3 v1Outer = new Vector3(topLeft.X, topLeft.Y, 0.0f);
        Vector3 v2Outer = new Vector3(bottomRight.X, topLeft.Y, 0.0f);
        Vector3 v3Outer = new Vector3(bottomRight.X, bottomRight.Y, 0.0f);
        Vector3 v4Outer = new Vector3(topLeft.X, bottomRight.Y, 0.0f);

        if (filled)
        {
            DrawFilledTriangle(v1Outer, v2Outer, v3Outer, colour);
            DrawFilledTriangle(v3Outer, v4Outer, v1Outer, colour);
        }

        if (edgeWidth > 0.0f)
        {
            DrawLine(v1Outer, v2Outer, edgeColour);
            DrawLine(v2Outer, v3Outer, edgeColour);
            DrawLine(v3Outer, v4Outer, edgeColour);
            DrawLine(v4Outer, v1Outer, edgeColour);
        }
    }


    public void DrawQuad(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, Color filledColour, Color edgeColour, bool filled, float edgeWidth)
    {
        DrawQuad(new Vector3(v1.X, v1.Y, 0.0f),
                    new Vector3(v2.X, v2.Y, 0.0f),
                    new Vector3(v3.X, v3.Y, 0.0f),
                    new Vector3(v4.X, v4.Y, 0.0f),
                    filledColour, edgeColour, filled, edgeWidth);
    }


    public void DrawQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color filledColour, Color edgeColour, bool filled, float edgeWidth)
    {
        Vector3 v1Outer = v1;
        Vector3 v2Outer = v2;
        Vector3 v3Outer = v3;
        Vector3 v4Outer = v4;

        if (edgeWidth > 0.0f)
        {
            Vector3 e1Normalised = v2 - v1;
            Vector3 e2Normalised = v3 - v2;
            Vector3 e3Normalised = v4 - v3;
            Vector3 e4Normalised = v1 - v4;

            e1Normalised.Normalize();
            e2Normalised.Normalize();
            e3Normalised.Normalize();
            e4Normalised.Normalize();

            Vector3 v1Adjust = (e1Normalised - e4Normalised);
            Vector3 v2Adjust = (e2Normalised - e1Normalised);
            Vector3 v3Adjust = (e3Normalised - e2Normalised);
            Vector3 v4Adjust = (e4Normalised - e3Normalised);

            v1Adjust.Normalize();
            v2Adjust.Normalize();
            v3Adjust.Normalize();
            v4Adjust.Normalize();

            float halfWidth = 0.5f * edgeWidth;

            float v1AdjustMod = Math.Clamp(Vector3.Dot(e1Normalised, v1Adjust), -1.0f, 1.0f);
            float v2AdjustMod = Math.Clamp(Vector3.Dot(e2Normalised, v2Adjust), -1.0f, 1.0f);
            float v3AdjustMod = Math.Clamp(Vector3.Dot(e3Normalised, v3Adjust), -1.0f, 1.0f);
            float v4AdjustMod = Math.Clamp(Vector3.Dot(e4Normalised, v4Adjust), -1.0f, 1.0f);

            v1AdjustMod = halfWidth / (float)Math.Acos(v1AdjustMod);
            v2AdjustMod = halfWidth / (float)Math.Acos(v2AdjustMod);
            v3AdjustMod = halfWidth / (float)Math.Acos(v3AdjustMod);
            v4AdjustMod = halfWidth / (float)Math.Acos(v4AdjustMod);

            v1 += v1Adjust * v1AdjustMod;
            v2 += v2Adjust * v2AdjustMod;
            v3 += v3Adjust * v3AdjustMod;
            v4 += v4Adjust * v4AdjustMod;
        }

        if (filled)
        {
            DrawFilledTriangle(v1, v2, v3, filledColour);
            DrawFilledTriangle(v3, v4, v1, filledColour);
        }

        if (edgeWidth > 0.0f)
        {
            DrawFilledTriangle(v1, v1Outer, v2, edgeColour);
            DrawFilledTriangle(v2, v1Outer, v2Outer, edgeColour);

            DrawFilledTriangle(v2, v2Outer, v3, edgeColour);
            DrawFilledTriangle(v3, v2Outer, v3Outer, edgeColour);

            DrawFilledTriangle(v3, v3Outer, v4, edgeColour);
            DrawFilledTriangle(v4, v3Outer, v4Outer, edgeColour);

            DrawFilledTriangle(v4, v4Outer, v1, edgeColour);
            DrawFilledTriangle(v1, v4Outer, v1Outer, edgeColour);
        }
    }


    // --------------------------------------------------------------------
    // -- Private Methods
    // --------------------------------------------------------------------

    private void Clear()
    {
        _triVertexCount = 0;
        _lineVertexCount = 0;
    }

}
