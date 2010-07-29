using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
namespace ICSharpCode.Profiler.Controller
{
	[StructLayout(LayoutKind.Sequential)]
	struct SharedMemoryHeader64
	{
		public int Magic;
		public volatile int ExclusiveAccess;
		public int TotalLength;
		public int NativeToManagedBufferOffset;
		public int ThreadDataOffset;
		public int ThreadDataLength;
		public int HeapOffset;
		public int HeapLength;
		public TargetProcessPointer64 NativeAddress;
		public TargetProcessPointer64 RootFuncInfoAddress;
		public TargetProcessPointer64 LastThreadListItem;
		public int ProcessorFrequency;
		public bool DoNotProfileDotnetInternals;
		public bool CombineRecursiveFunction;
		public Allocator64 Allocator;
	}
	[StructLayout(LayoutKind.Sequential)]
	unsafe struct Allocator64
	{
		public TargetProcessPointer64 startPos;
		public TargetProcessPointer64 pos;
		public TargetProcessPointer64 endPos;
		public fixed UInt64 freeList[32];
		public static void ClearFreeList(Allocator64* a)
		{
			for (int i = 0; i < 32; i++) {
				a->freeList[i] = 0;
			}
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	struct TargetProcessPointer64
	{
		public UInt64 Pointer;
		public override string ToString()
		{
			return "0x" + Pointer.ToString("x", CultureInfo.InvariantCulture);
		}
		public static implicit operator TargetProcessPointer(TargetProcessPointer64 p)
		{
			return new TargetProcessPointer(p);
		}
		public static TargetProcessPointer64 operator +(TargetProcessPointer64 p, Int64 offset)
		{
			unchecked {
				p.Pointer += (UInt64)offset;
			}
			return p;
		}
		public static TargetProcessPointer64 operator -(TargetProcessPointer64 p, Int64 offset)
		{
			unchecked {
				p.Pointer -= (UInt64)offset;
			}
			return p;
		}
		public static Int64 operator -(TargetProcessPointer64 ptr1, TargetProcessPointer64 ptr2)
		{
			unchecked {
				return (Int64)(ptr1.Pointer - ptr2.Pointer);
			}
		}
	}
	unsafe partial struct FunctionInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public static TargetProcessPointer64* GetChildren64(FunctionInfo* f)
		{
			if (f == null)
				throw new NullReferenceException();
			return (TargetProcessPointer64*)(f + 1);
		}
		public static void AddOrUpdateChild64(FunctionInfo* parent, FunctionInfo* child, Profiler profiler)
		{
			int slot = child->Id;
			while (true) {
				slot &= parent->LastChildIndex;
				FunctionInfo* slotContent = (FunctionInfo*)profiler.TranslatePointer(GetChildren64(parent)[slot]);
				if (slotContent == null || slotContent->Id == child->Id) {
					GetChildren64(parent)[slot] = profiler.TranslatePointerBack64(child);
					break;
				}
				slot++;
			}
		}
	}
	[StructLayout(LayoutKind.Sequential)]
	unsafe struct ThreadLocalData64
	{
		public int ThreadID;
		public volatile int InLock;
		public TargetProcessPointer64 Predecessor;
		public TargetProcessPointer64 Follower;
		public LightweightStack64 Stack;
	}
	[StructLayout(LayoutKind.Sequential)]
	unsafe struct LightweightStack64
	{
		public TargetProcessPointer64 Array;
		public TargetProcessPointer64 TopPointer;
		public TargetProcessPointer64 ArrayEnd;
	}
	[StructLayout(LayoutKind.Sequential)]
	unsafe struct StackEntry64
	{
		public TargetProcessPointer64 Function;
		public ulong StartTime;
		public int FrameCount;
	}
}
