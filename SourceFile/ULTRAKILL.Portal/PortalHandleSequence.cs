using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ULTRAKILL.Portal;

public readonly struct PortalHandleSequence : IEquatable<PortalHandleSequence>, IEquatable<PortalHandle>, IEnumerable<PortalHandle>, IEnumerable, IEquatable<PortalTraversalV2[]>
{
	private readonly PortalHandle[] handles;

	private readonly byte count;

	public static readonly PortalHandleSequence Empty;

	public int Count => count;

	public bool IsEmpty => count <= 0;

	public PortalHandle this[int index]
	{
		get
		{
			return handles[index];
		}
		set
		{
			handles[index] = value;
		}
	}

	public static PortalHandleSequence OfDepth(int depth)
	{
		return new PortalHandleSequence(new PortalHandle[depth]);
	}

	public PortalHandleSequence Then(PortalScene scene, PortalHandle handle)
	{
		if (handles == null || handles.Length == 0)
		{
			return new PortalHandleSequence(handle);
		}
		if (!IsEmpty)
		{
			PortalHandleSequence portalHandleSequence = this;
			if (portalHandleSequence[portalHandleSequence.Count - 1] == handle)
			{
				return this;
			}
			portalHandleSequence = this;
			if (portalHandleSequence[portalHandleSequence.Count - 1].Reverse() == handle)
			{
				if (Count != 1)
				{
					return new PortalHandleSequence(this.Take(Count - 1).ToArray());
				}
				return Empty;
			}
		}
		return new PortalHandleSequence(handles.Append(handle).ToArray());
	}

	public Span<PortalHandle> AsSpan()
	{
		return handles.AsSpan();
	}

	public PortalHandleSequence Append(PortalHandle handle)
	{
		if (handles == null || handles.Length == 0)
		{
			return new PortalHandleSequence(handle);
		}
		PortalHandle[] array = new PortalHandle[count + 1];
		handles.AsSpan().CopyTo(array);
		array[^1] = handle;
		return new PortalHandleSequence(array);
	}

	public PortalHandleSequence StartFrom(PortalScene scene, PortalHandle handle)
	{
		if (handles == null || handles.Length == 0)
		{
			return new PortalHandleSequence(handle);
		}
		if (!IsEmpty)
		{
			if (this[0] == handle)
			{
				return this;
			}
			if (this[0].Reverse() == handle)
			{
				if (Count != 1)
				{
					return new PortalHandleSequence(this.Skip(1).ToArray());
				}
				return Empty;
			}
		}
		return new PortalHandleSequence(handles.Prepend(handle).ToArray());
	}

	public PortalHandleSequence Prepend(PortalHandle handle)
	{
		if (handles == null || handles.Length == 0)
		{
			return new PortalHandleSequence(handle);
		}
		return new PortalHandleSequence(handles.Prepend(handle).ToArray());
	}

	public PortalHandleSequence RemoveLast()
	{
		if (Count == 0)
		{
			return this;
		}
		if (Count == 1)
		{
			return Empty;
		}
		PortalHandle[] array = handles;
		Array.Resize(ref array, Count - 1);
		return new PortalHandleSequence(array);
	}

	public PortalHandleSequence(params PortalTraversalV2[] traversals)
	{
		if (traversals == null || traversals.Length == 0)
		{
			handles = Array.Empty<PortalHandle>();
			count = 0;
			return;
		}
		handles = new PortalHandle[traversals.Length];
		count = (byte)traversals.Length;
		for (int i = 0; i < traversals.Length; i++)
		{
			handles[i] = traversals[i].portalHandle;
		}
	}

	public PortalHandleSequence(params PortalHandle[] handles)
	{
		if (handles == null || handles.Length == 0)
		{
			this.handles = Array.Empty<PortalHandle>();
			count = 0;
		}
		else
		{
			this.handles = handles;
			count = (byte)this.handles.Length;
		}
	}

	public static PortalHandleSequence Prepend(PortalHandle handle, params PortalTraversalV2[] traversals)
	{
		PortalHandle[] array = new PortalHandle[traversals.Length + 1];
		array[0] = handle;
		for (int i = 0; i < traversals.Length; i++)
		{
			array[i + 1] = traversals[i].portalHandle;
		}
		return new PortalHandleSequence(array);
	}

	public bool MatchesTraversals(in PortalTraversalV2[] traversals)
	{
		if (traversals.Length != Count)
		{
			return false;
		}
		for (int i = 0; i < traversals.Length; i++)
		{
			PortalHandle portalHandle = traversals[i].portalHandle;
			PortalHandle portalHandle2 = handles[i];
			if (portalHandle.instanceId != portalHandle2.instanceId)
			{
				return false;
			}
			if (portalHandle.side == portalHandle2.side)
			{
				return false;
			}
		}
		return true;
	}

	public PortalHandleSequence Reversed()
	{
		if (handles == null || handles.Length == 0)
		{
			return this;
		}
		PortalHandle[] array = new PortalHandle[handles.Length];
		Span<PortalHandle> span = MemoryMarshal.CreateSpan(ref array[0], array.Length);
		Span<PortalHandle> span2 = MemoryMarshal.CreateSpan(ref handles[0], array.Length);
		for (int i = 0; i < span2.Length; i++)
		{
			int num = i + 1;
			ref PortalHandle reference = ref span2[span2.Length - num];
			ref PortalHandle reference2 = ref span[i];
			reference2.instanceId = reference.instanceId;
			reference2.side = ((reference.side != PortalSide.Enter) ? PortalSide.Enter : PortalSide.Exit);
		}
		return new PortalHandleSequence(array);
	}

	public IEnumerator<PortalHandle> GetEnumerator()
	{
		for (int i = 0; i < count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private int ComputeHashCode()
	{
		if (count <= 0)
		{
			return 0;
		}
		int num = 17;
		num = num * 31 + count.GetHashCode();
		for (int i = 0; i < count; i++)
		{
			PortalHandle portalHandle = handles[i];
			num = num * 31 + portalHandle.GetHashCode();
		}
		return num;
	}

	public override int GetHashCode()
	{
		return ComputeHashCode();
	}

	public bool Equals(PortalHandleSequence other)
	{
		if (count != other.count)
		{
			return false;
		}
		if (count <= 0)
		{
			return true;
		}
		for (int i = 0; i < count; i++)
		{
			if (!EqualityComparer<PortalHandle>.Default.Equals(this[i], other[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is PortalHandleSequence other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(PortalHandleSequence left, PortalHandleSequence right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PortalHandleSequence left, PortalHandleSequence right)
	{
		return !left.Equals(right);
	}

	public bool Equals(PortalHandle other)
	{
		if (count == 1)
		{
			return handles[0].Equals(other);
		}
		return false;
	}

	public bool Equals(PortalTraversalV2[] other)
	{
		if (other == null || other.Length != count)
		{
			return false;
		}
		for (int i = 0; i < count; i++)
		{
			if (this[i] != other[i].portalHandle)
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator ==(PortalHandleSequence left, PortalTraversalV2[] right)
	{
		if (right != null)
		{
			return left.Equals(right);
		}
		return left.IsEmpty;
	}

	public static bool operator !=(PortalHandleSequence left, PortalTraversalV2[] right)
	{
		if (right != null)
		{
			return !left.Equals(right);
		}
		return !left.IsEmpty;
	}

	public override string ToString()
	{
		if (!IsEmpty)
		{
			return string.Format("Count: {0}, Handles: [{1}]", Count, string.Join(", ", this));
		}
		return "Empty";
	}
}
