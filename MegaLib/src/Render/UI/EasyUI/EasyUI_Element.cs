using System;
using System.Collections.Generic;
using System.Linq;
using MegaLib.Ext;
using MegaLib.IO;
using MegaLib.Mathematics.Geometry;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Color;
using MegaLib.Render.Layer;
using MegaLib.Render.RenderObject;

namespace MegaLib.Render.UI.EasyUI;

public class EasyUI_Element
{
  public bool IsVisible = true;

  public EasyUI_ElementEvents Events = new();
  public List<Triangle> Collision = [];
  public List<EasyUI_Element> Children = [];
  public List<RO_Line> Lines = [];

  public string Text
  {
    get => _text;
    set
    {
      if (_text == value) return;
      _text = value;
      _isTextChanged = true;
    }
  }

  private string _text;
  private bool _isTextChanged;

  public ElementStyle Style = new();
  private EasyUI_RenderData _backgroundRenderData = new() { Type = RenderDataType.Background };
  private EasyUI_RenderData _borderRenderData = new() { Type = RenderDataType.Border };
  private EasyUI_RenderData _textRenderData = new() { Type = RenderDataType.Text };
  private EasyUI_RenderData _lineRenderData = new() { Type = RenderDataType.Line };

  private bool _isClick;
  private bool _isMouseDown;
  private bool _isMouseDownIgnore;
  private bool _isMouseOver;
  public object Value; // Пользовательское значение

  protected FontData CurrentFontData;
  public Rectangle StencilRectangle;

  private float _lastClickTime;
  private int _clickCount;
  private float _timer;

  public EasyUI_Element Parent;
  public Layer_EasyUI LayerEasyUi;

  private bool _isNew = true;

  /*public EasyUI_Element()
  {

  }*/

  public void InitCollision(Rectangle r)
  {
    Collision.Clear();
    Collision.Add(new Triangle
    {
      A = new Vector3(r.FromX, r.FromY, 0),
      B = new Vector3(r.ToX, r.FromY, 0),
      C = new Vector3(r.ToX, r.ToY, 0)
    });
    Collision.Add(new Triangle
    {
      A = new Vector3(r.FromX, r.ToY, 0),
      B = new Vector3(r.FromX, r.FromY, 0),
      C = new Vector3(r.ToX, r.ToY, 0)
    });
  }

  public bool CheckCollision()
  {
    var ray = new Ray(
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, -10),
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, 10)
    );

    for (var i = 0; i < Collision.Count; i++)
    {
      Collision[i].RayIntersection(ray, out _, out var isHit);
      if (isHit) return true;
    }

    return false;
  }

  public virtual void Add(RO_Line line)
  {
    Lines.Add(line);
  }

  public virtual void Add(EasyUI_Element element)
  {
    Children.Add(element);
  }

  public virtual void Remove(EasyUI_Element element)
  {
    Children.Remove(element);
  }

  public virtual void Clear()
  {
    Children.Clear();
  }

  public virtual void ClearLines()
  {
    Lines.Clear();
  }

  public virtual void OnChanged()
  {
  }

  protected void HandleMouseEvents()
  {
    var isHovering = CheckCollision();

    // Если курсор над элементом. Вызываем OnMouseOver
    if (isHovering && !_isMouseOver)
    {
      _isMouseOver = true;
      Events?.OnMouseOver?.Invoke();
    }

    // Если курсор покинул элемент
    if (!isHovering && _isMouseOver)
    {
      _isMouseOver = false;
      Events?.OnMouseOut?.Invoke();
    }

    // Сбрасываем статус игнора если мышь отпущена
    if (Mouse.IsKeyUp(MouseKey.Left))
    {
      _isMouseDownIgnore = false;
    }

    // Нажал мышь за пределами элемента
    if (!_isMouseOver && Mouse.IsKeyDown(MouseKey.Left)) _isMouseDownIgnore = true;

    // Нажал мышь над элементом
    if (_isMouseOver && Mouse.IsKeyDown(MouseKey.Left) && !_isMouseDownIgnore && !_isMouseDown)
    {
      _isMouseDown = true;
      Events?.OnMouseDown?.Invoke();
    }

    // Клик только если мышь опущена на элементе
    if (_isMouseDown && _isMouseOver && Mouse.IsKeyUp(MouseKey.Left))
    {
      var currentTime = _timer; // Предположим, что у вас есть способ получить текущее время
      if (currentTime - _lastClickTime <= 0.3f)
      {
        _clickCount++;
      }
      else
      {
        _clickCount = 1; // Сброс количества кликов
      }

      _lastClickTime = currentTime;

      if (_clickCount == 2)
      {
        Events?.OnDoubleClick?.Invoke();
        _clickCount = 0; // Сброс после двойного клика
      }
      else
      {
        Events?.OnClick?.Invoke();
      }
    }

    // Отпустил мышь с нажатого элемента, причем неважно, где курсор
    if (_isMouseDown && Mouse.IsKeyUp(MouseKey.Left))
    {
      _isMouseDown = false;
      Events?.OnMouseUp?.Invoke();
    }
  }

  public void Build(EasyUI_BuildIn buildArgs)
  {
    _timer += buildArgs.Delta;
    LayerEasyUi = buildArgs.LayerEasyUi;
    Parent = buildArgs.Parent;

    if (!IsVisible) return;

    if (_isNew)
    {
      _isNew = false;
      return;
    }

    var stencilRectangleStack = new List<Rectangle>();
    if (buildArgs.StencilRectangleStack != null)
    {
      stencilRectangleStack.AddRange(buildArgs.StencilRectangleStack);
    }

    // buildArgs.StencilRectangleStack ??= [];

    CurrentFontData = buildArgs.FontData;

    // Событие до основного рендера
    Events?.OnBeforeRender?.Invoke(buildArgs.Delta);

    // Получаем bounding box текущего объекта в локальных координатах
    var elementBoundingBox = Style.BoundingBox;

    // Смещаем относительно родителя
    elementBoundingBox += buildArgs.ParentPosition;

    // Чекаем если элемент за пределами масок
    for (var i = 0; i < stencilRectangleStack.Count; i++)
    {
      if (!elementBoundingBox.IsIntersects(stencilRectangleStack[i]))
      {
        return;
      }
    }

    // Инициализация коллизии
    var hasCollision = Events.OnMouseDown != null || Events.OnMouseUp != null || Events.OnMouseOver != null ||
                       Events.OnMouseOut != null || Events.OnClick != null || Events.OnDoubleClick != null;
    if (hasCollision)
    {
      InitCollision(elementBoundingBox);
      HandleMouseEvents();
    }

    if (!StencilRectangle.IsEmpty)
    {
      var newMask = StencilRectangle + buildArgs.ParentPosition;
      for (var i = 0; i < stencilRectangleStack.Count; i++)
      {
        newMask = newMask.GetIntersection(stencilRectangleStack[i]);
      }

      stencilRectangleStack.Add(newMask);
    }

    // Установка стенсила если есть
    var stencilId = stencilRectangleStack.Count;

    if (!StencilRectangle.IsEmpty)
    {
      // Начинаем стенсил
      var stencil = new EasyUI_RenderData
      {
        Type = RenderDataType.StencilStart,
        StencilId = stencilId,
        BorderRadius = Style.BorderRadius
        //IsStencilStart = true,
      };
      stencil.DrawRectangle(stencilRectangleStack.Last(), new Vector4());
      buildArgs.RenderData.Add(stencil);
    }

    /*// Обводка. Если тело переместилось, обводку придется перерисовать
    if (Style.IsBorderChanged || Style.IsBackgroundChanged || buildArgs.IsParentChanged)
    {
      _borderRenderData.BorderRadius = Style.BorderRadius;
      _borderRenderData.Clear();
      if (Style.BorderWidth.LengthSquared > 0)
      {
        var borderBox = elementBoundingBox;
        borderBox.FromX -= Style.BorderWidth.X;
        // borderBox.ToX += Style.BorderWidth.X;
        _borderRenderData.DrawRectangle(borderBox, Style.BorderColor[0]);
      }

      buildArgs.Changes.Add(1);
    }

    // Если обводка есть то перерисовать
    if (Style.BorderWidth.LengthSquared > 0)
    {
      buildArgs.RenderData.Add(_borderRenderData);
    }*/

    // Бэкграунд по bounding box
    if (Style.IsBackgroundChanged || Style.IsBorderChanged || buildArgs.IsParentChanged)
    {
      _backgroundRenderData.BorderRadius = Style.BorderRadius;
      _backgroundRenderData.BorderWidth = Style.BorderWidth;
      _backgroundRenderData.BorderColor = Style.BorderColor;
      _backgroundRenderData.Clear();

      if (Style.BackgroundColor.A > 0 || Style.BorderWidth.LengthSquared > 0)
      {
        _backgroundRenderData.DrawRectangle(elementBoundingBox, Style.BackgroundColor);
      }

      buildArgs.Changes.Add(1);
    }

    // Если есть видимая область, то отправляем в рендер
    if (Style.BackgroundColor.A > 0 || Style.BorderWidth.LengthSquared > 0)
    {
      buildArgs.RenderData.Add(_backgroundRenderData);
    }

    // Обработка текста
    if (_isTextChanged || Style.IsTextChanged || Style.IsBackgroundChanged || buildArgs.IsParentChanged)
    {
      _textRenderData.Clear();
      if (Text != "")
        _textRenderData.DrawText(
          Text,
          Style.TextOffset,
          Style.TextAlignment,
          buildArgs.FontData,
          Style.TextColor,
          elementBoundingBox
        );
      _isTextChanged = false;
      buildArgs.Changes.Add(1);
      // Console.WriteLine("X");
    }

    if (Text != "") buildArgs.RenderData.Add(_textRenderData);

    // Обработка линий
    if (Style.IsBackgroundChanged || buildArgs.IsParentChanged)
    {
      _lineRenderData.Clear();
      _lineRenderData.Lines = Lines.ToArray().ToList();
      for (var i = 0; i < _lineRenderData.Lines.Count; i++)
      {
        var from = _lineRenderData.Lines[i];
        from.From += elementBoundingBox.Min;
        from.To += elementBoundingBox.Min;
        _lineRenderData.Lines[i] = from;
      }
    }

    if (Lines.Count > 0) buildArgs.RenderData.Add(_lineRenderData);

    /*if (Style.IsTextChanged)
    {
      _textRenderData = new RenderData
      {
        Type = RenderDataType.Text,
      };
      _textRenderData.DrawOutline(elementBoundingBox, Style.BorderWidth, Style.BorderColor);
      Style.IsTextChanged = false;
    }

    if (Text != "")
    {
      var textData = new EasyUI_RenderData
      {
        IsText = true
      };
      textData.DrawText(
        Text,
        Style.TextAlign,
        buildArgs.FontData,
        TextColor(),
        elementBoundingBox
      );
      RenderData.Add(textData);
    }*/

    // Проходимся по чилдам
    if (Children.Count > 0)
    {
      // var rList = new List<EasyUI_RenderData>();
      for (var i = 0; i < Children.Count; i++)
      {
        var child = Children[i];
        child.Build(buildArgs with
        {
          Parent = this,
          ParentPosition = buildArgs.ParentPosition + Style.Position,
          StencilRectangleStack = stencilRectangleStack.ToArray().ToList(),
          IsParentChanged = Style.IsBackgroundChanged || Style.IsBorderChanged || Style.IsTextChanged ||
                            buildArgs.IsParentChanged
        });

        // Копируем содержимое чилда
        // rList.AddRange(child.RenderData);
      }

      // Добавляем в основной список
      // if (rList.Count > 0) RenderData.AddRange(rList);
    }

    // Завершаем стенцил если установлен
    if (!StencilRectangle.IsEmpty)
    {
      var stencil = new EasyUI_RenderData
      {
        Type = RenderDataType.StencilStop,
        StencilId = stencilId
      };
      buildArgs.RenderData.Add(stencil);
    }

    // Событие рендера
    Events?.OnRender?.Invoke(buildArgs.Delta);

    // Применяем измнения
    if (Style.IsBackgroundChanged || buildArgs.IsParentChanged)
    {
      OnChanged();
    }

    if (Style.IsBackgroundChanged) Style.IsBackgroundChanged = false;
    if (Style.IsTextChanged) Style.IsTextChanged = false;
    if (Style.IsBorderChanged) Style.IsBorderChanged = false;
  }
}

/*public class EasyUI_Element
{
  public bool IsVisible = true;

  public EasyUI_ElementStyle Style = new();
  public List<EasyUI_RenderData> RenderData = [];
  public EasyUI_ElementEvents Events = new();
  public List<Triangle> Collision = [];
  public List<EasyUI_Element> Children = [];
  public string Text;

  private bool _isClick;
  private bool _isMouseDown;
  private bool _isMouseDownIgnore;
  private bool _isMouseOver;
  public object Value; // Пользовательское значение

  protected FontData CurrentFontData;
  public Rectangle StencilRectangle;

  private float _lastClickTime;
  private int _clickCount;
  private float _timer;

  public EasyUI_Element Parent;
  public Layer_EasyUI LayerEasyUi;

  public void InitCollision(Rectangle r)
  {
    Collision.Clear();
    Collision.Add(new Triangle
    {
      A = new Vector3(r.FromX, r.FromY, 0),
      B = new Vector3(r.ToX, r.FromY, 0),
      C = new Vector3(r.ToX, r.ToY, 0)
    });
    Collision.Add(new Triangle
    {
      A = new Vector3(r.FromX, r.ToY, 0),
      B = new Vector3(r.FromX, r.FromY, 0),
      C = new Vector3(r.ToX, r.ToY, 0)
    });
  }

  public bool CheckCollision()
  {
    var ray = new Ray(
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, -10),
      new Vector3(Mouse.ClientClamped.X, Mouse.ClientClamped.Y, 10)
    );

    for (var i = 0; i < Collision.Count; i++)
    {
      Collision[i].RayIntersection(ray, out _, out var isHit);
      if (isHit) return true;
    }

    return false;
  }

  public virtual void Add(EasyUI_Element element)
  {
    Children.Add(element);
  }

  public virtual void Remove(EasyUI_Element element)
  {
    Children.Remove(element);
  }

  public void Clear()
  {
    RenderData.Clear();
  }

  protected void HandleMouseEvents()
  {
    var isHovering = CheckCollision();

    // Если курсор над элементом. Вызываем OnMouseOver
    if (isHovering && !_isMouseOver)
    {
      _isMouseOver = true;
      Events?.OnMouseOver?.Invoke();
    }

    // Если курсор покинул элемент
    if (!isHovering && _isMouseOver)
    {
      _isMouseOver = false;
      Events?.OnMouseOut?.Invoke();
    }

    // Сбрасываем статус игнора если мышь отпущена
    if (Mouse.IsKeyUp(MouseKey.Left))
    {
      _isMouseDownIgnore = false;
    }

    // Нажал мышь за пределами элемента
    if (!_isMouseOver && Mouse.IsKeyDown(MouseKey.Left)) _isMouseDownIgnore = true;

    // Нажал мышь над элементом
    if (_isMouseOver && Mouse.IsKeyDown(MouseKey.Left) && !_isMouseDownIgnore && !_isMouseDown)
    {
      _isMouseDown = true;
      Events?.OnMouseDown?.Invoke();
    }

    // Клик только если мышь опущена на элементе
    if (_isMouseDown && _isMouseOver && Mouse.IsKeyUp(MouseKey.Left))
    {
      var currentTime = _timer; // Предположим, что у вас есть способ получить текущее время
      if (currentTime - _lastClickTime <= 0.3f)
      {
        _clickCount++;
      }
      else
      {
        _clickCount = 1; // Сброс количества кликов
      }

      _lastClickTime = currentTime;

      if (_clickCount == 2)
      {
        Events?.OnDoubleClick?.Invoke();
        _clickCount = 0; // Сброс после двойного клика
      }
      else
      {
        Events?.OnClick?.Invoke();
      }
    }

    // Отпустил мышь с нажатого элемента, причем неважно, где курсор
    if (_isMouseDown && Mouse.IsKeyUp(MouseKey.Left))
    {
      _isMouseDown = false;
      Events?.OnMouseUp?.Invoke();
    }
  }

  public virtual EasyUI_BuildOut Build(EasyUI_BuildIn buildArgs)
  {
    _timer += buildArgs.Delta;
    LayerEasyUi = buildArgs.LayerEasyUi;
    Parent = buildArgs.Parent;

    Clear();
    if (!IsVisible) return new EasyUI_BuildOut();

    var stencilRectangleStack = new List<Rectangle>();
    if (buildArgs.StencilRectangleStack != null)
    {
      stencilRectangleStack.AddRange(buildArgs.StencilRectangleStack);
    }

    // buildArgs.StencilRectangleStack ??= [];

    CurrentFontData = buildArgs.FontData;

    // Событие до основного рендера
    Events?.OnBeforeRender?.Invoke(buildArgs.Delta);

    // Получаем bounding box текущего объекта в локальных координатах
    var elementBoundingBox = BoundingBox();

    // Смещаем относительно родителя
    elementBoundingBox += buildArgs.ParentPosition;

    // Чекаем если элемент за пределами масок
    for (var i = 0; i < stencilRectangleStack.Count; i++)
    {
      if (!elementBoundingBox.IsIntersects(stencilRectangleStack[i]))
      {
        return new EasyUI_BuildOut();
      }
    }

    // Инициализация коллизии
    var hasCollision = Events.OnMouseDown != null || Events.OnMouseUp != null || Events.OnMouseOver != null ||
                       Events.OnMouseOut != null || Events.OnClick != null;
    if (hasCollision)
    {
      InitCollision(elementBoundingBox);
      HandleMouseEvents();
    }

    if (!StencilRectangle.IsEmpty)
    {
      var newMask = StencilRectangle + buildArgs.ParentPosition;
      for (var i = 0; i < stencilRectangleStack.Count; i++)
      {
        newMask = newMask.GetIntersection(stencilRectangleStack[i]);
      }

      stencilRectangleStack.Add(newMask);
    }

    // Установка стенсила если есть
    var stencilId = stencilRectangleStack.Count;

    if (!StencilRectangle.IsEmpty)
    {
      // Начинаем стенсил
      var stencil = new EasyUI_RenderData
      {
        IsStencilStart = true,
        StencilId = stencilId
      };
      stencil.DrawRectangle(stencilRectangleStack.Last(), new Vector4());
      RenderData.Add(stencil);
    }

    // Бэкграунд по bounding box
    var bgColor = BackgroundColor();
    if (bgColor.A > 0)
    {
      var background = new EasyUI_RenderData();
      background.DrawRectangle(elementBoundingBox, BackgroundColor());
      background.BorderRadius = BorderRadius();
      RenderData.Add(background);
    }

    if (Style.BorderWidth != null)
    {
      var outline = new EasyUI_RenderData();
      outline.DrawOutline(elementBoundingBox, BorderWidth(), BorderColor());
      RenderData.Add(outline);
    }

    if (Text != "")
    {
      var textData = new EasyUI_RenderData
      {
        IsText = true
      };
      textData.DrawText(
        Text,
        Style.TextAlign,
        buildArgs.FontData,
        TextColor(),
        elementBoundingBox
      );
      RenderData.Add(textData);
    }

    // Проходимся по чилдам
    if (Children.Count > 0)
    {
      var rList = new List<EasyUI_RenderData>();
      for (var i = 0; i < Children.Count; i++)
      {
        var child = Children[i];
        child.Build(buildArgs with
        {
          Parent = this,
          ParentPosition = buildArgs.ParentPosition + Position(),
          StencilRectangleStack = stencilRectangleStack.ToArray().ToList()
        });

        // Копируем содержимое чилда
        rList.AddRange(child.RenderData);
      }

      // Добавляем в основной список
      if (rList.Count > 0) RenderData.AddRange(rList);
    }

    // Завершаем стенцил если установлен
    if (!StencilRectangle.IsEmpty)
    {
      var stencil = new EasyUI_RenderData
      {
        IsStencilStop = true,
        StencilId = stencilId
      };
      RenderData.Add(stencil);
    }

    // Событие рендера
    Events?.OnRender?.Invoke(buildArgs.Delta);

    return new EasyUI_BuildOut();
  }
}*/