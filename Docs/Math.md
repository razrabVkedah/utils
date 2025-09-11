# Rusleo.Utils — Интерполяции и сплайны

## Что это такое

Модуль интерполяции и сплайнов — это набор функций и классов, которые позволяют:

* **плавно смешивать значения** (например, для процедурных шумов, анимаций или переходов);
* **строить кривые по набору контрольных точек** (2D/3D пути, траектории, профили);
* **управлять плавностью движения** (с помощью fade-функций, Catmull-Rom и Hermite сплайнов).

---

## Fade-функции (сглаживание параметра)

Перед интерполяцией параметр `t ∈ [0,1]` можно пропустить через кривую сглаживания. Это управляет плавностью на концах:

* **Linear** — обычный `t` (просто и быстро).
* **Smoothstep** (`3t² - 2t³`) — сглаживает углы, C¹ непрерывность.
* **Smootherstep** (`6t⁵ - 15t⁴ + 10t³`) — ещё мягче, C² непрерывность (нет резких изломов).
* **Cosine** (`0.5 - 0.5cos(πt)`) — мягкая синусоида.

---

## Интерполяции

### 1D (скаляры)

* **Lerp** — линейная интерполяция.
* **Cubic Hermite** — кубическая кривая с заданными касательными.
* **Catmull-Rom** — сплайн через точки, касательные считаются автоматически.

### 2D / 3D (вектора)

* Те же типы (Lerp, Hermite, Catmull-Rom), только для `Vector2` и `Vector3`.
* Поддерживается **centripetal Catmull-Rom** (устойчивее при неравномерных точках).
* Есть **loop-режим** (замкнутый контур).

---

## Классы

### `Interpolation`

Базовые функции:

```csharp
Interpolation.Lerp(a, b, t, fade);
Interpolation.CubicHermite(p0, p1, m0, m1, t, fade);
Interpolation.CatmullRom(p0, p1, p2, p3, t, fade);
Interpolation.Bilerp(...); // 2D
Interpolation.Trilerp(...); // 3D
```

---

### `Spline1D`

Кривая по массиву `float[]`.

```csharp
var spline = new Spline1D(new float[]{0, 1, 0.5f, 1})
                 .WithFade(FadeCurveType.Smootherstep);

float value = spline.Evaluate(0.3f, InterpKind.CatmullRom);
```

---

### `Spline2D`

Кривая по `Vector2[]`. Используется для путей на плоскости.

Особенности:

* `Loop` — замкнутый контур.
* `Tau` — натяжение сплайна (0.5 по умолчанию).
* `Centripetal` — безопасный режим для неравномерных точек.
* `Evaluate(u, kind)` — обычная выборка (u∈\[0,1]).
* `EvaluateByArcLength(u, kind)` — выборка равномерно по длине дуги.

Пример:

```csharp
var points = new Vector2[]{ new(0,0), new(1,2), new(3,1) };
var spline2D = new Spline2D(points, loop:false, tau:0.5f, fade:FadeCurveType.Smoothstep);

Vector2 pos = spline2D.Evaluate(0.5f, InterpKind.CatmullRom);
```

---

### `Spline3D`

Аналогично `Spline2D`, но для `Vector3[]`. Подходит для путей в пространстве (например, движения камеры).

Пример:

```csharp
var spline3D = new Spline3D(new[]{
    new Vector3(0,0,0),
    new Vector3(1,1,0),
    new Vector3(2,0,1)
}, loop:true);

Vector3 pos = spline3D.EvaluateByArcLength(0.25f, InterpKind.CatmullRom);
```

---

## Когда что использовать

* **Lerp** — просто и быстро, но с «ломаными» углами.
* **Smoothstep / Smootherstep** — когда нужен мягкий шум или плавные переходы.
* **Catmull-Rom** — естественные траектории через точки.
* **Centripetal Catmull-Rom** — если точки далеко/близко, чтобы избежать петель.
* **EvaluateByArcLength** — когда важна равномерная скорость вдоль кривой.

---

## TL;DR

* `Interpolation` — примитивы.
* `Spline1D` — графики и профили.
* `Spline2D` — пути на плоскости.
* `Spline3D` — траектории в 3D.
* Всегда можно выбрать fade-функцию для нужной мягкости.
