#region copyright
// MIT License
//
// Copyright (c) 2019 Smint.io GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice (including the next paragraph) shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// SPDX-License-Identifier: MIT
#endregion

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Jobs.Impl
{
    internal class DefaultSyncJobExecutionQueue : ISyncJobExecutionQueue
    {
        private static readonly int JobQueueLength = 1;
        private static readonly int JobQueueExtraSlots = 1;
        private static readonly int JobQueueMaxLength = JobQueueLength + JobQueueExtraSlots;

        private JobDescription _runningJob;
        private readonly ConcurrentQueue<JobDescription> _jobWaitingQueue = new ConcurrentQueue<JobDescription>();
        private readonly object _runningJobSemaphore = new object();

        public ISyncJobExecutionQueue AddJobForScheduleEvent(Func<Task> job)
        {
            if (job == null)
            {
                return this;
            }

            lock (_jobWaitingQueue)
            {
                if (_jobWaitingQueue.Count >= JobQueueMaxLength)
                {
                    return this;

                }
                else if (_jobWaitingQueue.Count == JobQueueLength)
                {
                    // check for the last element to be a job triggered by push event

                    JobDescription lastJob = null;

                    if (_jobWaitingQueue.TryPeek(out lastJob))
                    {
                        if (lastJob != null && !lastJob.IsPushEventJob)
                        {
                            return this;
                        }
                    }
                }

                // add this job
                _jobWaitingQueue.Enqueue(new JobDescription()
                {
                    Job = job,
                    IsPushEventJob = false
                });

                TryRunNextJob();
            }

            return this;
        }

        public ISyncJobExecutionQueue AddJobForPushEvent(Func<Task> job)
        {
            if (job == null)
            {
                return this;
            }

            lock (_jobWaitingQueue)
            {
                if (_jobWaitingQueue.Count >= JobQueueLength)
                {
                    return this;
                }

                _jobWaitingQueue.Enqueue(new JobDescription()
                {
                    Job = job,
                    IsPushEventJob = true
                });

                TryRunNextJob();
            }

            return this;
        }

        public ISyncJobExecutionQueue AddJob(bool isPushEventJob, Func<Task> job)
        {
            if (isPushEventJob)
            {
                return AddJobForPushEvent(job);
            }
            else
            {
                return AddJobForScheduleEvent(job);
            }
        }

        private void TryRunNextJob()
        {
            Task.Run(async () =>
            {
                await RunAsync();
            });
        }

        public async Task<ISyncJobExecutionQueue> RunAsync()
        {
            if (IsRunning())
            {
                return this;
            }

            JobDescription nextJob = null;
            lock (_jobWaitingQueue)
            {
                if (!_jobWaitingQueue.TryDequeue(out nextJob))
                {
                    return this;
                }
            }

            lock (_runningJobSemaphore)
            {
                _runningJob = nextJob;
            }

            // run outside of semaphore to avoid blocking other calls to run()
            JobDescription currentJob = nextJob;
            try
            {
                await currentJob.Job.Invoke();
            }
            finally
            {
                // remove the "running" flag AFTER reading all consumers waiting for notification
                lock (_runningJobSemaphore)
                {
                    if (_runningJob == currentJob)
                    {
                        _runningJob = null;
                    }
                }
            }

            TryRunNextJob();

            return this;
        }

        public bool HasWaitingJob()
        {
            lock (_jobWaitingQueue)
            {
                return !_jobWaitingQueue.IsEmpty;
            }
        }

        public bool IsRunning()
        {
            lock (_runningJobSemaphore)
            {
                return _runningJob != null;
            }
        }

        private class JobDescription
        {
            public Func<Task> Job { get; set; }
            public bool IsPushEventJob { get; set; }
        }
    }
}
