namespace Soap.If.MessagePipeline.MessageAggregator
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Soap.If.Utility.PureFunctions;

    public class MessagePropogator<TMessage> : IPropogateMessages<TMessage> where TMessage : IMessage
    {
        private readonly TMessage message;

        internal MessagePropogator(TMessage message)
        {
            this.message = message;
        }

        public void To(Action<TMessage> passTo)
        {
            CaptureStartTimestamp(this.message as IStateOperation);

            passTo(this.message);

            CaptureStopTimestampAndCalculateLatency(this.message as IStateOperation);
        }

        public TOut To<TOut>(Func<TMessage, TOut> passTo)
        {
            CaptureStartTimestamp(this.message as IStateOperation);

            var result = passTo(this.message);

            if (result is Task)
            {
                (result as Task).ContinueWith(_ => CaptureStopTimestampAndCalculateLatency(this.message as IStateOperation));
            }
            else
            {
                CaptureStopTimestampAndCalculateLatency(this.message as IStateOperation);
            }

            return result;
        }

        private void CaptureStartTimestamp(IStateOperation stateOperation)
        {
            if (stateOperation == null) return;

            stateOperation.StateOperationStartTimestamp = StopwatchUtil.GetStopwatchTimestamp();
        }

        private void CaptureStopTimestampAndCalculateLatency(IStateOperation stateOperation)
        {
            if (stateOperation == null) return;

            stateOperation.StateOperationStopTimestamp = StopwatchUtil.GetStopwatchTimestamp();
            stateOperation.StateOperationDuration = StopwatchUtil.CalculateLatency(
                                                        stateOperation.StateOperationStartTimestamp,
                                                        stateOperation.StateOperationStopTimestamp) ?? TimeSpan.Zero;
        }
    }
}