using Friflo.Engine.ECS;
using MLEM.Maths;
using Monod.TimeModule;
using Monod.Utils;

namespace Monod.ECS.Tweening;

public interface ILerper<T>
{
    public T Lerp(T from, T to, float time);
}

public struct Tween<TComponent, TField, TLerper> where TComponent : struct, IComponent where TLerper : ILerper<TField>, new()
{
    public Entity entity;

    public TField From;
    public TField To;

    public float CurrentTime;
    public float TotalTime;
    public bool Finished => CurrentTime >= TotalTime;

    public nint FieldOffset;
    public TLerper Lerper;

    public Easings.Easing EasingFunc;

    /// <summary>
    /// Create a new instance of <see cref="Tween{TLerper, TComponent, TField}"/> with set parameters.
    /// </summary>
    /// <param name="component">Component whose field to tween.</param>
    /// <param name="field">Reference to field, that should be tweened. Field must be in the <paramref name="component"/>.</param>
    /// <param name="entity">Entity whose component to tween.</param>
    /// <param name="to">Final value of the tween.</param>
    /// <param name="totalTime">Time of the full tween.</param>
    /// <param name="easingFunc"><see cref="Easings.Easing"/> to use for time. <see cref="Easings.Linear"/> by default.</param>
    /// <param name="lerper">Lerper to use.</param>
    public Tween(ref TComponent component, ref TField field, Entity entity, TField to, float totalTime, Easings.Easing? easingFunc = null, TLerper? lerper = default)
    {
        this.entity = entity;
        From = field;
        To = to;
        TotalTime = totalTime;
        FieldOffset = UnsafeUtils.GetFieldOffset(ref component, ref field);
        EasingFunc = easingFunc ?? Easings.Linear;
        Lerper = lerper ?? new();
    }

    public void Update()
    {
        CurrentTime += Time.DeltaTime;

        var data = entity.Data;
        if (!data.Has<TComponent>()) return;

        ref TComponent componentRef = ref data.Get<TComponent>();
        ref TField fieldRef = ref UnsafeUtils.GetField<TComponent, TField>(ref componentRef, FieldOffset);
        if (CurrentTime >= TotalTime)
        {
            fieldRef = To;
        }
            
        else
        {
            float time = CurrentTime / TotalTime;
            fieldRef = Lerper.Lerp(From, To, EasingFunc(time));
        }
    }
}