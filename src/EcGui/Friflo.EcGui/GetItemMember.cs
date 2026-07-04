using System;

namespace Friflo.EcGui;

internal delegate bool GetItemMember<TMember>(in DrawValue drawValue, out TMember value, out Exception exception);
