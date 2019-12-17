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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Jobs.Impl
{

    internal class DefaultSyncJobExecutionQueue : ISyncJobExecutionQueue
    {

        private static readonly int JobQueueLength = 1;
        private static readonly int JobQueueExtraSlots = 1;
        private static readonly int JobQueueMaxLength = JobQueueLength + JobQueueExtraSlots;


        private JobDescription _runningJob;
        private readonly IList<JobDescription> _jobWaitingQueue = new List<JobDescription>();
        private readonly object _runningJobSemaphore = new object();

        private readonly IList<Action<bool>> _waitingForNotification = new List<Action<bool>>();

        public ISyncJobExecutionQueue AddJobForScheduleEvent(Job job)
        {
            if (job == null)
            {
                return this;
            }

            lock (this._jobWaitingQueue)
            {
                if (this._jobWaitingQueue.Count >= JobQueueMaxLength)
                {
                    return this;

                }
                else if (this._jobWaitingQueue.Count == JobQueueLength)
                {

                    // check for the last element to be a job triggered by push event
                    JobDescription lastJob = this._jobWaitingQueue[this._jobWaitingQueue.Count - 1];
                    if (lastJob != null && !lastJob.IsPushEventJob)
                    {
                        return this;
                    }
                }

                // add this job
                this._jobWaitingQueue.Add(new JobDescription().WithJob(job).WithIsPushEvent(false));
            }

            return this;

        }

        public ISyncJobExecutionQueue AddJobForPushEvent(Job job)
        {
            if (job == null)
            {
                return this;
            }

            lock (this._jobWaitingQueue)
            {
                if (this._jobWaitingQueue.Count >= JobQueueLength)
                {
                    return this;
                }

                this._jobWaitingQueue.Add(new JobDescription().WithJob(job).WithIsPushEvent(true));
            }

            return this;
        }

        public ISyncJobExecutionQueue AddJob(bool isPushEventJob, Job job)
        {
            if (isPushEventJob)
            {
                return this.AddJobForPushEvent(job);
            }
            else
            {
                return this.AddJobForScheduleEvent(job);
            }
        }


        public Task<ISyncJobExecutionQueue> RunAsync()
        {
            if (this.IsRunning())
            {
                return Task.FromResult<ISyncJobExecutionQueue>(this);
            }

            JobDescription nextJob = null;
            lock (this._jobWaitingQueue)
            {
                if (this._jobWaitingQueue.Count > 0)
                {
                    nextJob = this._jobWaitingQueue[0];
                    this._jobWaitingQueue.RemoveAt(0);
                }
            }

            if (nextJob?.Job == null)
            {
                return Task.FromResult<ISyncJobExecutionQueue>(this);
            }

            lock (this._runningJobSemaphore)
            {
                this._runningJob = nextJob;
            }


            // run outside of semaphore to avoid blocking other calls to run()
            JobDescription currentJob = nextJob;
            try
            {
                currentJob.Job.Invoke();
            }
            finally
            {
                lock (currentJob)
                {
                    Monitor.PulseAll(currentJob);
                }

                // notify all waiting callback of ending job
                if (this._waitingForNotification.Count != 0)
                {
                    IList<Action<bool>> notifications = new List<Action<bool>>();
                    foreach (var action in this._waitingForNotification)
                    {
                        notifications.Add(action);
                    }
                    this._waitingForNotification.Clear();

                    if (notifications.Count > 0)
                    {
                        Task.Run(() =>
                            {
                                foreach (var notify in notifications)
                                {
                                    try
                                    {
                                        notify.Invoke(!currentJob.IsPushEventJob);
                                    }
                                    catch (Exception)
                                    {
                                        // ignore
                                    }
                                }
                            }
                        );
                    }

                    // remove the "running" flag AFTER reading all consumers waiting for notification
                    lock (this._runningJobSemaphore)
                    {
                        if (this._runningJob == currentJob)
                        {
                            this._runningJob = null;
                        }
                    }
                }
            }

            return Task.FromResult<ISyncJobExecutionQueue>(this);
        }


        public bool HasWaitingJob()
        {
            lock (this._jobWaitingQueue)
            {
                return this._jobWaitingQueue.Count > 0;
            }
        }


        public bool IsRunning()
        {
            return this._jobWaitingQueue != null;
        }


        public ISyncJobExecutionQueue NotifyWhenFinished(Action<bool> callback)
        {
            if (callback == null)
            {
                return this;
            }

            bool executeImmediately = false;
            lock (this._runningJobSemaphore)
            {
                executeImmediately |= this._runningJob == null;
            }

            lock (this._jobWaitingQueue)
            {
                executeImmediately |= this._jobWaitingQueue.Count == 0;
            }

            if (executeImmediately)
            {
                Task.Run(() => callback.Invoke(true));

            }
            else
            {
                this._waitingForNotification.Add(callback);
            }

            return this;
        }


        private class JobDescription
        {

            public Job Job { get; private set; }
            public bool IsPushEventJob { get; private set; }

            public JobDescription WithJob(Job job)
            {
                this.Job = job;
                return this;
            }

            public JobDescription WithIsPushEvent(bool isPushEvent)
            {
                this.IsPushEventJob = isPushEvent;
                return this;
            }
        }
    }
}
