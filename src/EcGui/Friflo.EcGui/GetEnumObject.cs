using System;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal delegate object GetEnumObject(Entity entity, MemberPath memberPath, out Exception? exception);
