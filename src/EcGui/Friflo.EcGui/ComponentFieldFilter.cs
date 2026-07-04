using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal delegate bool ComponentFieldFilter<TComponent>(in TComponent component) where TComponent : struct, IComponent;
