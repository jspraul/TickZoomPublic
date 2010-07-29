using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ICSharpCode.Profiler.Controller.Data;
using ICSharpCode.Profiler.Interprocess;
using Microsoft.Win32;
namespace ICSharpCode.Profiler.Controller
{
	unsafe public sealed partial class Profiler : IDisposable
	{
		[DllImport("Hook64.dll", EntryPoint = "rdtsc"), System.Security.SuppressUnmanagedCodeSecurityAttribute()]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
		static extern void Rdtsc64(out ulong value);
		void InitializeHeader64()
		{
			memHeader64 = (SharedMemoryHeader64*)fullView.Pointer;
			#if DEBUG
			memHeader64->Magic = 0x7e444247;
			#else
			memHeader64->Magic = 0x7e534d31;
			#endif
			memHeader64->TotalLength = profilerOptions.SharedMemorySize;
			memHeader64->NativeToManagedBufferOffset = Align(sizeof(SharedMemoryHeader64));
			memHeader64->ThreadDataOffset = Align(memHeader64->NativeToManagedBufferOffset + bufferSize);
			memHeader64->ThreadDataLength = threadDataSize;
			memHeader64->HeapOffset = Align(memHeader64->ThreadDataOffset + threadDataSize);
			memHeader64->HeapLength = profilerOptions.SharedMemorySize - memHeader64->HeapOffset;
			memHeader64->ProcessorFrequency = GetProcessorFrequency();
			memHeader64->DoNotProfileDotnetInternals = profilerOptions.DoNotProfileDotNetInternals;
			memHeader64->CombineRecursiveFunction = profilerOptions.CombineRecursiveFunction;
			if ((Int64)(fullView.Pointer + memHeader64->HeapOffset) % 8 != 0) {
				throw new DataMisalignedException("Heap is not aligned properly: " + ((Int64)(fullView.Pointer + memHeader64->HeapOffset)).ToString(CultureInfo.InvariantCulture) + "!");
			}
		}
		void CollectData64()
		{
			if (TranslatePointer(memHeader64->RootFuncInfoAddress) == null)
				return;
			ulong now = GetRdtsc();
			ThreadLocalData64* item = (ThreadLocalData64*)TranslatePointer(this.memHeader64->LastThreadListItem);
			List<Stack<int>> stackList = new List<Stack<int>>();
			while (item != null) {
				StackEntry64* entry = (StackEntry64*)TranslatePointer(item->Stack.Array);
				Stack<int> itemIDs = new Stack<int>();
				while (entry != null && entry <= (StackEntry64*)TranslatePointer(item->Stack.TopPointer)) {
					FunctionInfo* function = (FunctionInfo*)TranslatePointer(entry->Function);
					itemIDs.Push(function->Id);
					function->TimeSpent += now - entry->StartTime;
					entry++;
				}
				stackList.Add(itemIDs);
				item = (ThreadLocalData64*)TranslatePointer(item->Predecessor);
			}
			if (this.enableDC) {
				this.AddDataset(fullView.Pointer, memHeader64->NativeAddress + memHeader64->HeapOffset, memHeader64->Allocator.startPos - memHeader64->NativeAddress, memHeader64->Allocator.pos - memHeader64->Allocator.startPos, (cpuUsageCounter == null) ? 0 : cpuUsageCounter.NextValue(), isFirstDC, memHeader64->RootFuncInfoAddress);
				isFirstDC = false;
			}
			ZeroMemory(new IntPtr(TranslatePointer(memHeader64->Allocator.startPos)), new IntPtr(memHeader64->Allocator.pos - memHeader64->Allocator.startPos));
			memHeader64->Allocator.pos = memHeader64->Allocator.startPos;
			Allocator64.ClearFreeList(&memHeader64->Allocator);
			FunctionInfo* root = CreateFunctionInfo(0, 0, stackList.Count);
			memHeader64->RootFuncInfoAddress = TranslatePointerBack64(root);
			item = (ThreadLocalData64*)TranslatePointer(this.memHeader64->LastThreadListItem);
			now = GetRdtsc();
			foreach (Stack<int> thread in stackList) {
				FunctionInfo* child = null;
				StackEntry64* entry = (StackEntry64*)TranslatePointer(item->Stack.TopPointer);
				while (thread.Count > 0) {
					FunctionInfo* stackItem = CreateFunctionInfo(thread.Pop(), 0, child != null ? 1 : 0);
					if (child != null)
						FunctionInfo.AddOrUpdateChild64(stackItem, child, this);
					entry->Function = TranslatePointerBack64(stackItem);
					entry->StartTime = now;
					entry--;
					child = stackItem;
				}
				if (child != null)
					FunctionInfo.AddOrUpdateChild64(root, child, this);
				item = (ThreadLocalData64*)TranslatePointer(item->Predecessor);
			}
		}
		unsafe void* Malloc64(int bytes)
		{
			#if DEBUG
			const int debuggingInfoSize = 8;
			bytes += debuggingInfoSize;
			#endif
			void* t = TranslatePointer(memHeader64->Allocator.pos);
			memHeader64->Allocator.pos += bytes;
			#if DEBUG
			t = (byte*)t + debuggingInfoSize;
			((Int64*)t)[-1] = bytes - debuggingInfoSize;
			#endif
			return t;
		}
		bool AllThreadsWait64()
		{
			this.threadListMutex.WaitOne();
			bool isWaiting = true;
			ThreadLocalData64* item = (ThreadLocalData64*)TranslatePointer(this.memHeader64->LastThreadListItem);
			while (item != null) {
				if (item->InLock == 1)
					isWaiting = false;
				item = (ThreadLocalData64*)TranslatePointer(item->Predecessor);
			}
			this.threadListMutex.ReleaseMutex();
			return isWaiting;
		}
		unsafe internal void* TranslatePointer64(TargetProcessPointer64 ptr)
		{
			if (ptr.Pointer == 0)
				return null;
			unchecked {
				Int64 spaceDiff = (Int64)(new IntPtr(fullView.Pointer)) - (Int64)memHeader64->NativeAddress.Pointer;
				return new IntPtr((Int64)ptr.Pointer + spaceDiff).ToPointer();
			}
		}
		unsafe internal TargetProcessPointer64 TranslatePointerBack64(void* ptr)
		{
			if (ptr == null)
				return new TargetProcessPointer64();
			unchecked {
				Int64 spaceDiff = (Int64)(new IntPtr(fullView.Pointer)) - (Int64)memHeader64->NativeAddress.Pointer;
				TargetProcessPointer64 pointer = new TargetProcessPointer64();
				pointer.Pointer = (UInt64)((Int64)ptr - spaceDiff);
				return pointer;
			}
		}
		#region IDisposable Member
		#endregion
		#region UnmanagedProfilingDataSet implementation
		#endregion
	}
}
