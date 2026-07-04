using System;

namespace Friflo.EcGui;

internal delegate bool SetItemMember<TMember>(in DrawValue drawValue, in TMember value, out Exception exception);
