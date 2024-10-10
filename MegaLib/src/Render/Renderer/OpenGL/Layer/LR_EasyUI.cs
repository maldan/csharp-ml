using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MegaLib.Mathematics.LinearAlgebra;
using MegaLib.OS;
using MegaLib.OS.Api;
using MegaLib.Render.Buffer;
using MegaLib.Render.Layer;
using MegaLib.Render.Scene;
using MegaLib.Render.UI.EasyUI;

namespace MegaLib.Render.Renderer.OpenGL.Layer;

public class LR_EasyUI : LR_Base
{
  private ListGPU<Vector3> _vertices;
  private ListGPU<uint> _indices;
  private ListGPU<Vector4> _colors;
  private ListGPU<Vector2> _uv;
  private uint _vaoId;
  private Matrix4x4 _mx = Matrix4x4.Identity;
  private float _tick;
  private int _minBuild;
  private int _maxBuild;
  private List<int> _history = [];

  public LR_EasyUI(OpenGL_Context context, Layer_Base layer, Render_Scene scene) : base(context, layer, scene)
  {
  }

  public override void Init()
  {
    # region vertex

    // language=glsl
    var vertex = @"#version 330 core
        precision highp float;
        precision highp int;
        precision highp usampler2D;
        precision highp sampler2D;

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec2 aUV;
        layout (location = 2) in vec4 aColor;
        
        uniform mat4 uProjectionMatrix;
        uniform mat4 uViewMatrix;
        uniform mat4 uModelMatrix;
        
        out vec3 vo_Position;
        out vec4 vo_Color;
        out vec2 vo_UV;
        
        void main() {
            gl_Position = uProjectionMatrix * uViewMatrix * uModelMatrix * vec4(aPosition.xyz, 1.0);
            
            vo_Position = (uModelMatrix * vec4(aPosition.xyz, 1.0)).xyz;
            vo_Color = aColor;
            vo_UV = aUV;
        }";

    #endregion

    #region fragment

    // language=glsl
    var fragment = @"
        #version 330 core
      precision highp float;
      precision highp int;
      precision highp sampler2D;

      in vec3 vo_Position; // Позиция вершины
      in vec4 vo_Color;    // Цвет вершины
      in vec2 vo_UV;       // UV координаты для текстуры

      out vec4 color;      // Выходной цвет фрагмента

      uniform sampler2D uFontTexture;
      uniform float uMode;          // Режим рендеринга
      uniform vec4 uRectangle;     // Прямоугольник: (centerX, centerY, halfWidth, halfHeight)
      uniform vec4 uCornerRadius;  // Радиусы скругления углов: (bottomLeft, bottomRight, topRight, topLeft)
      uniform vec4 uBorderWidths;  // Радиусы скругления углов: (bottomLeft, bottomRight, topRight, topLeft)
      uniform vec4[4] uBorderColor;  // Радиусы скругления углов: (bottomLeft, bottomRight, topRight, topLeft)
        
      // Определяем, в каком квадранте находится текущая точка
      float getRadius(vec2 p, vec4 radii) {
          if (p.x > 0.0 && p.y > 0.0)
              return radii.y; // Верхний правый
          if (p.x < 0.0 && p.y > 0.0)
              return radii.x; // Верхний левый
          if (p.x < 0.0 && p.y < 0.0)
              return radii.w; // Нижний левый
          if (p.x > 0.0 && p.y < 0.0)
              return radii.z; // Нижний правый
          return 0.0;
      }
          
      // Функция для расчёта SDF для прямоугольника с закруглёнными углами
      float sdfRoundedRect(vec2 uv, vec2 rectPos, vec2 rectSize, vec4 radii) {
          // Половина размеров прямоугольника
          vec2 halfSize = rectSize * 0.5;
          
          // Текущий радиус угла
          float radius = getRadius(uv - rectPos, radii);

          // Корректировка размеров прямоугольника с учётом радиуса
          vec2 adjustedHalfSize = halfSize - vec2(
              uv.x > rectPos.x ? radii.y : radii.x,
              uv.y > rectPos.y ? radii.y : radii.w
          );

          // SDF для прямоугольника с закруглёнными углами
          vec2 d = abs(uv - rectPos) - halfSize + vec2(radius);
          return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - radius;
      }
        
      int getRectRegion(vec2 uv, vec2 rectPos, vec2 rectHalfSize) {
    // Вершины прямоугольника относительно его центра
    vec2 topLeft = rectPos - rectHalfSize;
    vec2 topRight = vec2(rectPos.x + rectHalfSize.x, rectPos.y - rectHalfSize.y);
    vec2 bottomLeft = vec2(rectPos.x - rectHalfSize.x, rectPos.y + rectHalfSize.y);
    vec2 bottomRight = rectPos + rectHalfSize;

    // Уравнение первой диагонали (от верхнего левого до нижнего правого)
    float diag1 = (bottomRight.y - topLeft.y) / (bottomRight.x - topLeft.x);
    float line1 = diag1 * (uv.x - topLeft.x) + topLeft.y;

    // Уравнение второй диагонали (от верхнего правого до нижнего левого)
    float diag2 = (bottomLeft.y - topRight.y) / (bottomLeft.x - topRight.x);
    float line2 = diag2 * (uv.x - topRight.x) + topRight.y;

    // Определяем, в каком треугольнике находится точка
    if (uv.y < line1 && uv.y < line2) {
        return 3; 
    } else if (uv.y < line1 && uv.y > line2) {
        return 2; 
    } else if (uv.y > line1 && uv.y > line2) {
        return 1; 
    } else if (uv.y > line1 && uv.y < line2) {
        return 4;
    }

    return 0;  // На диагоналях
}

bool isAboveDiagonal(vec2 point, vec2 v1, vec2 v3) {
    float diagonal = (v3.x - v1.x) * (point.y - v1.y) - (v3.y - v1.y) * (point.x - v1.x);
    return diagonal > 0.0;
}

int getBorderRegion(vec2 uv, vec2 rectPos, vec2 rectSize, vec4 borderWidths, vec4 cornerRadius) {
    // Вершины прямоугольника относительно его центра
    vec2 topLeft = rectPos + vec2(rectSize.x * -0.5, rectSize.y * 0.5);
    vec2 topRight = rectPos + rectSize * 0.5;
    vec2 bottomRight = rectPos + vec2(rectSize.x * 0.5, rectSize.y * -0.5);
    vec2 bottomLeft = rectPos + vec2(rectSize.x * -0.5, rectSize.y * -0.5);
    
    float topSection = rectPos.y + rectSize.y * 0.5 - borderWidths[0];
    float bottomSection = rectPos.y - rectSize.y * 0.5 + borderWidths[2];
    float rightSection = rectPos.x + rectSize.x * 0.5 - borderWidths[1];
    float leftSection = rectPos.x - rectSize.x * 0.5 + borderWidths[3];
    
    // if (distance(uv, bottomLeft) < 15.0) return 1;
    
    if (uv.y > topSection && uv.x > rightSection) {
        if (isAboveDiagonal(uv, topRight - vec2(borderWidths[1], borderWidths[0]), topRight)) {
            return 1;
        } else {
            return 2;
        }
    }
    
    if (uv.y > topSection && uv.x < leftSection) {
        if (isAboveDiagonal(uv, topLeft + vec2(borderWidths[3], -borderWidths[0]), topLeft)) {
            return 4;
        } else {
            return 1;
        }
    }

    if (uv.y < bottomSection && uv.x < leftSection) {
        if (isAboveDiagonal(uv, bottomLeft + vec2(borderWidths[3], borderWidths[2]), bottomLeft)) {
            return 3;
        } else {
            return 4;
        }
    }
    
    if (uv.y < bottomSection && uv.x > rightSection) {
        if (isAboveDiagonal(uv, bottomRight - vec2(borderWidths[1], -borderWidths[2]), bottomRight)) {
            return 2;
        } else {
            return 3;
        }
    }
    
    if (uv.y > topSection) return 1;
    if (uv.y < bottomSection) return 3;
    if (uv.x > rightSection) return 2;
    if (uv.x < leftSection) return 4;
    
    return 0;
}







    int getBorderRegion2(vec2 uv, vec2 rectPos, vec2 rectSize, vec4 borderWidths, vec4 cornerRadius) {
        if (uv.y > rectPos.y + rectSize.y * 0.5 - borderWidths[0]) {
            return 1;
        }
        if (uv.y < rectPos.y - rectSize.y * 0.5 + borderWidths[2]) {
            return 3;
        }
        if (uv.x > rectPos.x + rectSize.x * 0.5 - borderWidths[1]) {
            return 2;
        }
        if (uv.x < rectPos.x - rectSize.x * 0.5 + borderWidths[3]) {
            return 4;
        }
        return 0;
    }



        
      void main() {
        // Stencil mode
           if (uMode == 3.0) {
             // Позиция фрагмента относительно центра прямоугольника
              vec2 uv = gl_FragCoord.xy;

              // Вызываем функцию SDF для расчёта дистанции
              float dist = sdfRoundedRect(uv, uRectangle.xy, uRectangle.zw, uCornerRadius);
              float alpha = smoothstep(0.0, 1.0, -dist);
              if (alpha <= 0.0) discard;
              color = vo_Color;
             return;
           } else
          // Проверка режима рендеринга
          if (uMode == 2.0) {
              // Режим 2: Отображение текстуры шрифта
              vec4 texelColor = texture(uFontTexture, vo_UV) * vo_Color;
              if (texelColor.a <= 0.01) discard;
              color = texelColor;
          }
          else if (uMode == 1.0) {
              // Режим 1: Отображение сплошного цвета
              color = vo_Color;
          }
          else {
              // Позиция фрагмента относительно центра прямоугольника
              vec2 uv = gl_FragCoord.xy;

              // Вызываем функцию SDF для расчёта дистанции
              float dist = sdfRoundedRect(uv, uRectangle.xy, uRectangle.zw, uCornerRadius);
              float alpha = smoothstep(0.0, 1.0, -dist);
              float alpha2 = smoothstep(0.0, 0.05, -dist);
              
              // Если углы закругленные. То работает только 1 цвет и 1 толщина на все
              if (length(uCornerRadius) > 0.0) {
                if (dist > -uBorderWidths[0]) {
                  color = uBorderColor[0].rgba * vec4(1.0, 1.0, 1.0, alpha2);
                  return;
                }
                color = vo_Color.rgba * vec4(1.0, 1.0, 1.0, alpha);
                return;
              }
              
              // Без закругления работает нормально
              int rectId = getBorderRegion(uv, uRectangle.xy, uRectangle.zw, uBorderWidths, uCornerRadius) - 1;
              if (rectId >= 0) {
                if (dist >= -uBorderWidths[rectId] && dist <= uBorderWidths[rectId]) {
                  color = uBorderColor[rectId].rgba;
                  return;
                }
              }
              
              // Устанавливаем цвет фрагмента с альфа-каналом
              color = vo_Color.rgba * vec4(1.0, 1.0, 1.0, alpha);
          }
      }
    ";

    #endregion

    Shader.ShaderCode["vertex"] = vertex;
    Shader.ShaderCode["fragment"] = fragment;
    Shader.Compile();

    _vertices = [];
    _colors = [];
    _indices = [];
    _uv = [];
    Context.MapBuffer(_vertices);
    Context.MapBuffer(_uv);
    Context.MapBuffer(_colors);
    Context.MapBuffer(_indices, true);

    // Create vao
    OpenGL32.glGenVertexArrays(1, ref _vaoId);
  }

  private void RenderLines(EasyUI_RenderData rd)
  {
    var layer = (Layer_EasyUI)Layer;

    _vertices.Clear();
    _colors.Clear();

    foreach (var line in rd.Lines)
    {
      _vertices.Add(line.From);
      _vertices.Add(line.To);
      _colors.Add(new Vector4(line.FromColor.R, line.FromColor.G, line.FromColor.B,
        line.FromColor.A));
      _colors.Add(new Vector4(line.ToColor.R, line.ToColor.G, line.ToColor.B,
        line.ToColor.A));
    }

    // Загружаем на гпу
    _vertices.Sync();
    _colors.Sync();

    // Биндим vao
    OpenGL32.glBindVertexArray(_vaoId);

    // Активируем атрибуты
    Shader.EnableAttribute(_vertices, "aPosition");
    Shader.EnableAttribute(_colors, "aColor");

    // Режим линий
    Shader.SetUniform("uMode", 1);

    // Рисуем
    OpenGL32.glDrawArrays(OpenGL32.GL_LINES, 0, rd.Lines.Count * 2);

    // Разбиндим vao
    OpenGL32.glBindVertexArray(0);
  }

  private void RenderMain(EasyUI_RenderData rd)
  {
    var layer = (Layer_EasyUI)Layer;

    // Если нет вертексов то смысла дальше нет идти
    if (rd.Vertices.Count == 0) return;

    _vertices.Clear();
    _uv.Clear();
    _colors.Clear();
    _indices.Clear();

    _vertices.AddRange(rd.Vertices);
    _uv.AddRange(rd.UV);
    _colors.AddRange(rd.Colors);
    _indices.AddRange(rd.Indices);

    // Загружаем на гпу
    _vertices.Sync();
    _uv.Sync();
    _colors.Sync();
    _indices.Sync();

    // Биндим vao
    OpenGL32.glBindVertexArray(_vaoId);

    // Активируем атрибуты
    Shader.EnableAttribute(_vertices, "aPosition");
    Shader.EnableAttribute(_uv, "aUV");
    Shader.EnableAttribute(_colors, "aColor");

    // Texture
    Shader.ActivateTexture(layer.FontTexture, "uFontTexture", 0);

    if (rd.Type == RenderDataType.StencilStart)
    {
      Shader.SetUniform("uMode", 3);
    }
    else if (rd.Type == RenderDataType.Text)
    {
      Shader.SetUniform("uMode", 2);
    }
    else
    {
      Shader.SetUniform("uMode", 0);
    }

    var bb = rd.BoundingBox;
    Shader.SetUniform("uRectangle", new Vector4(
      bb.X + bb.Width / 2f,
      Window.Current.ClientHeight - (bb.Y + bb.Height / 2f),
      bb.Width,
      bb.Height
    ));
    Shader.SetUniform("uCornerRadius", rd.BorderRadius);
    Shader.SetUniform("uBorderWidths", rd.BorderWidth); // Толщина обводки

    if (rd.BorderColor.Top.LengthSquared > 0)
      Shader.SetUniform("uBorderColor[0]", rd.BorderColor.Top); // Толщина обводки
    if (rd.BorderColor.Right.LengthSquared > 0)
      Shader.SetUniform("uBorderColor[1]", rd.BorderColor.Right); // Толщина обводки
    if (rd.BorderColor.Bottom.LengthSquared > 0)
      Shader.SetUniform("uBorderColor[2]", rd.BorderColor.Bottom); // Толщина обводки
    if (rd.BorderColor.Left.LengthSquared > 0)
      Shader.SetUniform("uBorderColor[3]", rd.BorderColor.Left); // Толщина обводки

    // Shader.SetUniform("uBorderColors", new Vector4(1, 1, 1, 1)); // Цвет обводки для каждой стороны

    // Биндим индексы
    OpenGL32.glBindBuffer(OpenGL32.GL_ELEMENT_ARRAY_BUFFER, Context.GetBufferId(_indices));

    // Рисуем
    OpenGL32.glDrawElements(OpenGL32.GL_TRIANGLES, _indices.Count, OpenGL32.GL_UNSIGNED_INT, IntPtr.Zero);

    // Разбиндим vao
    OpenGL32.glBindVertexArray(0);
  }

  public override void Render()
  {
    var tt = new Stopwatch();
    tt.Start();

    var layer = (Layer_EasyUI)Layer;
    var renderData = layer.Build(Scene.DeltaTime);
    tt.Stop();

    _history.Add((int)tt.ElapsedTicks);

    _tick += 1;
    if (_tick > 300)
    {
      _minBuild = Math.Min(_minBuild, (int)tt.ElapsedTicks);
      _maxBuild = Math.Max(_maxBuild, (int)tt.ElapsedTicks);
      Console.WriteLine($"Build: {_minBuild} {_history.Average()} {_maxBuild}");
      _tick = 0;
      _history.Clear();
    }

    Shader.Use();
    Shader.Enable(OpenGL32.GL_BLEND);
    Shader.Enable(OpenGL32.GL_DEPTH_TEST);

    //var cp = Scene.Camera.Position;
    // cp.Z *= -1;
    if (layer.Camera != null)
    {
      Shader.SetUniform("uProjectionMatrix", layer.Camera.ProjectionMatrix);
      Shader.SetUniform("uViewMatrix", layer.Camera.ViewMatrix);
    }
    else
    {
      Shader.SetUniform("uProjectionMatrix", Scene.Camera.ProjectionMatrix);
      Shader.SetUniform("uViewMatrix", Scene.Camera.ViewMatrix);
    }

    Shader.SetUniform("uModelMatrix", _mx);

    // Маппим текстуру шрифтов
    Context.MapTexture(layer.FontTexture);

    // Основной рендер

    tt.Start();
    renderData.ForEach(rd =>
    {
      // Игнорирование данных (это проще чем удалять из массива)
      // if (rd.IsIgnore) return;

      if (rd.Type == RenderDataType.StencilStop)
      {
        OpenGL32.glStencilFunc(OpenGL32.GL_EQUAL, rd.StencilId - 1, 0xFF);
        OpenGL32.glStencilMask(0x00); // Запрещаем запись в буфер
        if (rd.StencilId <= 1)
        {
          OpenGL32.glDisable(OpenGL32.GL_STENCIL_TEST);
        }
      }
      else if (rd.Type == RenderDataType.StencilStart)
      {
        OpenGL32.glEnable(OpenGL32.GL_STENCIL_TEST);

        // Устанавливаем операцию стэнсила
        // GL_KEEP — не изменяет стэнсил-буфер при неудачном тесте
        // GL_REPLACE — записывает новое значение (rd.StencilId) при успешном тесте
        OpenGL32.glStencilOp(OpenGL32.GL_KEEP, OpenGL32.GL_KEEP, OpenGL32.GL_REPLACE);

        // Настраиваем, чтобы всегда записывать текущее значение (например, 3, 2 или 1)
        OpenGL32.glStencilFunc(OpenGL32.GL_ALWAYS, rd.StencilId, 0xFF);
        OpenGL32.glStencilMask(0xFF); // Разрешаем запись в стэнсил-буфер

        if (rd.StencilId == 1) OpenGL32.glClear(OpenGL32.GL_STENCIL_BUFFER_BIT); // Очищаем стэнсил-буфер

        // Отрисовываем маску
        RenderMain(rd); // Рисуем маску

        // Теперь настраиваем стэнсил, чтобы рисовать только внутри всех предыдущих масок
        // GL_EQUAL — рисует только в тех местах, где значение в стэнсиле совпадает с rd.StencilId
        OpenGL32.glStencilFunc(OpenGL32.GL_EQUAL, rd.StencilId, 0xFF);
        OpenGL32.glStencilMask(0x00); // Запрещаем запись в буфер
      }
      else if (rd.Type == RenderDataType.Line)
      {
        RenderLines(rd);
      }
      else
      {
        RenderMain(rd);
      }
    });
    tt.Stop();
    // Console.WriteLine($"Render: {tt.ElapsedTicks}");
  }
}