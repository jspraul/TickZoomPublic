using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace ICSharpCode.Profiler.Controller.Data
{
	unsafe sealed class UnmanagedCallTreeNode64 : CallTreeNode
	{
		FunctionInfo* data;
		CallTreeNode parent;
		const ulong CpuCycleMask = 0x7fffffffffffffL;
		UnmanagedProfilingDataSet dataSet;
		internal UnmanagedCallTreeNode64(UnmanagedProfilingDataSet dataSet, FunctionInfo* data, CallTreeNode parent)
		{
			this.data = data;
			this.dataSet = dataSet;
			this.parent = parent;
		}
		public override System.Linq.IQueryable<CallTreeNode> Children {
			get {
				dataSet.VerifyAccess();
				List<UnmanagedCallTreeNode64> children = new List<UnmanagedCallTreeNode64>();
				TargetProcessPointer64* childrenPtr = FunctionInfo.GetChildren64(data);
				for (int i = 0; i <= data->LastChildIndex; i++) {
					FunctionInfo* child = dataSet.GetFunctionInfo(childrenPtr[i]);
					if (child != null)
						children.Add(new UnmanagedCallTreeNode64(dataSet, child, this));
				}
				children.Sort((a, b) => a.Index.CompareTo(b.Index));
				return children.Cast<CallTreeNode>().AsQueryable();
			}
		}
		public override NameMapping NameMapping {
			get { return this.dataSet.GetMapping(this.data->Id); }
		}
		public override int RawCallCount {
			get {
				dataSet.VerifyAccess();
				return this.data->CallCount;
			}
		}
		public int Index {
			get {
				dataSet.VerifyAccess();
				return (int)(this.data->TimeSpent >> 56);
			}
		}
		public override bool IsActiveAtStart {
			get {
				dataSet.VerifyAccess();
				return (this.data->TimeSpent & ((ulong)1 << 55)) != 0;
			}
		}
		public override long CpuCyclesSpent {
			get {
				dataSet.VerifyAccess();
				return (long)(this.data->TimeSpent & CpuCycleMask);
			}
		}
		public override CallTreeNode Parent {
			get { return this.parent; }
		}
		public override double TimeSpent {
			get { return this.CpuCyclesSpent / (1000.0 * this.dataSet.ProcessorFrequency); }
		}
		public override double TimeSpentSelf {
			get { return this.CpuCyclesSpentSelf / (1000.0 * this.dataSet.ProcessorFrequency); }
		}
		public override CallTreeNode Merge(IEnumerable<CallTreeNode> nodes)
		{
			throw new NotImplementedException();
		}
		public override IQueryable<CallTreeNode> Callers {
			get { return GetCallers().AsQueryable(); }
		}
		IEnumerable<CallTreeNode> GetCallers()
		{
			if (parent != null)
				yield return parent;
		}
		public override bool Equals(CallTreeNode other)
		{
			if (other is UnmanagedCallTreeNode64) {
				UnmanagedCallTreeNode64 node = other as UnmanagedCallTreeNode64;
				return node.data == this.data;
			}
			return false;
		}
		public override int GetHashCode()
		{
			return (new IntPtr(data)).GetHashCode();
		}
	}
}
