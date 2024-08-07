using MegaLib.IO;
using MegaLib.Mathematics.LinearAlgebra;

namespace MegaLib.Render.Camera;

public class Camera_Base
{
  protected Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
  protected Matrix4x4 _viewMatrix = Matrix4x4.Identity;

  public Matrix4x4 ProjectionMatrix => _projectionMatrix;

  public Matrix4x4 ViewMatrix
  {
    get => _viewMatrix;
    set => _viewMatrix = value;
  }

  private Vector3 _position;
  private Quaternion _rotation = Quaternion.Identity;

  public bool IsXInverted = true;
  public bool IsYInverted = true;
  public bool IsZInverted = false;

  public Vector3 Position
  {
    get => _position;
    set
    {
      _position = value;
      CalculateView();
    }
  }

  public Quaternion Rotation
  {
    get => _rotation;
    set
    {
      _rotation = value;
      CalculateView();
    }
  }

  private void CalculateView()
  {
    _viewMatrix = Matrix4x4.Identity;
    var p = _position;
    if (IsXInverted) p.X *= -1;
    if (IsYInverted) p.Y *= -1;
    if (IsZInverted) p.Z *= -1;
    // p.Z *= -1;

    _viewMatrix = _viewMatrix.Rotate(_rotation);
    _viewMatrix = _viewMatrix.Translate(p);
  }

  public void OffsetPosition(float x = 0f, float y = 0f, float z = 0f)
  {
    OffsetPosition(new Vector3(x, y, z));
  }

  public void OffsetPosition(Vector3 dir)
  {
    var hh = Rotation;
    var head = Matrix4x4.Identity.Rotate(hh);

    var dirNew = dir.AddW(1.0f) * head;
    // this.position.add_(dirNew.toVector3());
    Position += dirNew.DropW();

    // this._positionOffset = this._positionOffset.add(dirNew.toVector3());
  }

  public void BasicMovement(float delta)
  {
    if (Keyboard.IsKeyDown(KeyboardKey.A)) OffsetPosition(-delta);
    if (Keyboard.IsKeyDown(KeyboardKey.D)) OffsetPosition(delta);

    if (Keyboard.IsKeyDown(KeyboardKey.W)) OffsetPosition(z: delta);
    if (Keyboard.IsKeyDown(KeyboardKey.S)) OffsetPosition(z: -delta);

    if (Keyboard.IsKeyDown(KeyboardKey.Q)) Rotation = Rotation.RotateEuler(0f, -delta, 0f, "rad");
    if (Keyboard.IsKeyDown(KeyboardKey.E)) Rotation = Rotation.RotateEuler(0f, delta, 0f, "rad");

    if (Keyboard.IsKeyDown(KeyboardKey.ArrowUp)) OffsetPosition(y: delta);
    if (Keyboard.IsKeyDown(KeyboardKey.ArrowDown)) OffsetPosition(y: -delta);
  }
}