using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	[ProtoContract]
	public class Message
	{

		private static System.Diagnostics.Stopwatch mWatch;

		private static System.Diagnostics.Stopwatch GetWatch()
		{
			if (mWatch == null)
			{
				mWatch = new Stopwatch();
				mWatch.Restart();

			}
			return mWatch;
		}


		public Message()
		{
			ID = System.Threading.Interlocked.Increment(ref mID);
			Mode = ReceiveMode.Eq;
			IsLocal = true;
		}

		private static long mID = 0;

		[ProtoMember(1)]
		public long ID { get; set; }

		[ProtoMember(2)]
		public string Pulisher { get; set; }

		[ProtoMember(3)]
		public string Consumers { get; set; }

		[ProtoMember(4)]
		public string DataType { get; set; }

		[ProtoMember(5)]
		public ReceiveMode Mode { get; set; }


		private object mData;

		public object Data
		{
			get
			{
				return mData;
			}
			set
			{
				mData = value;
			}
		}

		public void Reply(object body)
		{
			Message result = new Message();
			result.ID = ID;
			result.Consumers = Pulisher;
			result.Pulisher = Consumers;
			result.Data = body;
			result.Track("reply message");
			Subscriber.Publish(result);
			result.EndTrack("reply message completed!", Subscriber.Node);
		}


		private bool mIsLocal;

		internal bool IsLocal
		{
			get
			{
				return mIsLocal;

			}
			set
			{
				mIsLocal = value;
			}
		}

		public Message Copy()
		{
			Message result = new Message();
			result.ID = this.ID;
			result.Pulisher = this.Pulisher;
			result.Consumers = this.Consumers;
			result.DataType = DataType;
			result.Mode = this.Mode;
			result.Data = Data;
			result.Subscriber = this.Subscriber;
			return result;

		}


		private ISubscriber mSubscriber;

		internal ISubscriber Subscriber
		{
			get
			{
				return mSubscriber;
			}
			set
			{
				mSubscriber = value;
			}
		}

		internal void ProcessError(Exception error)
		{
			if (AsyncResult != null)
			{
				AsyncResult.Completed(null, error);
			}
		}

		private PublishResult mAsyncResult;

		internal PublishResult AsyncResult
		{
			get
			{
				return mAsyncResult;
			}
			set
			{
				mAsyncResult = value;
			}

		}

		#region track process time

		class TrackItem
		{
			public string Name { get; set; }
			public double Time { get; set; }
		}

		private Queue<TrackItem> mTracks = null;

		[Conditional("DEBUG")]
		public void Track(string name)
		{
			if (mTracks == null)
			{
				mTracks = new Queue<SmartRoute.Message.TrackItem>();

			}
			TrackItem item = new SmartRoute.Message.TrackItem { Name = name, Time = GetWatch().Elapsed.TotalMilliseconds };
			mTracks.Enqueue(item);
		}
		[Conditional("DEBUG")]
		public void EndTrack(string name, INode node)
		{
			Track(name);
			int i = 0;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			while (mTracks.Count > 0)
			{
				TrackItem item = mTracks.Dequeue();
				if (i == 0)
					sb.AppendFormat("{0}\t time:{1}\r\n", item.Name, item.Time);
				else
					sb.AppendFormat("\t\t\t{0}\t time:{1}\r\n", item.Name, item.Time);
				i++;
			}
			node.Loger.Process(LogType.MESSAGE_DEBUG, sb.ToString());
		}

		#endregion
	}

	public enum ReceiveMode : int
	{
		Eq = 0,
		NotEq = 2,
		All = 4,
		Regex = 8
	}
}
