using System;
using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.Render.Camera;

namespace MegaLib.Mathematics.Geometry;

public struct Ray
{
  public Vector3 Position;
  public Vector3 Direction;
  public float Length;

  public Ray(Vector3 from, Vector3 to)
  {
    Position = from;
    Direction = (to - from).Normalized;
    Length = Vector3.Distance(from, to);
  }

  public Vector3 Start => Position;
  public Vector3 End => Position + Direction * Length;

  public override string ToString()
  {
    return $"Ray(P: {Position}, D: {Direction}, L: {Length})";
  }

  public static Ray FromCamera(Camera_Base cameraBase)
  {
    // Получение координат мыши и параметров экрана
    var mousePosition = Mouse.Client;
    var projectionMatrix = cameraBase.ProjectionMatrix;
    var viewMatrix = cameraBase.ViewMatrix;
    var screenWidth = Window.Current?.ClientWidth ?? 0f;
    var screenHeight = Window.Current?.ClientHeight ?? 0f;

    // Преобразование координат мыши в NDC (Normalized Device Coordinates)
    var x = 2.0f * mousePosition.X / screenWidth - 1.0f;
    var y = 1.0f - 2.0f * mousePosition.Y / screenHeight; // Инверсия Y для экрана
    var ndcNear = new Vector3(x, y, -1.0f); // Near clip плоскость, Z = -1
    var ndcFar = new Vector3(x, y, 1.0f); // Far clip плоскость, Z = 1

    // Инвертируем матрицу проекции и матрицу вида
    var invProjection = projectionMatrix.Inverted;
    var invView = viewMatrix.Inverted;

    // Преобразуем точки near и far из NDC в мировое пространство
    var nearClipWorld = Vector4.Transform(new Vector4(ndcNear, 1.0f), invProjection);
    nearClipWorld = Vector4.Transform(nearClipWorld, invView);

    var farClipWorld = Vector4.Transform(new Vector4(ndcFar, 1.0f), invProjection);
    farClipWorld = Vector4.Transform(farClipWorld, invView);

    // Преобразуем из гомогенных координат
    if (nearClipWorld.W != 0)
    {
      nearClipWorld /= nearClipWorld.W;
    }

    if (farClipWorld.W != 0)
    {
      farClipWorld /= farClipWorld.W;
    }

    // Рассчитываем начальную и конечную точки луча
    var rayStart = new Vector3(nearClipWorld.X, nearClipWorld.Y, nearClipWorld.Z); // точка на near clip
    var rayEnd = new Vector3(farClipWorld.X, farClipWorld.Y, farClipWorld.Z); // точка на far clip

    // Рассчитываем направление луча
    var direction = (rayEnd - rayStart).Normalized;

    // Для отладки можно вывести начальную и конечную точки луча
    // Console.WriteLine($"Ray Start: {rayStart}, Ray End: {rayEnd}");

    // Возвращаем луч с длиной 10, например
    return new Ray(rayStart, rayStart + direction * -40.0f);
  }
}