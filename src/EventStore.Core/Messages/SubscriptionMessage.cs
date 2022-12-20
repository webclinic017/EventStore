using System;
using EventStore.Core.Messaging;

namespace EventStore.Core.Messages {
	public static partial class SubscriptionMessage {
		[DerivedMessage]
		public partial class PollStream : Message {
			public readonly string StreamId;
			public readonly long LastIndexedPosition;
			public readonly long? LastEventNumber;
			public readonly DateTime ExpireAt;

			public readonly Message OriginalRequest;

			public PollStream(string streamId, long lastIndexedPosition, long? lastEventNumber, DateTime expireAt,
				Message originalRequest) {
				StreamId = streamId;
				LastIndexedPosition = lastIndexedPosition;
				LastEventNumber = lastEventNumber;
				ExpireAt = expireAt;
				OriginalRequest = originalRequest;
			}
		}

		[DerivedMessage]
		public partial class CheckPollTimeout : Message {
		}

		[DerivedMessage]
		public partial class PersistentSubscriptionTimerTick : Message {
			public Guid CorrelationId { get; }

			public PersistentSubscriptionTimerTick(Guid correlationId) {
				CorrelationId = correlationId;
			}
		}
		
		[DerivedMessage]
		public partial class PersistentSubscriptionsRestart : Message {
			public IEnvelope ReplyEnvelope { get; }
			
			public PersistentSubscriptionsRestart(IEnvelope replyEnvelope) {
				ReplyEnvelope = replyEnvelope;
			}
		}

		[DerivedMessage]
		public partial class PersistentSubscriptionsRestarting : Message {
		}

		[DerivedMessage]
		public partial class InvalidPersistentSubscriptionsRestart : Message {
		}
	
		[DerivedMessage]
		public partial class PersistentSubscriptionsStarted : Message {
		}
		
		[DerivedMessage]
		public partial class PersistentSubscriptionsStopped : Message {
		}
	}
}
