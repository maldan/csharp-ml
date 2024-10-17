namespace MegaLib.Mathematics.LinearAlgebra;

public class Transform
{
  private bool _isChanged;
  private Matrix4x4 _matrix = Matrix4x4.Identity;
  private Vector3 _position = Vector3.Zero;
  private Quaternion _rotation = Quaternion.Identity;
  private Vector3 _scale = Vector3.One;

  public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
  {
    _matrix = _matrix.Translate(position);
    _matrix = _matrix.Rotate(rotation);
    _matrix = _matrix.Scale(scale);
  }

  public Transform(Matrix4x4 matrix)
  {
    _position = matrix.Position;
    _rotation = matrix.Rotation;
    _scale = matrix.Scaling;
    Calculate();
  }

  public Transform Clone()
  {
    return new Transform
    {
      Position = Position,
      Rotation = Rotation,
      Scale = Scale
    };
  }

  public Transform()
  {
    _matrix = _matrix.Translate(Vector3.Zero);
    _matrix = _matrix.Rotate(Quaternion.Identity);
    _matrix = _matrix.Scale(Vector3.One);
  }

  public Matrix4x4 Matrix
  {
    get
    {
      if (_isChanged) Calculate();
      return _matrix;
    }
    set
    {
      _isChanged = true;
      _matrix = value;
      _position = _matrix.Position;
      _rotation = _matrix.Rotation;
      _scale = _matrix.Scaling;
    }
  }

  public Vector3 Position
  {
    get => _position;
    set
    {
      _position = value;
      _isChanged = true;
    }
  }

  public Quaternion Rotation
  {
    get => _rotation;
    set
    {
      _rotation = value;
      _isChanged = true;
    }
  }

  public Vector3 Scale
  {
    get => _scale;
    set
    {
      _scale = value;
      _isChanged = true;
    }
  }

  private void Calculate()
  {
    _matrix = Matrix4x4.Identity;
    _matrix = _matrix.Translate(_position);
    _matrix = _matrix.Rotate(_rotation);
    _matrix = _matrix.Scale(_scale);
    _isChanged = false;
  }

  public static Transform operator *(Transform a, Transform b)
  {
    return new Transform { Matrix = a.Matrix * b.Matrix };
  }
}