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
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Jobs
{
    /// <summary>
    /// Handles collision of execution of jobs to copy data from Smint.io platform to sync targets.
    /// </summary>
    ///
    /// <remarks>
    /// ## Job runs
    /// <para>
    /// Jobs to copy from Smint.io to sync targets are not allowed to run simultaneously. It simple does not make sense.
    /// So new events to trigger a new job run, add the new run to a waiting queue in case a job is currently running.
    /// More jobs are just ignored, as the already waiting job will do the same task. Hence the waiting queue just
    /// consists of a single slot.
    /// </para>
    ///
    /// <para>
    /// Because jobs triggered by receiving a push event from <a href="htts://www.pusher.com">Pusher.com</a> do not sync
    /// meta data by default, scheduled jobs are not rejected in case the last item in the queue is a push event, but
    /// the queue reserves a special, additional slot for scheduled jobs for that.
    /// </para>
    /// </remarks>
    internal interface ISyncJobExecutionQueue
    {

        /// <summary>
        /// Adds a scheduled job or put it in the waiting queue if a slot is available.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// If another scheduled job is already waiting, this job is being silently ignored and no action is taken. If
        /// just a <em>push-event-job</em> is waiting, this job will be added as a second waiting job to a special slot.
        /// </para>
        ///
        /// </remarks>
        /// <param name="job">the async job to run.</param>
        /// <returns><c>this</c></returns>
        ISyncJobExecutionQueue AddJobForScheduleEvent(Func<Task> job);


        /// <summary>
        /// Adds a job because of a push event or add it to the waiting queue.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// If another job is already waiting, this job is being silently ignored and no action is taken.
        /// </para>
        ///
        /// </remarks>
        /// <param name="job">the async job to run.</param>
        /// <returns><c>this</c></returns>
        ISyncJobExecutionQueue AddJobForPushEvent(Func<Task> job);

        /// <summary>
        /// Adds a job.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// Calls <see cref="AddJobForPushEvent(Job)" /> if parameter <c>isPushEventJob</c> is <c>true</c> and calls
        /// <see cref="AddJobForScheduleEvent(Job)" /> otherwise.
        /// </para>
        ///
        /// </remarks>
        /// <param name="isPushEventJob">whether this is job, that has been triggered by a push event, or not.</param>
        /// <param name="job">the async job to run.</param>
        /// <returns><c>this</c></returns>
        ISyncJobExecutionQueue AddJob(bool isPushEventJob, Func<Task> job);


        /// <summary>
        /// Execute the next waiting job if no other job is running.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// The function returns immediately without executing anything, if a job is already running.
        /// </para>
        /// </remarks>
        Task<ISyncJobExecutionQueue> RunAsync();


        /// <summary>
        /// Checks whether a job has already been added to the waiting queue.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// The call is non-blocking.
        /// </para>
        /// </remarks>
        /// <returns><c>true</c> if at least one job is waiting or <c>false</c> otherwise.</returns>
        bool HasWaitingJob();


        /// <summary>
        /// Checks whether a job is currently being executed.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// The call is non-blocking.
        /// </para>
        /// </remarks>
        /// <returns><c>true</c> if at least one job is waiting or <c>false</c> otherwise.</returns>
        bool IsRunning();
    }
}
