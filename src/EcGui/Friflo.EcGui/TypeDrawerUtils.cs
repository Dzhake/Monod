using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Friflo.Engine.ECS;

namespace Friflo.EcGui;

internal static class TypeDrawerUtils
{
	private static readonly Dictionary<Type, TypeDrawerEntry[]> Map = new Dictionary<Type, TypeDrawerEntry[]>
	{
		{
			typeof(string),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new StringDrawer())
			}
		},
		{
			typeof(char),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new CharDrawer())
			}
		},
		{
			typeof(bool),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new BoolDrawer())
			}
		},
		{
			typeof(DateTime),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new DateTimeDrawer())
			}
		},
		{
			typeof(byte),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new UInt8Drawer())
			}
		},
		{
			typeof(ushort),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new UInt16Drawer())
			}
		},
		{
			typeof(uint),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new UInt32Drawer())
			}
		},
		{
			typeof(ulong),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new UInt64Drawer())
			}
		},
		{
			typeof(sbyte),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new Int8Drawer())
			}
		},
		{
			typeof(short),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new Int16Drawer())
			}
		},
		{
			typeof(int),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new Int32Drawer())
			}
		},
		{
			typeof(long),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new Int64Drawer())
			}
		},
		{
			typeof(float),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new Float32Drawer())
			}
		},
		{
			typeof(double),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new Float64Drawer())
			}
		},
		{
			typeof(decimal),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new DecimalDrawer())
			}
		},
		{
			typeof(Vector2),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new VectorFloat2Drawer())
			}
		},
		{
			typeof(Vector3),
			new TypeDrawerEntry[2]
			{
				new TypeDrawerEntry(new VectorFloat3Drawer()),
				new TypeDrawerEntry("color", new Color3Drawer())
			}
		},
		{
			typeof(Vector4),
			new TypeDrawerEntry[2]
			{
				new TypeDrawerEntry(new VectorFloat4Drawer()),
				new TypeDrawerEntry("color", new Color4Drawer())
			}
		},
		{
			typeof(Quaternion),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new QuaternionDrawer())
			}
		},
		{
			typeof(Entity),
			new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(new EntityDrawer())
			}
		}
	};

	private const BindingFlags GetMethodFlags = BindingFlags.Static | BindingFlags.NonPublic;

	internal static void RegisterTypeDrawer(Type type, TypeDrawer drawer, string? drawerName)
	{
		TypeDrawerEntry typeDrawerEntry = new TypeDrawerEntry(drawerName, drawer);
		Dictionary<Type, TypeDrawerEntry[]> map = Map;
		if (map.TryGetValue(type, out var value))
		{
			TypeDrawerEntry[] array = new TypeDrawerEntry[value.Length + 1];
			value.CopyTo(array, 0);
			array[value.Length] = typeDrawerEntry;
			map[type] = array;
		}
		else
		{
			map.Add(type, new TypeDrawerEntry[1] { typeDrawerEntry });
		}
	}

	internal static TypeDrawer GetTypeDrawer(Type type, string? drawerName)
	{
		Dictionary<Type, TypeDrawerEntry[]> map = Map;
		if (map.TryGetValue(type, out var value))
		{
			if (value.Length == 1)
			{
				return value[0].drawer;
			}
			TypeDrawerEntry[] array = value;
			for (int i = 0; i < array.Length; i++)
			{
				TypeDrawerEntry typeDrawerEntry = array[i];
				if (typeDrawerEntry.name == drawerName)
				{
					return typeDrawerEntry.drawer;
				}
			}
			return value[0].drawer;
		}
		if (type.IsEnum)
		{
			EnumDrawer enumDrawer = new EnumDrawer(type);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(enumDrawer)
			});
			return enumDrawer;
		}
		if (type.IsArray)
		{
			TypeDrawer typeDrawer = (TypeDrawer)typeof(ArrayDrawer<>).MakeGenericType(type.GetElementType()).GetMethod("CreateArrayDrawer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(typeDrawer)
			});
			return typeDrawer;
		}
		Type[] interfaces = type.GetInterfaces();
		if (IsGenericDictionary(interfaces, typeof(IDictionary<, >), out Type[] genericTypeArgs))
		{
			Type type2 = genericTypeArgs[0];
			Type type3 = genericTypeArgs[1];
			TypeDrawer typeDrawer2 = (TypeDrawer)typeof(DictionaryDrawer<, , >).MakeGenericType(type, type2, type3).GetMethod("CreateDictionaryDrawer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(typeDrawer2)
			});
			return typeDrawer2;
		}
		if (IsGenericTypeOf(interfaces, typeof(IList<>), out Type genericTypeArg))
		{
			Type type4 = genericTypeArg;
			TypeDrawer typeDrawer3 = (TypeDrawer)typeof(ListDrawer<, >).MakeGenericType(type, type4).GetMethod("CreateListDrawer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(typeDrawer3)
			});
			return typeDrawer3;
		}
		if (IsGenericTypeOf(interfaces, typeof(ISet<>), out genericTypeArg))
		{
			Type type5 = genericTypeArg;
			TypeDrawer typeDrawer4 = (TypeDrawer)typeof(SetDrawer<, >).MakeGenericType(type, type5).GetMethod("CreateSetDrawer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(typeDrawer4)
			});
			return typeDrawer4;
		}
		if (IsGenericTypeOf(interfaces, typeof(ICollection<>), out genericTypeArg))
		{
			Type type6 = genericTypeArg;
			TypeDrawer typeDrawer5 = (TypeDrawer)typeof(CollectionDrawer<, >).MakeGenericType(type, type6).GetMethod("CreateCollectionDrawer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(typeDrawer5)
			});
			return typeDrawer5;
		}
		if (IsGenericTypeOf(interfaces, typeof(IEnumerable<>), out genericTypeArg))
		{
			Type type7 = genericTypeArg;
			TypeDrawer typeDrawer6 = (TypeDrawer)typeof(EnumerableDrawer<, >).MakeGenericType(type, type7).GetMethod("CreateEnumerableDrawer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(typeDrawer6)
			});
			return typeDrawer6;
		}
		if (type.IsClass || type.IsInterface)
		{
			TypeDrawer typeDrawer7 = ObjectDrawerUtils.CreateClassDrawer(type);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(typeDrawer7)
			});
			return typeDrawer7;
		}
		if (type.IsValueType)
		{
			if (IsRelationsType(type, out Type itemType, out Type keyType))
			{
				TypeDrawer typeDrawer8 = (TypeDrawer)typeof(RelationsDrawer<, >).MakeGenericType(itemType, keyType).GetMethod("CreateRelationDrawer", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
				map.Add(type, new TypeDrawerEntry[1]
				{
					new TypeDrawerEntry(typeDrawer8)
				});
				return typeDrawer8;
			}
			StructDrawer structDrawer = new StructDrawer(type);
			map.Add(type, new TypeDrawerEntry[1]
			{
				new TypeDrawerEntry(structDrawer)
			});
			return structDrawer;
		}
		UnsupportedTypeDrawer unsupportedTypeDrawer = new UnsupportedTypeDrawer(type);
		map.Add(type, new TypeDrawerEntry[1]
		{
			new TypeDrawerEntry(unsupportedTypeDrawer)
		});
		return unsupportedTypeDrawer;
	}

	private static bool IsGenericTypeOf(Type[] interfaces, Type interfaceType, out Type genericTypeArg)
	{
		foreach (Type type in interfaces)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
			{
				genericTypeArg = type.GenericTypeArguments[0];
				return true;
			}
		}
		genericTypeArg = null;
		return false;
	}

	private static bool IsGenericDictionary(Type[] interfaces, Type interfaceType, out Type[] genericTypeArgs)
	{
		foreach (Type type in interfaces)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
			{
				genericTypeArgs = type.GenericTypeArguments;
				return true;
			}
		}
		genericTypeArgs = Array.Empty<Type>();
		return false;
	}

	private static bool IsRelationsType(Type type, out Type itemType, out Type keyType)
	{
		if (type.IsGenericType && type.Namespace == "Friflo.Engine.ECS" && type.Name == "Relations`1")
		{
			itemType = type.GetGenericArguments()[0];
			Type[] interfaces = itemType.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IRelation<>))
				{
					keyType = type2.GetGenericArguments()[0];
					return true;
				}
			}
		}
		itemType = null;
		keyType = null;
		return false;
	}
}
